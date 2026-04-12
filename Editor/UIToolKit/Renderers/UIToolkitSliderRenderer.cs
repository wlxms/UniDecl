using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitSliderRenderer : IElementRenderer<W.Slider, VisualElement>
    {
        public VisualElement Render(W.Slider element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;

            if (!string.IsNullOrEmpty(element.Label))
                container.Add(new Label(element.Label));

            var slider = new UnityEngine.UIElements.Slider(element.LowValue, element.HighValue)
            {
                value = element.Value
            };

            slider.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new SliderChangeEvent(element, evt.newValue, evt.previousValue));
            });
slider.RegisterCallback<PointerUpEvent>(_ =>
            {
                element.OnCommit?.Invoke(element.Value);
                element.NotifyChanged();
            });

            slider.RegisterCallback<PointerCaptureOutEvent>(_ =>
            {
                element.OnCommit?.Invoke(element.Value);
                element.NotifyChanged();
            });

            
            container.Add(slider);
            return container;
        }
    }

    public struct SliderChangeEvent
    {
        public W.Slider Source { get; }
        public float NewValue { get; }
        public float PreviousValue { get; }

        public SliderChangeEvent(W.Slider source, float newValue, float previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
