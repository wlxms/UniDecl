using System;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitEnumFieldRenderer : IElementRenderer<W.EnumField, VisualElement>
    {
        public VisualElement Render(W.EnumField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var enumType = element.EnumType ?? typeof(int);
            var enumValues = Enum.GetValues(enumType);
            var currentValue = enumValues.GetValue(element.Value);

            var field = new EnumField(element.Label, (Enum)currentValue);
            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = Convert.ToInt32(evt.newValue);
                element.OnValueChanged?.Invoke(element.Value);
                manager.Dispatch(new EnumFieldChangeEvent(element, element.Value));
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct EnumFieldChangeEvent
    {
        public W.EnumField Source { get; }
        public int Value { get; }
        public EnumFieldChangeEvent(W.EnumField source, int value) { Source = source; Value = value; }
    }
}
