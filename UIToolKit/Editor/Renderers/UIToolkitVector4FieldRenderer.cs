using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitVector4FieldRenderer : IElementRenderer<W.Vector4Field, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.Vector4Field, VisualElement>
    {
        public VisualElement Render(W.Vector4Field element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var field = new Vector4Field(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时选择型，Commit 在 ChangeEvent 回调里调用
            var binding = new SnapshotBinding<Vector4>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new Vector4FieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        public bool TryUpdate(W.Vector4Field element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is Vector4Field field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.Vector4Field f && TryUpdate(f, existing, manager, state);
    }

    public struct Vector4FieldChangeEvent
    {
        public W.Vector4Field Source { get; }
        public Vector4 NewValue { get; }
        public Vector4 PreviousValue { get; }

        public Vector4FieldChangeEvent(W.Vector4Field source, Vector4 newValue, Vector4 previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
