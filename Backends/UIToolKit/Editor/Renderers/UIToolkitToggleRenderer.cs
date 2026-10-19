using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitToggleRenderer : IElementRenderer<W.Toggle, VisualElement>
    {
        public VisualElement Render(W.Toggle element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is Toggle reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var toggle = new Toggle(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    toggle.SetValueWithoutNotify((bool)restore);
                    element.Value = (bool)restore;
                    element.OnValueChanged?.Invoke((bool)restore);
                });

            toggle.RegisterValueChangedCallback(evt =>
            {
                if (!ReferenceEquals(evt.target, toggle)) return; // 子控件 ChangeEvent 冒泡不处理
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ToggleChangeEvent(element, evt.newValue, evt.previousValue));
                binding.BreakMerge(); // 离散点击：每次独立 step
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, toggle);
            return toggle;
        }
    }

    public struct ToggleChangeEvent
    {
        public W.Toggle Source { get; }
        public bool NewValue { get; }
        public bool PreviousValue { get; }

        public ToggleChangeEvent(W.Toggle source, bool newValue, bool previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
