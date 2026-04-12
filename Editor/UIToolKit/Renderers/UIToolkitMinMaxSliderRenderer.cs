using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitMinMaxSliderRenderer : IElementRenderer<W.MinMaxSlider, VisualElement>
    {
        public VisualElement Render(W.MinMaxSlider element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;

            if (!string.IsNullOrEmpty(element.Label))
                container.Add(new Label(element.Label));

            var slider = new MinMaxSlider(element.MinValue, element.MaxValue, element.LowLimit, element.HighLimit);
            slider.RegisterValueChangedCallback(evt =>
            {
                element.MinValue = evt.newValue.x;
                element.MaxValue = evt.newValue.y;
                element.OnValueChanged?.Invoke(evt.newValue.x, evt.newValue.y);
                manager.Dispatch(new MinMaxSliderChangeEvent(element, evt.newValue.x, evt.newValue.y));
            });

            slider.RegisterCallback<PointerUpEvent>(_ =>
            {
                element.OnCommit?.Invoke(element.MinValue, element.MaxValue);
                element.NotifyChanged();
            });

            slider.RegisterCallback<PointerCaptureOutEvent>(_ =>
            {
                element.OnCommit?.Invoke(element.MinValue, element.MaxValue);
                element.NotifyChanged();
            });

            container.Add(slider);
            return container;
        }
    }

    public struct MinMaxSliderChangeEvent
    {
        public W.MinMaxSlider Source { get; }
        public float MinValue { get; }
        public float MaxValue { get; }

        public MinMaxSliderChangeEvent(W.MinMaxSlider source, float min, float max)
        {
            Source = source;
            MinValue = min;
            MaxValue = max;
        }
    }
}
