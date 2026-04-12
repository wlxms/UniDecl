using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitScrollViewRenderer : IElementRenderer<W.ScrollView, VisualElement>
    {
        public VisualElement Render(W.ScrollView element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var scrollView = new UnityEngine.UIElements.ScrollView();

            foreach (var child in element.Children)
            {
                var childElement = manager.RenderElement(child);
                if (childElement != null)
                    scrollView.Add(childElement);
            }

            return scrollView;
        }
    }
}
