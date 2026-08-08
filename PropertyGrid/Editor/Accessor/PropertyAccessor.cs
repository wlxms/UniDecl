using System;
using System.Collections.Generic;
using System.Reflection;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.PropertyGrid.Runtime;

namespace UniDecl.PropertyGrid.Editor
{
    public sealed class PropertyAccessor
    {
        public object Root { get; }
        public Type PropertyType { get; }
        public string DisplayName { get; }
        public string FullPath { get; }
        public FieldInfo SourceField { get; }
        public string StyleClass { get; set; }
        public BuildContext Context { get; internal set; }
        public event Action<object, object> ValueChanged;
        readonly FieldInfo[] _path;

        public PropertyAccessor(object root, FieldInfo[] path, string displayName, string fullPath, string styleClass = null, BuildContext context = null)
        {
            Root = root; _path = path; DisplayName = displayName; FullPath = fullPath;
            SourceField = path.Length > 0 ? path[path.Length - 1] : null;
            PropertyType = SourceField?.FieldType ?? typeof(object);
            StyleClass = styleClass; Context = context;
        }

        public object GetValue()
        {
            object cur = Root;
            for (int i = 0; i < _path.Length; i++)
            {
                if (cur == null) throw new InvalidOperationException($"Null intermediate at '{_path[i].Name}' in '{FullPath}'");
                cur = _path[i].GetValue(cur);
            }
            return cur;
        }

        public void SetValue(object value)
        {
            object[] chain = new object[_path.Length];
            object cur = Root;
            for (int i = 0; i < _path.Length; i++)
            {
                if (cur == null) throw new InvalidOperationException($"Null intermediate at '{_path[i].Name}' in '{FullPath}'");
                chain[i] = _path[i].GetValue(cur);
                if (i < _path.Length - 1) cur = chain[i];
            }
            var oldVal = chain[_path.Length - 1];
            var leafCarrier = (_path.Length == 1) ? Root : chain[_path.Length - 2];
            _path[_path.Length - 1].SetValue(leafCarrier, value);
            chain[_path.Length - 1] = value;
            for (int i = _path.Length - 2; i >= 0; i--)
            {
                if (!_path[i].FieldType.IsValueType) continue;
                var carrier = (i == 0) ? Root : chain[i - 1];
                _path[i].SetValue(carrier, chain[i]);
            }
            ValueChanged?.Invoke(value, oldVal);
            Context?.RaiseFieldChanged(SourceField?.Name, value);
        }

        public PropertyAccessor Child(FieldInfo childField)
        {
            var np = new FieldInfo[_path.Length + 1];
            Array.Copy(_path, np, _path.Length);
            np[_path.Length] = childField;
            return new PropertyAccessor(Root, np, childField.Name, $"{FullPath}/{childField.Name}", StyleClass, Context);
        }
    }

    // =========================================================================
    // Decorator 接口 + Registries
    // =========================================================================

    public readonly struct DecoratorContext
    {
        public PropertyAccessor Accessor { get; }
        public PropertyGridAttribute[] Attributes { get; }
        public BuildContext BuildContext { get; }
        public Elements.PropertyField Field { get; }
        public DecoratorContext(PropertyAccessor a, PropertyGridAttribute[] attrs, BuildContext bc, Elements.PropertyField f) { Accessor = a; Attributes = attrs; BuildContext = bc; Field = f; }
        public T GetAttribute<T>() where T : PropertyGridAttribute { if (Attributes == null) return null; for (int i = 0; i < Attributes.Length; i++) if (Attributes[i] is T t) return t; return null; }
    }

    public interface IFieldDecorator
    {
        int Priority { get; }
        bool Applies(in DecoratorContext ctx);
        IElement Process(IElement input, DecoratorContext ctx);
    }

    public abstract class ReplacementDecorator : IFieldDecorator { public virtual int Priority => 2000; public abstract bool Applies(in DecoratorContext ctx); public abstract IElement Process(IElement input, DecoratorContext ctx); }
    public abstract class MetadataDecorator : IFieldDecorator { public virtual int Priority => 1000; public abstract bool Applies(in DecoratorContext ctx); public abstract IElement Process(IElement input, DecoratorContext ctx); }

    // =========================================================================
    // TypeRenderer Registry
    // =========================================================================

    public interface IFieldTypeRenderer { Type FieldType { get; } IElement CreateWidget(PropertyAccessor accessor, BuildContext ctx); }

    public sealed class FallbackTypeRenderer : IFieldTypeRenderer
    {
        public Type FieldType => typeof(object);
        public IElement CreateWidget(PropertyAccessor a, BuildContext c) => new UniDecl.BuiltIn.Runtime.Widgets.Label($"{a.DisplayName}: {a.GetValue() ?? "null"}");
    }

    public static class FieldTypeRendererRegistry
    {
        static readonly Dictionary<Type, IFieldTypeRenderer> _r = new Dictionary<Type, IFieldTypeRenderer>();
        static IFieldTypeRenderer _fb = new FallbackTypeRenderer();
        public static void Register(IFieldTypeRenderer r) { if (_r.ContainsKey(r.FieldType)) UnityEngine.Debug.LogWarning($"[PropertyGrid] TypeRenderer '{r.FieldType.Name}' overridden"); _r[r.FieldType] = r; }
        public static bool TryResolve(Type t, out IFieldTypeRenderer r)
        {
            // 1. 精确匹配
            if (_r.TryGetValue(t, out r)) return true;
            // 2. 继承链回退（enum → typeof(Enum)，子类 → typeof(UnityEngine.Object)）
            if (t.IsEnum && _r.TryGetValue(typeof(Enum), out r)) return true;
            if (typeof(UnityEngine.Object).IsAssignableFrom(t) && _r.TryGetValue(typeof(UnityEngine.Object), out r)) return true;
            r = null;
            return false;
        }
        public static IFieldTypeRenderer Fallback => _fb;
    }

    public static class FieldDecoratorRegistry
    {
        static readonly List<IFieldDecorator> _d = new List<IFieldDecorator>();
        static IReadOnlyList<IFieldDecorator> _snap = Array.Empty<IFieldDecorator>();
        public static void Register(IFieldDecorator d, string pn = "BuiltIn")
        {
            var dt = d.GetType();
            for (int i = 0; i < _d.Count; i++)
                if (_d[i].GetType() == dt) { UnityEngine.Debug.LogWarning($"[PropertyGrid] Decorator '{dt.Name}' overridden"); _d[i] = d; RebuildSnap(); return; }
            int idx = _d.Count;
            for (int i = 0; i < _d.Count; i++) if (_d[i].Priority < d.Priority) { idx = i; break; }
            _d.Insert(idx, d); RebuildSnap();
        }
        public static IReadOnlyList<IFieldDecorator> All => _snap;
        static void RebuildSnap() => _snap = _d.ToArray();
    }
}
