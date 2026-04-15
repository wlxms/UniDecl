using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitColorFieldRenderer : IElementRenderer<W.ColorField, VisualElement>
    {
        public VisualElement Render(W.ColorField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new ColorField(element.Label)
            {
                value = element.Value,
                showAlpha = element.ShowAlpha,
                showEyeDropper = element.ShowEyeDropper,
            };

            field.RegisterValueChangedCallback<Color>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ColorFieldChangeEvent(element, evt.newValue, evt.previousValue));
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct ColorFieldChangeEvent
    {
        public W.ColorField Source { get; }
        public Color NewValue { get; }
        public Color PreviousValue { get; }

        public ColorFieldChangeEvent(W.ColorField source, Color newValue, Color previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
