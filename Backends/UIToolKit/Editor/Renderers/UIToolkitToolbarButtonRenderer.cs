using UnityEngine.UIElements;
using UniDecl.Core;
using W = UniDecl.Widgets;
using UniDecl.Backend.UIToolKit.Editor.Style;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitToolbarButtonRenderer : IElementRenderer<W.ToolbarButton, VisualElement>
    {
        public VisualElement Render(W.ToolbarButton element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var button = new UnityEditor.UIElements.ToolbarButton(() =>
            {
                manager.Dispatch(new ToolbarButtonClickEvent(element));
                element.OnClick?.Invoke();
                element.NotifyChanged();
            });

            if (!string.IsNullOrEmpty(element.Text))
                button.text = element.Text;

            UIToolkitStyleApplier.ApplyElementStyles(element, button);
            return button;
        }
    }

    public struct ToolbarButtonClickEvent
    {
        public W.ToolbarButton SourceButton { get; }
        public ToolbarButtonClickEvent(W.ToolbarButton source) { SourceButton = source; }
    }
}
