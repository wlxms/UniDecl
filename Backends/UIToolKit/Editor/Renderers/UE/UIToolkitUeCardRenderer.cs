using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using WUE = UniDecl.Widgets.UE;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers.UE
{
    public class UIToolkitUeCardRenderer : IElementRenderer<WUE.UeCard, VisualElement>
    {
        public VisualElement Render(WUE.UeCard element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            // 轴向 / 交叉轴对齐由 .ud-card 承担（DefaultStyle.uss）

            if (!string.IsNullOrEmpty(element.Title))
            {
                var title = new Label(element.Title);
                title.AddToClassList("ud-card-title");
                container.Add(title);
            }

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
