using UnityEngine.UIElements;
using UniDecl.Core;
using W = UniDecl.Widgets;
using UniDecl.Backend.UIToolKit.Editor.Style;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitToolbarRenderer : IElementRenderer<W.Toolbar, VisualElement>
    {
        public VisualElement Render(W.Toolbar element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var toolbar = new UnityEditor.UIElements.Toolbar();

            foreach (var child in element.Children)
            {
                var childElement = manager.RenderElement(child);
                if (childElement != null)
                    toolbar.Add(childElement);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, toolbar);
            return toolbar;
        }
    }
}
