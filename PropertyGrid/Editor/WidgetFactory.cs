using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UniDecl.PropertyGrid.Runtime;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Widgets;
using UnityEditor;
using UnityEngine;
using PropertyField = UniDecl.PropertyGrid.Editor.Elements.PropertyField;

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
            if (node is GroupLayoutNode g) return BuildGroup(g, ctx);
            return null;
        }

        static IElement BuildObject(ObjectLayoutNode node, BuildContext ctx)
        {
            var childCtx = ctx.CreateChild(node);
            var handler = GlobalPropertyGridRegistry.FindByNode(node) ?? new ObjectLayoutHandler();
            var container = (ContainerElement)handler.Build(node, childCtx);
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
            var items = new List<(int o, IElement e)>();
            foreach (var c in node.Children) { var el = BuildNode(c, ctx); if (el != null) items.Add((c.Order, el)); }
            foreach (var (_, element) in items.OrderBy(x => x.o)) container.Add(element);
            return container;
        }

        static IElement BuildField(FieldLayoutNode node, BuildContext ctx)
        {
            var a = node.Accessor; var attrs = node.Attributes;
            if (a.Context == null) a.Context = ctx;
            var lta = GetAttr<LabelTextAttribute>(attrs);
            var label = lta != null ? FieldBinder.ResolveReference(lta.Text, ctx.Renderer, ctx.Target) : ObjectNames.NicifyVariableName(a.SourceField?.Name ?? a.DisplayName);
            if (!CheckCondition(attrs, ctx)) return null;
            IElement editor;
            if (FieldTypeRendererRegistry.TryResolve(a.PropertyType, out var tr)) editor = tr.CreateWidget(a, ctx);
            else { Debug.LogWarning($"[PropertyGrid] No TypeRenderer for '{a.PropertyType.Name}', field='{a.DisplayName}'. Using Fallback."); editor = FieldTypeRendererRegistry.Fallback.CreateWidget(a, ctx); }
            if (editor == null) return null;
            if (editor is Element el) el.WithKey($"insp_{a.SourceField?.Name ?? a.DisplayName}");
            if (editor is PropertyGridElement ne) ne.HostObject = ctx.PropertyGridElement?.HostObject;
            var pf = new PropertyField(label) { Editor = editor, Accessor = a };
            // EnableIf 处理（P1-2 修复）
            ApplyEnableIf(attrs, ctx, pf);
            var dc = new DecoratorContext(a, attrs, ctx, pf);
            IElement result = pf;
            foreach (var d in FieldDecoratorRegistry.All) if (d.Applies(dc)) result = d.Process(result, dc);
            return result;
        }

        static IElement BuildClass(ClassElementLayoutNode node, BuildContext ctx)
        {
            if (node.Source is ButtonAttribute ba) { var b = new Button(ba.Label); b.WithKey($"clsbtn_{ba.Method}"); b.OnClick = () => { if (ctx.Renderer == null) return; var m = FieldBinder.FindMethod(ctx.Renderer.GetType(), ba.Method, ctx.Target?.GetType()); if (m != null) { var ps = m.GetParameters(); if (ps.Length == 0) m.Invoke(ctx.Renderer, null); else if (ps.Length == 1 && ctx.Target != null) m.Invoke(ctx.Renderer, new[] { ctx.Target }); } }; return b; }
            if (node.Source is PropertyGridLabelAttribute la) { var t = FieldBinder.ResolveReference(la.Text, ctx.Renderer, ctx.Target); return new Label(t).WithKey($"clslbl_{la.Text}"); }
            if (node.Source is PropertyGridInfoBoxAttribute ia) { var t = FieldBinder.ResolveReference(ia.Text, ctx.Renderer, ctx.Target); var mt = ia.Type == InfoBoxType.Warning ? HelpBoxMessageType.Warning : ia.Type == InfoBoxType.Error ? HelpBoxMessageType.Error : HelpBoxMessageType.Info; return new HelpBox(t, mt).WithKey($"clsinfo_{ia.Text}"); }
            if (node.Source is InfoBoxAttribute fa) { var t = FieldBinder.ResolveReference(fa.Text, ctx.Renderer, ctx.Target); var mt = fa.Type == InfoBoxType.Warning ? HelpBoxMessageType.Warning : fa.Type == InfoBoxType.Error ? HelpBoxMessageType.Error : HelpBoxMessageType.Info; return new HelpBox(t, mt).WithKey($"clsinfo_{fa.Text}"); }
            return null;
        }

        static bool CheckCondition(PropertyGridAttribute[] attrs, BuildContext ctx)
        {
            foreach (var a in attrs)
            {
                if (a is ShowIfAttribute s) { var v = ctx.ResolveMember(s.Member); if (!IsTrue(v, s.Value)) return false; }
                else if (a is HideIfAttribute h) { var v = ctx.ResolveMember(h.Member); if (IsTrue(v, h.Value)) return false; }
            }
            return true;
        }

        static void ApplyEnableIf(PropertyGridAttribute[] attrs, BuildContext ctx, PropertyField pf)
        {
            foreach (var a in attrs)
            {
                if (a is EnableIfAttribute e)
                {
                    var v = ctx.ResolveMember(e.Member);
                    if (!IsTrue(v, e.Value)) pf.IsReadOnly = true;
                }
            }
        }

        static bool IsTrue(object mv, object ev) { if (ev == null) return mv is bool b ? b : mv != null; if (mv == null) return false; return ev.Equals(mv); }
        static T GetAttr<T>(PropertyGridAttribute[] a) where T : PropertyGridAttribute { if (a == null) return null; foreach (var x in a) if (x is T t) return t; return null; }
    }
}
