using System;
using System.Collections.Generic;
using System.Linq;
using UniDecl.PropertyGrid.Runtime;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Widgets;
using UnityEditor;
using UnityEngine;
using PropertyField = UniDecl.PropertyGrid.Editor.Elements.PropertyField;
using EditorValueSync = UniDecl.PropertyGrid.Editor.Elements.EditorValueSync;
using TabSelectionMemory = UniDecl.PropertyGrid.Editor.Elements.TabSelectionMemory;

namespace UniDecl.PropertyGrid.Editor
{
    public static class WidgetFactory
    {
        public static IElement CreateTree(LayoutTree tree, BuildContext rootCtx) => BuildNode(tree.Root, rootCtx);

        static IElement BuildNode(ILayoutNode node, BuildContext ctx)
        {
            if (node is FieldLayoutNode f) return BuildField(f, ctx);
            if (node is ClassElementLayoutNode c) return BuildClass(c, ctx);
            if (node is ObjectLayoutNode o) return BuildObject(o, ctx);
            if (node is TabViewLayoutNode tv) return BuildTabView(tv, ctx);
            if (node is GroupLayoutNode g) return BuildGroup(g, ctx);
            return null;
        }

        static IElement BuildObject(ObjectLayoutNode node, BuildContext ctx)
        {
            var childCtx = ctx.CreateChild(node);
            var handler = GlobalPropertyGridRegistry.FindByNode(node) ?? new ObjectLayoutHandler();
            var container = (ContainerElement)handler.Build(node, childCtx);
            // 子节点顺序已在 LayoutResolver.Normalize 统一排序，此处按序直出
            foreach (var c in node.Children) { var el = BuildNode(c, childCtx); if (el != null) container.Add(el); }
            return container;
        }

        static IElement BuildGroup(GroupLayoutNode node, BuildContext ctx)
        {
            var handler = GlobalPropertyGridRegistry.FindByNode(node);
            ContainerElement container;
            if (handler != null) { var b = handler.Build(node, ctx); container = b as ContainerElement; if (container == null && b != null) return b; }
            else { container = new VerticalLayout(); container.WithKey($"group_{node.Path}"); }
            // P2-2: 组级 StyleClass 通过 Key 前缀携带，渲染层解析（Element 无 StyleClass 字段）
            if (!string.IsNullOrEmpty(node.StyleClass))
                container.WithKey($"{container.Key}#{node.StyleClass.Replace(' ', '_')}");
            foreach (var c in node.Children) { var el = BuildNode(c, ctx); if (el != null) container.Add(el); }
            return container;
        }

        /// <summary>
        /// 页签视图——Normalize 合成的连续 TabGroup 容器。
        /// 页内容直出（不再包一层 Foldout），页标题取自 Tab 节点的 DisplayName（类级 Title）。
        /// 页选择经 TabSelectionMemory 跨整树重建持久化（交互会触发根级重建）。
        /// </summary>
        static IElement BuildTabView(TabViewLayoutNode node, BuildContext ctx)
        {
            var keyPaths = node.Children.Select(c => c.Path).ToArray();
            var viewKey = $"tabview_{string.Join("|", keyPaths)}";
            var tabs = new Tabs { SelectedIndex = TabSelectionMemory.Get(viewKey, node.SelectedIndex) };
            tabs.WithKey(viewKey);
            tabs.OnSelectionChanged = index => TabSelectionMemory.Set(viewKey, index);
            foreach (var c in node.Children)
            {
                if (!(c is GroupLayoutNode g)) continue;
                var page = new VerticalLayout();
                page.WithKey($"tabpage_{g.Path}");
                foreach (var ch in g.Children) { var el = BuildNode(ch, ctx); if (el != null) page.Add(el); }
                tabs.AddPage(g.DisplayName ?? g.Path, page);
            }
            if (tabs.Pages.Count == 0) return null;
            return tabs;
        }

        static IElement BuildField(FieldLayoutNode node, BuildContext ctx)
        {
            var a = node.Accessor; var attrs = node.Attributes;
            if (a.Context == null) a.Context = ctx;
            var lta = GetAttr<LabelTextAttribute>(attrs);
            var label = lta != null ? FieldBinder.ResolveReference(lta.Text, ctx.Renderer, ctx.Target) : ObjectNames.NicifyVariableName(a.SourceField?.Name ?? a.DisplayName);
            IElement editor;
            if (FieldTypeRendererRegistry.TryResolve(a.PropertyType, out var tr)) editor = tr.CreateWidget(a, ctx);
            else { Debug.LogWarning($"[PropertyGrid] No TypeRenderer for '{a.PropertyType.Name}', field='{a.DisplayName}'. Using Fallback."); editor = FieldTypeRendererRegistry.Fallback.CreateWidget(a, ctx); }
            if (editor == null) return null;
            if (editor is Element el) el.WithKey($"insp_{a.SourceField?.Name ?? a.DisplayName}");
            var pf = new PropertyField(label) { Editor = editor, Accessor = a };
            // EnableIf 处理（P1-2 修复）
            ApplyEnableIf(attrs, ctx, pf);

            // 数据被外部（按钮回调/校验器等）修改后，把最新值回写编辑器驱动刷新。
            // 订阅先于装饰器执行：Validate 等构建期写入 FieldChanged 时即被捕捉。
            ctx.Root.FieldChanged += (_, __) => EditorValueSync.Sync(pf.Editor, a);

            var dc = new DecoratorContext(a, attrs, ctx, pf);
            IElement result = pf;
            foreach (var d in FieldDecoratorRegistry.All) if (d.Applies(dc)) result = d.Process(result, dc);

            // 构建期装饰器可能已改数据（如 Validate 回写结果），渲染前对齐一次编辑器初值
            EditorValueSync.Sync(pf.Editor, a);

            return result;
        }

        static IElement BuildClass(ClassElementLayoutNode node, BuildContext ctx)
        {
            if (node.Source is ButtonAttribute ba)
            {
                var b = new Button(ba.Label);
                b.WithKey($"clsbtn_{ba.Method}");
                b.OnClick = () => InvokeRendererMethod(ctx, ba.Method);
                return b;
            }
            if (node.Source is PropertyGridLabelAttribute la)
            {
                var label = new Label(FieldBinder.ResolveReference(la.Text, ctx.Renderer, ctx.Target));
                label.WithKey($"clslbl_{la.Text}");
                WatchReference(la.Text, ctx, value => { if (label.Text != value) { label.Text = value; label.Rebuild(); } });
                return label;
            }
            if (node.Source is PropertyGridInfoBoxAttribute ia)
            {
                var info = new HelpBox(FieldBinder.ResolveReference(ia.Text, ctx.Renderer, ctx.Target), ToMessageType(ia.Type));
                info.WithKey($"clsinfo_{ia.Text}");
                WatchReference(ia.Text, ctx, value => { if (info.Text != value) { info.Text = value; info.Rebuild(); } });
                return info;
            }
            if (node.Source is InfoBoxAttribute fa)
            {
                var info = new HelpBox(FieldBinder.ResolveReference(fa.Text, ctx.Renderer, ctx.Target), ToMessageType(fa.Type));
                info.WithKey($"clsinfo_{fa.Text}");
                WatchReference(fa.Text, ctx, value => { if (info.Text != value) { info.Text = value; info.Rebuild(); } });
                return info;
            }
            var placeholder = new Label(node.DisplayName ?? string.Empty);
            placeholder.WithKey($"clsmsg_{node.DisplayName}");
            return placeholder;
        }

        /// <summary>
        /// 调用 Renderer 方法（Button/Validate 等回调的统一入口）。
        /// 方法可能直接改写数据对象而不经 accessor——调用后广播 FieldChanged，
        /// 让 @ 标签、条件显隐与编辑器值同步统一走既有刷新链路。
        /// </summary>
        internal static void InvokeRendererMethod(BuildContext ctx, string method)
        {
            if (ctx.Renderer == null) return;
            var m = FieldBinder.FindMethod(ctx.Renderer.GetType(), method, ctx.Target?.GetType());
            if (m == null) return;
            var ps = m.GetParameters();
            if (ps.Length == 0) m.Invoke(ctx.Renderer, null);
            else if (ctx.Target != null) m.Invoke(ctx.Renderer, new[] { ctx.Target });
            ctx.Root.RaiseFieldChanged(method, null);
        }

        static HelpBoxMessageType ToMessageType(InfoBoxType type)
        {
            return type == InfoBoxType.Warning ? HelpBoxMessageType.Warning
                : type == InfoBoxType.Error ? HelpBoxMessageType.Error
                : HelpBoxMessageType.Info;
        }

        static void WatchReference(string reference, BuildContext ctx, Action<string> apply)
        {
            if (string.IsNullOrEmpty(reference) || !reference.StartsWith("@")) return;
            ctx.Root.FieldChanged += (_, __) => apply(FieldBinder.ResolveReference(reference, ctx.Renderer, ctx.Target));
        }

        static void ApplyEnableIf(PropertyGridAttribute[] attrs, BuildContext ctx, PropertyField pf)
        {
            if (attrs == null || attrs.All(a => !(a is EnableIfAttribute))) return;

            pf.IsReadOnly = !EvaluateEnabled(attrs, ctx);
            ctx.Root.FieldChanged += (_, __) =>
            {
                var readOnly = !EvaluateEnabled(attrs, ctx);
                if (pf.IsReadOnly == readOnly) return;
                pf.IsReadOnly = readOnly;
                pf.Rebuild();
            };
        }

        static bool EvaluateEnabled(PropertyGridAttribute[] attrs, BuildContext ctx)
        {
            foreach (var a in attrs)
                if (a is EnableIfAttribute e && !IsTrue(ctx.ResolveMember(e.Member), e.Value)) return false;
            return true;
        }

        static bool IsTrue(object mv, object ev) { if (ev == null) return mv is bool b ? b : mv != null; if (mv == null) return false; return ev.Equals(mv); }
        static T GetAttr<T>(PropertyGridAttribute[] a) where T : PropertyGridAttribute { if (a == null) return null; foreach (var x in a) if (x is T t) return t; return null; }
    }
}
