using System;
using System.Collections.Generic;
using System.Reflection;
using UniDecl.PropertyGrid.Runtime;
using UnityEngine;

namespace UniDecl.PropertyGrid.Editor
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class UniPropertyGridPluginAttribute : Attribute { public string Name { get; } public UniPropertyGridPluginAttribute(string n) { Name = n; } }
    public interface IUniPropertyGridPlugin { string Name { get; } void OnInit(IPluginRegistry r); }
    public interface IPluginRegistry { string PluginName { get; } void RegisterLayoutHandler<TAttr, TNode>(ILayoutHandler<TNode> h) where TAttr : LayoutGroupAttribute where TNode : ILayoutNode; void RegisterRootLayoutHandler<TNode>(ILayoutHandler<TNode> h) where TNode : ILayoutNode; }

    public static class GlobalPropertyGridRegistry
    {
        sealed class Entry { public ILayoutHandler Handler; public string PluginName; }
        static readonly Dictionary<Type, Entry> _byAttr = new Dictionary<Type, Entry>();
        static readonly Dictionary<Type, Entry> _byNode = new Dictionary<Type, Entry>();
        static Entry _root;
        public static void Mount(string pn, Type at, Type nt, ILayoutHandler h)
        {
            if (at == null) { if (_root != null && _root.Handler.GetType() != h.GetType()) Debug.LogWarning($"[PropertyGrid] Plugin '{pn}' overrides '{_root.PluginName}' for Root"); _root = new Entry { Handler = h, PluginName = pn }; }
            else MountOne(_byAttr, at, pn, h);
            MountOne(_byNode, nt, pn, h);
        }
        static void MountOne(Dictionary<Type, Entry> d, Type k, string pn, ILayoutHandler h) { if (d.TryGetValue(k, out var e) && e.Handler != h) Debug.LogWarning($"[PropertyGrid] Plugin '{pn}' overrides '{e.PluginName}' for {k.Name}"); d[k] = new Entry { Handler = h, PluginName = pn }; }
        public static ILayoutHandler FindByAttr(LayoutGroupAttribute a) => a != null && _byAttr.TryGetValue(a.GetType(), out var e) ? e.Handler : null;
        public static ILayoutHandler FindByNode(ILayoutNode n) => n != null && _byNode.TryGetValue(n.GetType(), out var e) ? e.Handler : null;
        public static ILayoutHandler FindRoot() => _root?.Handler;
        public static void Clear() { _byAttr.Clear(); _byNode.Clear(); _root = null; }
    }

    public sealed class PluginRegistry : IPluginRegistry
    {
        public string PluginName { get; }
        readonly List<(Type at, Type nt, ILayoutHandler h)> _lh = new List<(Type, Type, ILayoutHandler)>();
        readonly List<(Type nt, ILayoutHandler h)> _rh = new List<(Type, ILayoutHandler)>();
        public PluginRegistry(string pn) { PluginName = pn; }
        public void RegisterLayoutHandler<TAttr, TNode>(ILayoutHandler<TNode> h) where TAttr : LayoutGroupAttribute where TNode : ILayoutNode => _lh.Add((typeof(TAttr), typeof(TNode), h));
        public void RegisterRootLayoutHandler<TNode>(ILayoutHandler<TNode> h) where TNode : ILayoutNode => _rh.Add((typeof(TNode), h));
        public void MountToGlobal() { foreach (var (a, n, h) in _lh) GlobalPropertyGridRegistry.Mount(PluginName, a, n, h); foreach (var (n, h) in _rh) GlobalPropertyGridRegistry.Mount(PluginName, null, n, h); }
    }

    public static class PluginDiscovery
    {
        public static List<IUniPropertyGridPlugin> Discover()
        {
            var r = new List<IUniPropertyGridPlugin>();
            var ts = new List<Type>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()) { Type[] types; try { types = asm.GetTypes(); } catch { continue; } foreach (var t in types) { try { if (!t.IsClass || t.IsAbstract) continue; if (t.GetCustomAttribute<UniPropertyGridPluginAttribute>() == null) continue; if (!typeof(IUniPropertyGridPlugin).IsAssignableFrom(t)) continue; ts.Add(t); } catch { } } }
            ts.Sort((a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));
            foreach (var t in ts) { try { r.Add((IUniPropertyGridPlugin)Activator.CreateInstance(t)); } catch (Exception ex) { Debug.LogError($"[PropertyGrid] Plugin '{t.FullName}' failed: {ex.Message}"); } }
            return r;
        }
    }

    [UniPropertyGridPlugin("BuiltIn")]
    public class BuiltInPropertyGridPlugin : IUniPropertyGridPlugin
    {
        public string Name => "BuiltIn";
        public void OnInit(IPluginRegistry r)
        {
            r.RegisterLayoutHandler<HGroupAttribute, HGroupLayoutNode>(new HGroupLayoutHandler());
            r.RegisterLayoutHandler<VGroupAttribute, VGroupLayoutNode>(new VGroupLayoutHandler());
            r.RegisterLayoutHandler<BoxGroupAttribute, BoxGroupLayoutNode>(new BoxGroupLayoutHandler());
            r.RegisterLayoutHandler<FoldoutGroupAttribute, FoldoutLayoutNode>(new FoldoutLayoutHandler());
            r.RegisterLayoutHandler<HeaderGroupAttribute, HeaderLayoutNode>(new HeaderLayoutHandler());
            r.RegisterLayoutHandler<TabGroupAttribute, TabLayoutNode>(new TabLayoutHandler());
            r.RegisterRootLayoutHandler<ObjectLayoutNode>(new ObjectLayoutHandler());

            FieldTypeRendererRegistry.Register(new IntTypeRenderer());
            FieldTypeRendererRegistry.Register(new FloatTypeRenderer());
            FieldTypeRendererRegistry.Register(new DoubleTypeRenderer());
            FieldTypeRendererRegistry.Register(new LongTypeRenderer());
            FieldTypeRendererRegistry.Register(new StringTypeRenderer());
            FieldTypeRendererRegistry.Register(new BoolTypeRenderer());
            FieldTypeRendererRegistry.Register(new EnumTypeRenderer());
            FieldTypeRendererRegistry.Register(new ColorTypeRenderer());
            FieldTypeRendererRegistry.Register(new Vector2TypeRenderer());
            FieldTypeRendererRegistry.Register(new Vector3TypeRenderer());
            FieldTypeRendererRegistry.Register(new Vector4TypeRenderer());
            FieldTypeRendererRegistry.Register(new Vector2IntTypeRenderer());
            FieldTypeRendererRegistry.Register(new Vector3IntTypeRenderer());
            FieldTypeRendererRegistry.Register(new RectTypeRenderer());
            FieldTypeRendererRegistry.Register(new BoundsTypeRenderer());
            FieldTypeRendererRegistry.Register(new CurveTypeRenderer());
            FieldTypeRendererRegistry.Register(new GradientTypeRenderer());
            FieldTypeRendererRegistry.Register(new LayerMaskTypeRenderer());
            FieldTypeRendererRegistry.Register(new ObjectTypeRenderer());

            // Replacement Decorator（P=2000）
            FieldDecoratorRegistry.Register(new RangeDecorator());
            FieldDecoratorRegistry.Register(new MinMaxSliderDecorator());
            FieldDecoratorRegistry.Register(new TextAreaDecorator());
            FieldDecoratorRegistry.Register(new EnumToggleButtonsDecorator());
            FieldDecoratorRegistry.Register(new ButtonFieldDecorator());
            FieldDecoratorRegistry.Register(new DropdownDecorator());
            // Metadata Decorator（P=1000）
            FieldDecoratorRegistry.Register(new LabelTextDecorator());
            FieldDecoratorRegistry.Register(new HideLabelDecorator());
            FieldDecoratorRegistry.Register(new ReadOnlyDecorator());
            FieldDecoratorRegistry.Register(new TooltipDecorator());
            FieldDecoratorRegistry.Register(new IndentDecorator());
            FieldDecoratorRegistry.Register(new OnValueChangedDecorator());
            FieldDecoratorRegistry.Register(new ConditionDecorator());
        }
    }
}
