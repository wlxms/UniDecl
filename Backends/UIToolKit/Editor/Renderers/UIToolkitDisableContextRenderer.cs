using UnityEngine.UIElements;
using UniDecl.Core.Contexts;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitDisableContextRenderer : IElementRenderer<DisableContext, VisualElement>
    {
        public VisualElement Render(DisableContext element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
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
