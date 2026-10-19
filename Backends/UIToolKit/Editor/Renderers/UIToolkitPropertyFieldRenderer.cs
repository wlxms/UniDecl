using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitPropertyFieldRenderer : IElementRenderer<W.PropertyField, VisualElement>
    {
        public VisualElement Render(W.PropertyField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new PropertyField();
            field.bindingPath = element.BindingPath;
            if (!string.IsNullOrEmpty(element.Label))
                field.label = element.Label;

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }
}
