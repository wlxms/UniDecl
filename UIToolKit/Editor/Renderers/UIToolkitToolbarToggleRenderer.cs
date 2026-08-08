using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using W = UniDecl.BuiltIn.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitToolbarToggleRenderer : IElementRenderer<W.ToolbarToggle, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.ToolbarToggle, VisualElement>
    {
        public VisualElement Render(W.ToolbarToggle element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var toggle = new UnityEditor.UIElements.ToolbarToggle() { value = element.Value };
            if (!string.IsNullOrEmpty(element.Text))
                toggle.text = element.Text;

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding<bool>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { toggle.SetValueWithoutNotify(v); element.Value = v; });

            toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ToolbarToggleChangeEvent(element, evt.newValue));
                binding.Commit();  // 瞬时型：ChangeEvent 即提交
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, toggle);
            return toggle;
        }

        public bool TryUpdate(W.ToolbarToggle element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is UnityEditor.UIElements.ToolbarToggle field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.ToolbarToggle f && TryUpdate(f, existing, manager, state);
    }

    public struct ToolbarToggleChangeEvent
    {
        public W.ToolbarToggle Source { get; }
        public bool NewValue { get; }
        public ToolbarToggleChangeEvent(W.ToolbarToggle source, bool newValue) { Source = source; NewValue = newValue; }
    }
}
