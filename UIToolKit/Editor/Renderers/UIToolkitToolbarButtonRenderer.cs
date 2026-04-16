using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitToolbarButtonRenderer : IElementRenderer<W.ToolbarButton, VisualElement>
    {
        public VisualElement Render(W.ToolbarButton element, IElementRenderHost<VisualElement> manager, ElementState state)
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
