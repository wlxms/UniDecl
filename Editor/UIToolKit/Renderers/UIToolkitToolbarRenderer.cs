using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;
using UniDecl.Editor.UIToolKit.Style;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitToolbarRenderer : IElementRenderer<W.Toolbar, VisualElement>
    {
        public VisualElement Render(W.Toolbar element, IElementRenderHost<VisualElement> manager, ElementState state)
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
