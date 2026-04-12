using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitIntegerFieldRenderer : IElementRenderer<W.IntegerField, VisualElement>
    {
        public VisualElement Render(W.IntegerField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new IntegerField { value = element.Value };
            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue, evt.previousValue);
                manager.Dispatch(new IntegerFieldChangeEvent(element, evt.newValue, evt.previousValue));
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

            return field;
        }
    }

    public struct IntegerFieldChangeEvent
    {
        public W.IntegerField Source { get; }
        public int NewValue { get; }
        public int PreviousValue { get; }

        public IntegerFieldChangeEvent(W.IntegerField source, int newValue, int previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
