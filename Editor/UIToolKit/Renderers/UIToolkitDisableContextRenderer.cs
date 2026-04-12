using UnityEngine.UIElements;
using UniDecl.Runtime.Contexts;
using UniDecl.Runtime.Core;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitDisableContextRenderer : IElementRenderer<DisableContext, VisualElement>
    {
        public VisualElement Render(DisableContext element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var childElement = manager.RenderElement(element.Child);
            if (childElement == null) return null;

            childElement.SetEnabled(!element.Value);
            return childElement;
        }
    }
}
