using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitToolbarMenuRenderer : IElementRenderer<W.ToolbarMenu, VisualElement>
    {
        public VisualElement Render(W.ToolbarMenu element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var menu = new UnityEditor.UIElements.ToolbarMenu();
            if (!string.IsNullOrEmpty(element.Text))
                menu.text = element.Text;

            if (element.OnClick != null)
            {
                menu.menu.AppendAction(element.Text ?? "Menu", a =>
                {
                    element.OnClick?.Invoke();
                    manager.Dispatch(new ToolbarMenuClickEvent(element));
                    element.NotifyChanged();
                });
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, menu);
            return menu;
        }
    }

    public struct ToolbarMenuClickEvent
    {
        public W.ToolbarMenu SourceMenu { get; }
        public ToolbarMenuClickEvent(W.ToolbarMenu source) { SourceMenu = source; }
    }
}
