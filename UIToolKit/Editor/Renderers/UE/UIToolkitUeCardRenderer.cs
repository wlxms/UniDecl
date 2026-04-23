using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using WUE = UniDecl.Runtime.Widgets.UE;

namespace UniDecl.Editor.UIToolKit.Renderers.UE
{
    public class UIToolkitUeCardRenderer : IElementRenderer<WUE.UeCard, VisualElement>
    {
        public VisualElement Render(WUE.UeCard element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Column;
            container.style.alignItems = Align.Stretch;

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
