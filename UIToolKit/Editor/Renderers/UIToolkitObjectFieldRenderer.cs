using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitObjectFieldRenderer : IElementRenderer<W.ObjectField, VisualElement>
    {
        public VisualElement Render(W.ObjectField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new ObjectField(element.Label) {
                objectType = element.ObjectType,
                value = element.Value,
                allowSceneObjects = element.AllowSceneObjects
            };
            
            field.RegisterValueChangedCallback<UnityEngine.Object>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ObjectFieldChangeEvent(element, evt.newValue, evt.previousValue));
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct ObjectFieldChangeEvent
    {
        public W.ObjectField Source { get; }
        public UnityEngine.Object NewValue { get; }
        public UnityEngine.Object PreviousValue { get; }
        
        public ObjectFieldChangeEvent(W.ObjectField source, UnityEngine.Object newV, UnityEngine.Object prevV)
        {
            Source = source;
            NewValue = newV;
            PreviousValue = prevV;
        }
    }
}
