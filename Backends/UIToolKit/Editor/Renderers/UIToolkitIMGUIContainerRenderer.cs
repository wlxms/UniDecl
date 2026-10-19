using WU = UniDecl.Unity.Widgets;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitIMGUIContainerRenderer : IElementRenderer<WU.IMGUIContainer, VisualElement>
    {
        public VisualElement Render(WU.IMGUIContainer element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var imguiContainer = new IMGUIContainer(element.OnGUIHandler);

            UIToolkitStyleApplier.ApplyElementStyles(element, imguiContainer);
            return imguiContainer;
        }
    }
}
