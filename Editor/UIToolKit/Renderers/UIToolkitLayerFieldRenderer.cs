using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitLayerFieldRenderer : IElementRenderer<W.LayerField, VisualElement>
    {
        public VisualElement Render(W.LayerField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new LayerField(element.Label) { value = element.Value };
            
            field.RegisterValueChangedCallback<int>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new LayerFieldChangeEvent(element, evt.newValue, evt.previousValue));
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct LayerFieldChangeEvent
    {
        public W.LayerField Source { get; }
        public int NewValue { get; }
        public int PreviousValue { get; }
        
        public LayerFieldChangeEvent(W.LayerField source, int newV, int prevV)
        {
            Source = source;
            NewValue = newV;
            PreviousValue = prevV;
        }
    }
}
