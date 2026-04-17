using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets.MD;

namespace UniDecl.Editor.UIToolKit.Renderers.MD
{
    /// <summary>
    /// UIToolkit renderer for <see cref="W.Blockquote"/>.
    /// Produces a column-flex <see cref="VisualElement"/> with the
    /// <c>ud-md-blockquote</c> CSS class, then recursively renders each child widget.
    /// </summary>
    public class UIToolkitBlockquoteRenderer : IElementRenderer<W.Blockquote, VisualElement>
    {
        public VisualElement Render(W.Blockquote element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.AddToClassList("ud-md-blockquote");
            container.style.flexDirection = FlexDirection.Column;

            foreach (var child in element.Children)
            {
                var ve = manager.RenderElement(child);
                if (ve != null)
                    container.Add(ve);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }
}
