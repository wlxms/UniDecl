using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitVisualSplitterRenderer : IElementRenderer<W.VisualSplitter, VisualElement>
    {
        public VisualElement Render(W.VisualSplitter element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            // VisualSplitter is internal in this Unity version; fallback to a container
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
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

    public struct VisualSplitterChangeEvent
    {
        public W.VisualSplitter Source { get; }
        public VisualSplitterChangeEvent(W.VisualSplitter source) { Source = source; }
    }
}
