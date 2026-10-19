using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Widgets;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitHorizontalLayoutRenderer : IElementRenderer<HorizontalLayout, VisualElement>
    {
        public VisualElement Render(HorizontalLayout element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            // 轴向 / 交叉轴对齐见 .ud-horizontal-layout（DefaultStyle.uss）

            foreach (var child in element.Children)
            {
                var childElement = manager.RenderElement(child);
                if (childElement != null)
                    container.Add(childElement);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }
    }
}
