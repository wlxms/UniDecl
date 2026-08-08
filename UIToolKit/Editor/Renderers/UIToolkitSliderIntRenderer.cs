using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using W = UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitSliderIntRenderer : IElementRenderer<W.SliderInt, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.SliderInt, VisualElement>
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

            // Snapshot 绑定——Register setter + 提供 Commit() 方法
            var binding = new SnapshotBinding<int>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { slider.SetValueWithoutNotify(v); element.Value = v; });

            slider.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new SliderIntChangeEvent(element, evt.newValue, evt.previousValue));
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

        public bool TryUpdate(W.SliderInt element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is VisualElement ve && ve.Q<UnityEngine.UIElements.SliderInt>() is var slider && slider != null)
            {
                slider.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.SliderInt f && TryUpdate(f, existing, manager, state);
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
