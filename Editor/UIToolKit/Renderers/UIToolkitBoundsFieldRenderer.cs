using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitBoundsFieldRenderer : IElementRenderer<W.BoundsField, VisualElement>
    {
        public VisualElement Render(W.BoundsField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var field = new BoundsField(element.Label) { value = element.Value };
            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new BoundsFieldChangeEvent(element, evt.newValue, evt.previousValue));
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct BoundsFieldChangeEvent
    {
        public W.BoundsField Source { get; }
        public Bounds NewValue { get; }
        public Bounds PreviousValue { get; }

        public BoundsFieldChangeEvent(W.BoundsField source, Bounds newValue, Bounds previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
