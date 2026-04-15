using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitToolbarToggleRenderer : IElementRenderer<W.ToolbarToggle, VisualElement>
    {
        public VisualElement Render(W.ToolbarToggle element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var toggle = new UnityEditor.UIElements.ToolbarToggle() { value = element.Value };
            if (!string.IsNullOrEmpty(element.Text))
                toggle.text = element.Text;

            toggle.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ToolbarToggleChangeEvent(element, evt.newValue));
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
        public ToolbarToggleChangeEvent(W.ToolbarToggle source, bool newValue) { Source = source; NewValue = newValue; }
    }
}
