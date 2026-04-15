using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitTwoPaneSplitViewRenderer : IElementRenderer<W.TwoPaneSplitView, VisualElement>
    {
        public VisualElement Render(W.TwoPaneSplitView element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            // TwoPaneSplitView doesn't exist in UnityEditor.UIElements in this Unity version; fallback to container
            var container = new VisualElement();
            container.style.flexDirection = element.Orientation == W.SplitViewOrientation.Horizontal
                ? FlexDirection.Row
                : FlexDirection.Column;
            container.style.flexGrow = 1;

            foreach (var child in element.Children)
            {
                var childVe = manager.RenderElement(child);
                if (childVe != null) container.Add(childVe);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }

    public struct TwoPaneSplitViewChangeEvent
    {
        public W.TwoPaneSplitView Source { get; }
        public TwoPaneSplitViewChangeEvent(W.TwoPaneSplitView source) { Source = source; }
    }
}
