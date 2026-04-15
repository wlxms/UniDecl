using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitVector3IntFieldRenderer : IElementRenderer<W.Vector3IntField, VisualElement>
    {
        public VisualElement Render(W.Vector3IntField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var field = new Vector3IntField(element.Label) { value = element.Value };
            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new Vector3IntFieldChangeEvent(element, evt.newValue, evt.previousValue));
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct Vector3IntFieldChangeEvent
    {
        public W.Vector3IntField Source { get; }
        public Vector3Int NewValue { get; }
        public Vector3Int PreviousValue { get; }

        public Vector3IntFieldChangeEvent(W.Vector3IntField source, Vector3Int newValue, Vector3Int previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
