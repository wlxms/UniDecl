using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitIMGUIContainerRenderer : IElementRenderer<W.IMGUIContainer, VisualElement>
    {
        public VisualElement Render(W.IMGUIContainer element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var imguiContainer = new IMGUIContainer(element.OnGUIHandler);

            UIToolkitStyleApplier.ApplyElementStyles(element, imguiContainer);
            return imguiContainer;
        }
    }
}
