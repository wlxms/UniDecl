using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitRectFieldRenderer : IElementRenderer<W.RectField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.RectField, VisualElement>
    {
        public VisualElement Render(W.RectField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var field = new RectField(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时选择型，Commit 在 ChangeEvent 回调里调用
            var binding = new SnapshotBinding<Rect>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new RectFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        public bool TryUpdate(W.RectField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is RectField field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.RectField f && TryUpdate(f, existing, manager, state);
    }

    public struct RectFieldChangeEvent
    {
        public W.RectField Source { get; }
        public Rect NewValue { get; }
        public Rect PreviousValue { get; }

        public RectFieldChangeEvent(W.RectField source, Rect newValue, Rect previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
