using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitToolbarToggleRenderer : IElementRenderer<W.ToolbarToggle, VisualElement>
    {
        public VisualElement Render(W.ToolbarToggle element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is ToolbarToggle reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var toggle = new ToolbarToggle { text = element.Text, value = element.Value };

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
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ToolbarToggleChangeEvent(element, evt.newValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, toggle);
            return toggle;
        }
    }

    public struct ToolbarToggleChangeEvent
    {
        public W.ToolbarToggle Source { get; }
        public bool NewValue { get; }

        public ToolbarToggleChangeEvent(W.ToolbarToggle source, bool newValue)
        {
            Source = source;
            NewValue = newValue;
        }
    }
}
