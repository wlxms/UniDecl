using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
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
