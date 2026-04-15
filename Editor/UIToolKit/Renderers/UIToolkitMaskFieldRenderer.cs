using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitMaskFieldRenderer : IElementRenderer<W.MaskField, VisualElement>
    {
        public VisualElement Render(W.MaskField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var choices = element.Choices != null
                ? new System.Collections.Generic.List<string>(element.Choices)
                : new System.Collections.Generic.List<string>();
            var field = new MaskField(choices, element.Value, null);
            field.label = element.Label;
            
            field.RegisterValueChangedCallback<int>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new MaskFieldChangeEvent(element, evt.newValue, evt.previousValue));
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct MaskFieldChangeEvent
    {
        public W.MaskField Source { get; }
        public int NewValue { get; }
        public int PreviousValue { get; }
        
        public MaskFieldChangeEvent(W.MaskField source, int newV, int prevV)
        {
            Source = source;
            NewValue = newV;
            PreviousValue = prevV;
        }
    }
}
