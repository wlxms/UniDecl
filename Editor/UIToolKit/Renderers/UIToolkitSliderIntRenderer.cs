using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitSliderIntRenderer : IElementRenderer<W.SliderInt, VisualElement>
    {
        public VisualElement Render(W.SliderInt element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;

            if (!string.IsNullOrEmpty(element.Label))
                container.Add(new Label(element.Label));

            var slider = new UnityEngine.UIElements.SliderInt(element.LowValue, element.HighValue)
            {
                value = element.Value
            };

            slider.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new SliderIntChangeEvent(element, evt.newValue, evt.previousValue));
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
            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }

    public struct SliderIntChangeEvent
    {
        public W.SliderInt Source { get; }
        public int NewValue { get; }
        public int PreviousValue { get; }

        public SliderIntChangeEvent(W.SliderInt source, int newValue, int previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
