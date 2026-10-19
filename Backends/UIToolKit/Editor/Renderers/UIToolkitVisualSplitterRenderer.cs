using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitVisualSplitterRenderer : IElementRenderer<W.VisualSplitter, VisualElement>
    {
        public VisualElement Render(W.VisualSplitter element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            // VisualSplitter is internal in this Unity version; fallback to a container
            var container = new VisualElement();
            // 轴向 / 伸缩由 .ud-splitter 承担（DefaultStyle.uss）

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
