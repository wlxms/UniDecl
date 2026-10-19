using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UniDecl.PropertyGrid.Runtime;
using UnityEditor;
using UnityEngine;

namespace UniDecl.PropertyGrid.Editor
{
    internal sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);
        int IEqualityComparer<object>.GetHashCode(object o) => RuntimeHelpers.GetHashCode(o);
    }

    public class LayoutTree { public ILayoutNode Root; }

    public static class LayoutResolver
    {
        const int MaxDepth = 64;
        static int _autoGroupCounter;

        public static LayoutTree Resolve(TypeMeta meta, object target,
            PropertyAccessor parentAccessor = null,
            PropertyGridAttribute[] parentPropagated = null,
            HashSet<object> visiting = null, int depth = 0,
            InlineDirection initialValueDirection = InlineDirection.Default,
            bool stripOwnGroups = false)
        {
            var isRootCall = visiting == null;
            _autoGroupCounter = 0;
            visiting ??= new HashSet<object>(new ReferenceEqualityComparer());
            visiting.Add(target);

            var root = new ObjectLayoutNode
            {
                Path = "Root",
                DisplayName = "Root",
                Target = target,
                Meta = meta,
                RendererType = meta.RendererType,
                PropagatedAttributes = parentPropagated ?? Array.Empty<PropertyGridAttribute>(),
                ValueAccessor = parentAccessor,
                InitialValueDirection = initialValueDirection,
            };

            var cgm = new Dictionary<string, GroupMeta>();
            CollectClassGroups(meta, cgm);
            root.ClassGroupMap = cgm;

            ILayoutNode currentGroup = root;
            // 当前活跃的传染式流组（HGroup/VGroup）——无自身布局特性的字段从其中"逃逸"回父级，
            // 避免行组开启后所有后续字段被永久吸附进同一行
            GroupLayoutNode contagiousFlowGroup = null;
            foreach (var field in meta.Fields)
            {
                var attrs = field.GetCustomAttributes<PropertyGridAttribute>(false).ToArray();

                // 字段上声明的组标题提前入册：即使该 attr 不是字段的生效布局特性，
                // 其 Title 也应体现在对应组节点上（如 Basic/Attributes 的"基础属性"）
                RegisterFieldGroupTitles(attrs, cgm);

                var accessor = parentAccessor?.Child(field) ?? new PropertyAccessor(target, new[] { field }, field.Name, field.Name, GetStyleClass(attrs));

                // StripOwnGroups：InlineProperty 展开时剥离子字段自身的组声明，字段留在父级流中
                var la = stripOwnGroups ? null : GetLayoutAttr(attrs);
                if (la != null)
                {
                    var p = !string.IsNullOrEmpty(la.Path) ? la.Path : $"_AutoGroup_{_autoGroupCounter++}";
                    currentGroup = EnsureGroup(root, p, la, cgm);
                    contagiousFlowGroup = la is HGroupAttribute || la is VGroupAttribute ? (GroupLayoutNode)currentGroup : null;
                }
                else if (contagiousFlowGroup != null && ReferenceEquals(currentGroup, contagiousFlowGroup))
                {
                    // 无组字段跟在行组后面——回落到行组的父级（纵向流），不挤进当前行
                    currentGroup = currentGroup.Parent ?? root;
                    contagiousFlowGroup = null;
                }

                if (IsExpandable(field.FieldType) && depth < MaxDepth)
                {
                    var objectGroup = ResolveObjectGroup(currentGroup, root, la);
                    var sv = accessor.GetValue();
                    if (sv == null) { var nn = new ClassElementLayoutNode { DisplayName = $"{field.Name} (null)" }; ((LayoutNodeBase)objectGroup).AddChild(nn); }
                    else if (visiting.Contains(sv)) { var cn = new ClassElementLayoutNode { DisplayName = $"[循环引用: {field.FieldType.Name}]" }; ((LayoutNodeBase)objectGroup).AddChild(cn); Debug.LogWarning($"[PropertyGrid] Circular: {field.Name}"); }
                    else
                    {
                        var sm = ReflectionCache.GetOrCreateMeta(field.FieldType);
                        var ia = GetAttr<InlinePropertyAttribute>(attrs);
                        var mp = MergePropagated(root.PropagatedAttributes, attrs);
                        var childDirection = ia != null
                            ? InlineDirection.Horizontal
                            : InlineDirection.Default;
                        var st = Resolve(sm, sv, accessor, mp, visiting, depth + 1, childDirection, ia != null && ia.StripOwnGroups);
                        ((LayoutNodeBase)objectGroup).AddChild(st.Root);
                    }
                    continue;
                }

                var fn = new FieldLayoutNode { Accessor = accessor, Attributes = MergePropagated(root.PropagatedAttributes, attrs), Order = GetOrder(field) };
                ((LayoutNodeBase)currentGroup).AddChild(fn);
            }

            if (meta.ClassAttributes != null)
            {
                foreach (var a in meta.ClassAttributes)
                {
                    ClassElementLayoutNode cn = null; string gb = "Root"; int o = 0;
                    if (a is ButtonAttribute ba && !string.IsNullOrEmpty(ba.Label)) { cn = new ClassElementLayoutNode { Source = a, DisplayName = ba.Label }; gb = ba.GroupBy ?? "Root"; o = ba.Order; }
                    else if (a is PropertyGridLabelAttribute la) { cn = new ClassElementLayoutNode { Source = a, DisplayName = la.Text }; gb = la.GroupBy ?? "Root"; o = la.Order; }
                    else if (a is PropertyGridInfoBoxAttribute ia) { cn = new ClassElementLayoutNode { Source = a, DisplayName = ia.Text }; gb = ia.GroupBy ?? "Root"; o = ia.Order; }
                    else if (a is InfoBoxAttribute fa) { cn = new ClassElementLayoutNode { Source = a, DisplayName = fa.Text }; gb = fa.GroupBy ?? "Root"; o = fa.Order; }
                    if (cn != null) { cn.Order = o; var tg = EnsureGroup(root, gb, null, cgm); ((LayoutNodeBase)tg).AddChild(cn); }
                }
            }

            visiting.Remove(target);

            // 结构性后处理只在根调用执行一次：全树 Order 排序 + 连续 TabGroup 包装为页签视图
            if (isRootCall) Normalize(root);

            return new LayoutTree { Root = root };
        }

        static ILayoutNode ResolveObjectGroup(ILayoutNode currentGroup, ILayoutNode root, LayoutGroupAttribute fieldLayout)
        {
            if (fieldLayout != null || !(currentGroup is HGroupLayoutNode))
                return currentGroup;

            return currentGroup.Parent ?? root;
        }

        /// <summary>Serializable 展开判定（List 渲染器对元素类型复用同一规则）。</summary>
        internal static bool IsExpandable(Type t)
        {
            if (t.IsPrimitive || t == typeof(string) || t.IsEnum) return false;
            if (typeof(UnityEngine.Object).IsAssignableFrom(t)) return false;
            if (t.IsArray || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))) return false;
            return t.GetCustomAttribute<SerializableAttribute>() != null || (t.IsValueType && t.IsSerializable);
        }

        static LayoutGroupAttribute GetLayoutAttr(PropertyGridAttribute[] a) { for (int i = a.Length - 1; i >= 0; i--) if (a[i] is LayoutGroupAttribute l) return l; return null; }
        static string GetStyleClass(PropertyGridAttribute[] a) { var s = new List<string>(); foreach (var x in a) if (x is StyleClassAttribute sc) s.Add(sc.ClassName); return s.Count > 0 ? string.Join(" ", s) : null; }
        static int GetOrder(FieldInfo f) => f.GetCustomAttribute<PropertyOrderAttribute>()?.Order ?? 0;

        static void CollectClassGroups(TypeMeta meta, Dictionary<string, GroupMeta> m)
        {
            if (meta.ClassAttributes == null) return;
            foreach (var a in meta.ClassAttributes)
            {
                if (!(a is LayoutGroupAttribute la) || string.IsNullOrEmpty(la.Path)) continue;
                var k = la.Path;
                if (!m.ContainsKey(k)) { var gm = new GroupMeta { Title = la.Title, Order = la.Order, Attr = la }; if (a is FoldoutGroupAttribute fa) gm.Expanded = fa.Expanded; m[k] = gm; }
                else if (la.Title != null) m[k].Title = la.Title;
            }
        }

        /// <summary>
        /// 把字段声明的组特性元数据（Title/Order/Expanded）入册到 cgm——仅补元数据，
        /// 不改变节点类型来源（节点类型仍由类级声明 / 生效字段特性决定）。
        /// </summary>
        static void RegisterFieldGroupTitles(PropertyGridAttribute[] attrs, Dictionary<string, GroupMeta> m)
        {
            if (attrs == null) return;
            foreach (var a in attrs)
            {
                if (!(a is LayoutGroupAttribute la) || string.IsNullOrEmpty(la.Path)) continue;
                var k = la.Path;
                if (!m.TryGetValue(k, out var gm))
                    m[k] = gm = new GroupMeta { Title = la.Title, Order = la.Order };
                else if (gm.Title == null && la.Title != null)
                    gm.Title = la.Title;
            }
        }

        // =========================================================================
        // 结构性后处理：Order 排序 + TabView 包装
        // =========================================================================

        /// <summary>
        /// 递归规范化：先处理子树，再对当前容器做稳定排序，最后把连续的 TabLayoutNode
        /// （≥2 个）合成为一个 TabViewLayoutNode。排序在布局层统一完成，渲染层按序直出。
        /// </summary>
        static void Normalize(ILayoutNode node)
        {
            foreach (var c in node.Children.ToList()) Normalize(c);

            if (!(node is LayoutNodeBase container)) return;
            container.SortChildrenStable();
            if (!(node is TabViewLayoutNode)) // 已包装的页节点不再重复包装
                WrapConsecutiveTabs(container);
        }

        /// <summary>把同一容器下连续出现的 TabLayoutNode 合并为一个 TabViewLayoutNode。</summary>
        static void WrapConsecutiveTabs(LayoutNodeBase container)
        {
            var children = container.Children.ToList();
            int i = 0;
            while (i < children.Count)
            {
                if (!(children[i] is TabLayoutNode)) { i++; continue; }

                int end = i;
                while (end < children.Count && children[end] is TabLayoutNode) end++;

                if (end - i >= 2)
                {
                    var tabView = new TabViewLayoutNode
                    {
                        Path = $"TabView/{children[i].Path}",
                        DisplayName = "TabView",
                        Order = children[i].Order,
                        SelectedIndex = 0,
                    };
                    // 页顺序即声明顺序；页标题由各 Tab 节点的 DisplayName（类级 Title）提供
                    tabView.AddChildren(children.GetRange(i, end - i));
                    container.ReplaceChildrenRange(i, end - i, tabView);
                    children = container.Children.ToList();
                }

                i = end;
            }
        }

        // =========================================================================
        // 组路径解析
        // =========================================================================

        /// <summary>
        /// 确保路径对应的组节点存在并返回最深层节点。
        /// 每段的节点类型由三级优先级决定：字段显式特性（仅末级段）→ 类级声明（cgm.Attr）→ VGroup 兜底。
        /// 已存在的兜底节点遇到更明确的类型声明时原地升级（保留子节点与顺序）。
        /// </summary>
        static ILayoutNode EnsureGroup(ILayoutNode root, string path, LayoutGroupAttribute attr, Dictionary<string, GroupMeta> cgm)
        {
            if (path == "Root") return root;
            var segs = path.Split('/');
            ILayoutNode parent = root;
            var cumulativePath = "";
            for (int i = 0; i < segs.Length; i++)
            {
                cumulativePath = i == 0 ? segs[i] : $"{cumulativePath}/{segs[i]}";
                var isLast = i == segs.Length - 1;
                cgm.TryGetValue(cumulativePath, out var gm);

                // 本段生效的声明：末级段优先用字段显式 attr，否则用类级声明（中间段由此获得类型）
                var segAttr = (attr != null && isLast) ? attr : gm?.Attr;

                ILayoutNode existing = FindChildGroup(parent, cumulativePath);
                if (existing != null)
                {
                    if (segAttr != null && existing is GroupLayoutNode existingGroup)
                        existing = UpgradeIfNeeded(parent, existingGroup, segAttr);
                    parent = existing;
                }
                else
                {
                    var h = segAttr != null ? GlobalPropertyGridRegistry.FindByAttr(segAttr) : null;
                    GroupLayoutNode n = h != null
                        ? (GroupLayoutNode)h.CreateNode(cumulativePath)
                        : new VGroupLayoutNode { Path = cumulativePath };
                    // 标题优先级：类级声明 > 字段特性 Title > 末段友好名（裸路径仅作 Path 标识）
                    var title = gm?.Title ?? segAttr?.Title ?? ObjectNames.NicifyVariableName(segs[i]);
                    n.DisplayName = title;
                    n.Order = gm?.Order ?? segAttr?.Order ?? 0;
                    if (n is FoldoutLayoutNode fl && gm != null) fl.Expanded = gm.Expanded;
                    ((LayoutNodeBase)parent).AddChild(n);
                    parent = n;
                }
            }
            return parent;
        }

        /// <summary>用累积完整路径在直接子级中查找已有组节点（避免不同父级下同名段冲突）。</summary>
        static ILayoutNode FindChildGroup(ILayoutNode parent, string cumulativePath)
        {
            foreach (var c in parent.Children)
                if ((c is GroupLayoutNode g && g.Path == cumulativePath) || (c is ObjectLayoutNode o && o.Path == cumulativePath))
                    return c;
            return null;
        }

        /// <summary>
        /// 类型升级：existing 的已注册类型与 segAttr 期望的类型不一致时，
        /// 新建期望类型节点并继承 Order 与全部子节点。VGroup 兜底被覆盖时静默升级；
        /// 两个不同显式类型冲突时报错并以最后声明为准。
        /// </summary>
        static ILayoutNode UpgradeIfNeeded(ILayoutNode parent, GroupLayoutNode existing, LayoutGroupAttribute segAttr)
        {
            var h = GlobalPropertyGridRegistry.FindByAttr(segAttr);
            if (h == null || h.LayoutNodeType == existing.GetType()) return existing;

            if (!(existing is VGroupLayoutNode))
                Debug.LogError($"[PropertyGrid] Group type conflict for '{existing.Path}': existing {existing.GetType().Name}, new {h.LayoutNodeType.Name}. Using last.");

            var nn = (GroupLayoutNode)h.CreateNode(existing.Path);
            nn.DisplayName = existing.DisplayName; // 升级只换类型，保留已解析的标题
            nn.Order = existing.Order;
            foreach (var c in existing.Children.ToList()) nn.AddChild(c);
            ((LayoutNodeBase)parent).ReplaceChild(existing, nn);
            return nn;
        }

        static PropertyGridAttribute[] MergePropagated(PropertyGridAttribute[] p, PropertyGridAttribute[] o)
        {
            if (p == null || p.Length == 0) return o ?? Array.Empty<PropertyGridAttribute>();
            if (o == null || o.Length == 0) return p;
            var m = new Dictionary<Type, PropertyGridAttribute>();
            foreach (var a in p) if (a.PropagateOnInline) m[a.GetType()] = a;
            foreach (var a in o) m[a.GetType()] = a;
            return m.Values.ToArray();
        }

        static T GetAttr<T>(PropertyGridAttribute[] a) where T : PropertyGridAttribute { if (a == null) return null; foreach (var x in a) if (x is T t) return t; return null; }
    }
}
