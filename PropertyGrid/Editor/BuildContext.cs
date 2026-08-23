using System;
using System.Collections.Generic;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.PropertyGrid.Editor
{
    public class BuildContext
    {
        public object Target;
        public object Renderer;
        public TypeMeta Meta;
        public PropertyGridElement PropertyGridElement;
        public BuildContext Parent { get; private set; }
        public BuildContext Root => Parent?.Root ?? this;
        public event Action<string, object> FieldChanged;

        internal void RaiseFieldChanged(string fieldName, object value)
        {
            FieldChanged?.Invoke(fieldName, value);
            Parent?.RaiseFieldChanged(fieldName, value);
        }

        public static BuildContext CreateRoot(PropertyGridElement element, object target, TypeMeta meta)
            => new BuildContext { Target = target, Meta = meta, PropertyGridElement = element };

        public BuildContext CreateChild(ObjectLayoutNode node)
        {
            object r = null;
            if (node.RendererType != null)
                try { r = Activator.CreateInstance(node.RendererType); } catch (Exception ex) { UnityEngine.Debug.LogWarning($"[PropertyGrid] Renderer creation failed: {ex.Message}"); }
            return new BuildContext { Target = node.Target, Renderer = r, Meta = node.Meta, PropertyGridElement = Root.PropertyGridElement, Parent = this };
        }

        public object ResolveMember(string member)
        {
            if (string.IsNullOrEmpty(member)) return null;
            if (Renderer != null && FieldBinder.TryResolveValue(Renderer, member, out var v1)) return v1;
            if (Target != null && FieldBinder.TryResolveValue(Target, member, out var v2)) return v2;
            return Parent?.ResolveMember(member);
        }
    }
}
