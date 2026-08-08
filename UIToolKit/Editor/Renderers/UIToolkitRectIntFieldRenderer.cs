using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitRectIntFieldRenderer : IElementRenderer<W.RectIntField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.RectIntField, VisualElement>
    {
        public VisualElement Render(W.RectIntField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var field = new RectIntField(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时选择型，Commit 在 ChangeEvent 回调里调用
            var binding = new SnapshotBinding<RectInt>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new RectIntFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        public bool TryUpdate(W.RectIntField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is RectIntField field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.RectIntField f && TryUpdate(f, existing, manager, state);
    }

    public struct RectIntFieldChangeEvent
    {
        public W.RectIntField Source { get; }
        public RectInt NewValue { get; }
        public RectInt PreviousValue { get; }

        public RectIntFieldChangeEvent(W.RectIntField source, RectInt newValue, RectInt previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
