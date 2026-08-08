using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitVector2FieldRenderer : IElementRenderer<W.Vector2Field, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.Vector2Field, VisualElement>
    {
        public VisualElement Render(W.Vector2Field element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var field = new Vector2Field(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时选择型，Commit 在 ChangeEvent 回调里调用
            var binding = new SnapshotBinding<Vector2>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new Vector2FieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        public bool TryUpdate(W.Vector2Field element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is Vector2Field field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.Vector2Field f && TryUpdate(f, existing, manager, state);
    }

    public struct Vector2FieldChangeEvent
    {
        public W.Vector2Field Source { get; }
        public Vector2 NewValue { get; }
        public Vector2 PreviousValue { get; }

        public Vector2FieldChangeEvent(W.Vector2Field source, Vector2 newValue, Vector2 previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
