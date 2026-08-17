using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitMinMaxSliderRenderer : IElementRenderer<W.MinMaxSlider, VisualElement>
    {
        public VisualElement Render(W.MinMaxSlider element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is MinMaxSlider reused)
            {
                reused.SetValueWithoutNotify(new Vector2(element.MinValue, element.MaxValue));
                return reused;
            }

            var slider = new MinMaxSlider(element.Label, element.MinValue, element.MaxValue,
                element.LowLimit, element.HighLimit);

            // Snapshot 绑定——瞬时型，ChangeEvent 即提交（连续拖动靠 Merge 合并）
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => new Vector2(element.MinValue, element.MaxValue),
                (restore, current, changes) =>
                {
                    var v = (Vector2)restore;
                    slider.SetValueWithoutNotify(v);
                    element.MinValue = v.x;
                    element.MaxValue = v.y;
                    element.OnValueChanged?.Invoke(v.x, v.y);
                });

            // 手势语义：PointerDown 打断合并链，拖动中的连续 ChangeEvent 按时间窗合并为一个 step
            slider.RegisterCallback<PointerDownEvent>(_ => binding.BreakMerge());
            slider.RegisterValueChangedCallback(evt =>
            {
                element.MinValue = evt.newValue.x;
                element.MaxValue = evt.newValue.y;
                element.OnValueChanged?.Invoke(evt.newValue.x, evt.newValue.y);
                manager.Dispatch(new MinMaxSliderChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            slider.RegisterCallback<PointerUpEvent>(_ => element.OnCommit?.Invoke(element.MinValue, element.MaxValue));
            UIToolkitStyleApplier.ApplyElementStyles(element, slider);
            return slider;
        }
    }

    public struct MinMaxSliderChangeEvent
    {
        public W.MinMaxSlider Source { get; }
        public Vector2 NewValue { get; }
        public Vector2 PreviousValue { get; }

        public MinMaxSliderChangeEvent(W.MinMaxSlider source, Vector2 newValue, Vector2 previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
