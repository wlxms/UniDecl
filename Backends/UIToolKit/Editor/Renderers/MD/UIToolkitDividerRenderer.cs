using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using W = UniDecl.Widgets.MD;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers.MD
{
    /// <summary>
    /// UIToolkit renderer for <see cref="W.Divider"/>.
    /// Produces a single <see cref="VisualElement"/> with the <c>ud-md-hr</c> CSS class.
    /// </summary>
    public class UIToolkitDividerRenderer : IElementRenderer<W.Divider, VisualElement>
    {
        public VisualElement Render(W.Divider element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var hr = new VisualElement();
            hr.AddToClassList("ud-md-hr");

            UIToolkitStyleApplier.ApplyElementStyles(element, hr);
            return hr;
        }
    }
}
