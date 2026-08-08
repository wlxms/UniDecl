using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UniDecl.PropertyGrid.Runtime;
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
            HashSet<object> visiting = null, int depth = 0)
        {
            _autoGroupCounter = 0;
            visiting ??= new HashSet<object>(new ReferenceEqualityComparer());
            visiting.Add(target);

            var root = new ObjectLayoutNode { Path = "Root", DisplayName = "Root", Target = target, Meta = meta, RendererType = meta.RendererType, PropagatedAttributes = parentPropagated ?? Array.Empty<PropertyGridAttribute>() };

            var cgm = new Dictionary<string, GroupMeta>();
            CollectClassGroups(meta, cgm);
            root.ClassGroupMap = cgm;

            ILayoutNode currentGroup = root;
            foreach (var field in meta.Fields)
            {
                var attrs = field.GetCustomAttributes<PropertyGridAttribute>(false).ToArray();
                var accessor = parentAccessor?.Child(field) ?? new PropertyAccessor(target, new[] { field }, field.Name, field.Name, GetStyleClass(attrs));

                var la = GetLayoutAttr(attrs);
                if (la != null) { var p = !string.IsNullOrEmpty(la.Path) ? la.Path : $"_AutoGroup_{_autoGroupCounter++}"; currentGroup = EnsureGroup(root, p, la, cgm); }

                if (IsExpandable(field.FieldType) && depth < MaxDepth)
                {
                    var sv = accessor.GetValue();
                    if (sv == null) { var nn = new ClassElementLayoutNode { DisplayName = $"{field.Name} (null)" }; ((LayoutNodeBase)currentGroup).AddChild(nn); }
                    else if (visiting.Contains(sv)) { var cn = new ClassElementLayoutNode { DisplayName = $"[循环引用: {field.FieldType.Name}]" }; ((LayoutNodeBase)currentGroup).AddChild(cn); Debug.LogWarning($"[PropertyGrid] Circular: {field.Name}"); }
                    else
                    {
                        var sm = ReflectionCache.GetOrCreateMeta(field.FieldType);
                        var ia = GetAttr<InlinePropertyAttribute>(attrs);
                        var mp = MergePropagated(root.PropagatedAttributes, attrs);
                        var st = Resolve(sm, sv, accessor, mp, visiting, depth + 1);
                        if (ia != null) ((ObjectLayoutNode)st.Root).Direction = InlineDirection.Horizontal;
                        ((LayoutNodeBase)currentGroup).AddChild(st.Root);
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
            return new LayoutTree { Root = root };
        }

        static bool IsExpandable(Type t)
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
            foreach (var a in meta.ClassAttributes) { if (!(a is LayoutGroupAttribute la) || string.IsNullOrEmpty(la.Path)) continue; var k = la.Path; if (!m.ContainsKey(k)) { var gm = new GroupMeta { Title = la.Title, Order = la.Order }; if (a is FoldoutGroupAttribute fa) gm.Expanded = fa.Expanded; m[k] = gm; } else if (la.Title != null) m[k].Title = la.Title; }
        }

        static ILayoutNode EnsureGroup(ILayoutNode root, string path, LayoutGroupAttribute attr, Dictionary<string, GroupMeta> cgm)
        {
            if (path == "Root") return root;
            var segs = path.Split('/');
            ILayoutNode parent = root;
            var cumulativePath = "";
            for (int i = 0; i < segs.Length; i++)
            {
                cumulativePath = i == 0 ? segs[i] : $"{cumulativePath}/{segs[i]}";
                // 用累积完整路径查找已有节点（避免不同父级下同名段冲突）
                ILayoutNode existing = null;
                foreach (var c in parent.Children) { if ((c is GroupLayoutNode g && g.Path == cumulativePath) || (c is ObjectLayoutNode o && o.Path == cumulativePath)) { existing = c; break; } }
                if (existing != null)
                {
                    // 已有节点——检查是否需要类型升级
                    if (attr != null && i == segs.Length - 1 && existing is GroupLayoutNode eg)
                    {
                        var h = GlobalPropertyGridRegistry.FindByAttr(attr);
                        if (h != null && h.LayoutNodeType != existing.GetType())
                        {
                            // VGroup 兜底节点被显式声明覆盖——静默升级，不报错
                            if (existing is VGroupLayoutNode)
                            {
                                var nn = (GroupLayoutNode)h.CreateNode(cumulativePath); nn.Order = eg.Order;
                                foreach (var cc in eg.Children.ToArray()) ((LayoutNodeBase)nn).AddChild(cc);
                                var cl = (List<ILayoutNode>)((LayoutNodeBase)parent).Children;
                                var ix = cl.IndexOf(existing); if (ix >= 0) cl[ix] = nn;
                                existing = nn;
                            }
                            // 两个不同显式类型冲突——报错并以最后为准
                            else
                            {
                                Debug.LogError($"[PropertyGrid] Group type conflict for '{path}': existing {existing.GetType().Name}, new {h.LayoutNodeType.Name}. Using last.");
                                var nn = (GroupLayoutNode)h.CreateNode(cumulativePath); nn.Order = eg.Order;
                                foreach (var cc in eg.Children.ToArray()) ((LayoutNodeBase)nn).AddChild(cc);
                                var cl = (List<ILayoutNode>)((LayoutNodeBase)parent).Children;
                                var ix = cl.IndexOf(existing); if (ix >= 0) cl[ix] = nn;
                                existing = nn;
                            }
                        }
                    }
                    parent = existing;
                }
                else
                {
                    // 新节点——只在末级段使用 attr 的类型，中间段用 VGroup 兜底
                    var h = (attr != null && i == segs.Length - 1) ? GlobalPropertyGridRegistry.FindByAttr(attr) : null;
                    GroupLayoutNode n = h != null ? (GroupLayoutNode)h.CreateNode(cumulativePath) : new VGroupLayoutNode { Path = cumulativePath, DisplayName = segs[i] };
                    if (cgm.TryGetValue(cumulativePath, out var gm)) { n.DisplayName = gm.Title ?? segs[i]; n.Order = gm.Order; if (n is FoldoutLayoutNode fl) fl.Expanded = gm.Expanded; }
                    ((LayoutNodeBase)parent).AddChild(n); parent = n;
                }
            }
            return parent;
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
