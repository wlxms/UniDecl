using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitGradientFieldRenderer : IElementRenderer<W.GradientField, VisualElement>
    {
        public VisualElement Render(W.GradientField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var field = new GradientField(element.Label) { value = element.Value };
            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new GradientFieldChangeEvent(element, evt.newValue, evt.previousValue));
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct GradientFieldChangeEvent
    {
        public W.GradientField Source { get; }
        public Gradient NewValue { get; }
        public Gradient PreviousValue { get; }

        public GradientFieldChangeEvent(W.GradientField source, Gradient newValue, Gradient previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
