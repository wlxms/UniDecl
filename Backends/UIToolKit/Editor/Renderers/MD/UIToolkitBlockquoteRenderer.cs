using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using W = UniDecl.Widgets.MD;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers.MD
{
    /// <summary>
    /// UIToolkit renderer for <see cref="W.Blockquote"/>.
    /// Produces a column-flex <see cref="VisualElement"/> with the
    /// <c>ud-md-blockquote</c> CSS class, then recursively renders each child widget.
    /// </summary>
    public class UIToolkitBlockquoteRenderer : IElementRenderer<W.Blockquote, VisualElement>
    {
        public VisualElement Render(W.Blockquote element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.AddToClassList("ud-md-blockquote");
            // 轴向由 .ud-md-blockquote 承担（DefaultStyle.uss）

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
