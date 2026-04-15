using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitEnumFlagsFieldRenderer : IElementRenderer<W.EnumFlagsField, VisualElement>
    {
        public VisualElement Render(W.EnumFlagsField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var enumType = element.EnumType ?? typeof(int);
            var enumValues = Enum.GetValues(enumType);
            var currentValue = enumValues.GetValue(element.Value);

            var field = new EnumFlagsField(element.Label, (Enum)currentValue);
            
            field.RegisterValueChangedCallback<Enum>(evt =>
            {
                element.Value = Convert.ToInt32(evt.newValue);
                element.OnValueChanged?.Invoke(element.Value);
                manager.Dispatch(new EnumFlagsFieldChangeEvent(element, element.Value));
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct EnumFlagsFieldChangeEvent
    {
        public W.EnumFlagsField Source { get; }
        public int Value { get; }
        
        public EnumFlagsFieldChangeEvent(W.EnumFlagsField source, int value)
        {
            Source = source;
            Value = value;
        }
    }
}
