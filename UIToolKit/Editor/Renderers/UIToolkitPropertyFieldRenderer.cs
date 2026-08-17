using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
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
