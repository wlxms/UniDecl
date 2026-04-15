using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitDoubleFieldRenderer : IElementRenderer<W.DoubleField, VisualElement>
    {
        public VisualElement Render(W.DoubleField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new DoubleField { value = element.Value };
            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue, evt.previousValue);
                manager.Dispatch(new DoubleFieldChangeEvent(element, evt.newValue, evt.previousValue));
            });

            field.RegisterCallback<BlurEvent>(_ =>
            {
                element.OnCommit?.Invoke(element.Value);
                element.NotifyChanged();
            });

            field.RegisterCallback<KeyUpEvent>(e =>
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    element.OnCommit?.Invoke(element.Value);
                    element.NotifyChanged();
                }
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct DoubleFieldChangeEvent
    {
        public W.DoubleField Source { get; }
        public double NewValue { get; }
        public double PreviousValue { get; }

        public DoubleFieldChangeEvent(W.DoubleField source, double newValue, double previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
