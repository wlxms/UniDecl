using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using W = UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitMinMaxSliderRenderer : IElementRenderer<W.MinMaxSlider, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.MinMaxSlider, VisualElement>
    {
        public VisualElement Render(W.MinMaxSlider element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;

            if (!string.IsNullOrEmpty(element.Label))
                container.Add(new Label(element.Label));

            var slider = new MinMaxSlider(element.MinValue, element.MaxValue, element.LowLimit, element.HighLimit);

            // Snapshot 绑定——值类型用 Vector2（UITK 原生），setter 里拆分到 MinValue/MaxValue
            var binding = new SnapshotBinding<Vector2>(state?.Scope, element.Key,
                new Vector2(element.MinValue, element.MaxValue),
                () => new Vector2(element.MinValue, element.MaxValue),
                v => { slider.SetValueWithoutNotify(v); element.MinValue = v.x; element.MaxValue = v.y; });

            slider.RegisterValueChangedCallback(evt =>
            {
                element.MinValue = evt.newValue.x;
                element.MaxValue = evt.newValue.y;
                element.OnValueChanged?.Invoke(evt.newValue.x, evt.newValue.y);
                manager.Dispatch(new MinMaxSliderChangeEvent(element, evt.newValue.x, evt.newValue.y));
            });

            slider.RegisterCallback<PointerUpEvent>(_ =>
            {
                binding.Commit();
                element.OnCommit?.Invoke(element.MinValue, element.MaxValue);
                element.NotifyChanged();
            });

            slider.RegisterCallback<PointerCaptureOutEvent>(_ =>
            {
                binding.Commit();
                element.OnCommit?.Invoke(element.MinValue, element.MaxValue);
                element.NotifyChanged();
            });

            container.Add(slider);
            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }

        public bool TryUpdate(W.MinMaxSlider element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is VisualElement ve && ve.Q<MinMaxSlider>() is var slider && slider != null)
            {
                slider.SetValueWithoutNotify(new Vector2(element.MinValue, element.MaxValue));
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.MinMaxSlider f && TryUpdate(f, existing, manager, state);
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
