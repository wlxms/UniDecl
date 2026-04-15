using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitVector2IntFieldRenderer : IElementRenderer<W.Vector2IntField, VisualElement>
    {
        public VisualElement Render(W.Vector2IntField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var field = new Vector2IntField(element.Label) { value = element.Value };
            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new Vector2IntFieldChangeEvent(element, evt.newValue, evt.previousValue));
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct Vector2IntFieldChangeEvent
    {
        public W.Vector2IntField Source { get; }
        public Vector2Int NewValue { get; }
        public Vector2Int PreviousValue { get; }

        public Vector2IntFieldChangeEvent(W.Vector2IntField source, Vector2Int newValue, Vector2Int previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
