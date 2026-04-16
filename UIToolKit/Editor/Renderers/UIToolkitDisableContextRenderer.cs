using UnityEngine.UIElements;
using UniDecl.Runtime.Contexts;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitDisableContextRenderer : IElementRenderer<DisableContext, VisualElement>
    {
        public VisualElement Render(DisableContext element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var childElement = manager.RenderElement(element.Child);
            if (childElement == null) return null;

            childElement.SetEnabled(!element.Value);
            UIToolkitStyleApplier.ApplyElementStyles(element, childElement);
            return childElement;
        }
    }
}
