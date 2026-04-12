using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitVerticalLayoutRenderer : IElementRenderer<VerticalLayout, VisualElement>
    {
        public VisualElement Render(VerticalLayout element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.alignItems = Align.Stretch;

            foreach (var child in element.Children)
            {
                var childElement = manager.RenderElement(child);
                if (childElement != null)
                    container.Add(childElement);
            }

            return container;
        }
    }
}
