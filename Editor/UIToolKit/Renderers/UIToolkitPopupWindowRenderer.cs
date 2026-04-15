using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitPopupWindowRenderer : IElementRenderer<W.PopupWindow, VisualElement>
    {
        public VisualElement Render(W.PopupWindow element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            // UnityEditor.UIElements.PopupWindow doesn't exist in this Unity version; fallback to container
            var container = new VisualElement();
            container.name = element.Text ?? "PopupWindow";

            foreach (var child in element.Children)
            {
                var childVe = manager.RenderElement(child);
                if (childVe != null) container.Add(childVe);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }

    public struct PopupWindowChangeEvent
    {
        public W.PopupWindow Source { get; }
        public PopupWindowChangeEvent(W.PopupWindow source) { Source = source; }
    }
}
