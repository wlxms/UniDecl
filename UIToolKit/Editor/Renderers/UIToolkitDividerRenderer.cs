using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    /// <summary>
    /// UIToolkit renderer for <see cref="W.Divider"/>.
    /// Produces a single <see cref="VisualElement"/> with the <c>ud-md-hr</c> CSS class.
    /// </summary>
    public class UIToolkitDividerRenderer : IElementRenderer<W.Divider, VisualElement>
    {
        public VisualElement Render(W.Divider element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var hr = new VisualElement();
            hr.AddToClassList("ud-md-hr");

            UIToolkitStyleApplier.ApplyElementStyles(element, hr);
            return hr;
        }
    }
}
