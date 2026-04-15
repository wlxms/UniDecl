using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitRectFieldRenderer : IElementRenderer<W.RectField, VisualElement>
    {
        public VisualElement Render(W.RectField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var field = new RectField(element.Label) { value = element.Value };
            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new RectFieldChangeEvent(element, evt.newValue, evt.previousValue));
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
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
