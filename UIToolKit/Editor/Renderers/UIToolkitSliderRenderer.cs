using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitSliderRenderer : IElementRenderer<W.Slider, VisualElement>
    {
        public VisualElement Render(W.Slider element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is Slider reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var slider = new Slider(element.Label, element.LowValue, element.HighValue) { value = element.Value };

            // Snapshot 绑定——瞬时型，ChangeEvent 即提交（连续拖动靠 Merge 合并）
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    slider.SetValueWithoutNotify((float)restore);
                    element.Value = (float)restore;
                    element.OnValueChanged?.Invoke((float)restore);
                });

            // 手势语义：PointerDown 打断合并链，拖动中的连续 ChangeEvent 按时间窗合并为一个 step
            slider.RegisterCallback<PointerDownEvent>(_ => binding.BreakMerge());
            slider.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new SliderChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            slider.RegisterCallback<PointerUpEvent>(_ => element.OnCommit?.Invoke(element.Value));
            UIToolkitStyleApplier.ApplyElementStyles(element, slider);
            return slider;
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
