using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using W = UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitSliderRenderer : IElementRenderer<W.Slider, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.Slider, VisualElement>
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

            // Snapshot 绑定——Register setter + 提供 Commit() 方法
            var binding = new SnapshotBinding<float>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { slider.SetValueWithoutNotify(v); element.Value = v; });

            slider.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new SliderChangeEvent(element, evt.newValue, evt.previousValue));
            });

            slider.RegisterCallback<PointerUpEvent>(_ =>
            {
                binding.Commit();
                element.OnCommit?.Invoke(element.Value);
                element.NotifyChanged();
            });

            slider.RegisterCallback<PointerCaptureOutEvent>(_ =>
            {
                binding.Commit();
                element.OnCommit?.Invoke(element.Value);
                element.NotifyChanged();
            });

            
            container.Add(slider);
            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }

        public bool TryUpdate(W.Slider element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is VisualElement ve && ve.Q<UnityEngine.UIElements.Slider>() is var slider && slider != null)
            {
                slider.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.Slider f && TryUpdate(f, existing, manager, state);
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
