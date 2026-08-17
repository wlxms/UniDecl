using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitObjectFieldRenderer : IElementRenderer<W.ObjectField, VisualElement>
    {
        public VisualElement Render(W.ObjectField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is ObjectField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new ObjectField(element.Label)
            {
                objectType = element.ObjectType,
                value = element.Value,
                allowSceneObjects = element.AllowSceneObjects
            };

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((UnityEngine.Object)restore);
                    element.Value = (UnityEngine.Object)restore;
                    element.OnValueChanged?.Invoke((UnityEngine.Object)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ObjectFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
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

        public ObjectFieldChangeEvent(W.ObjectField source, UnityEngine.Object newValue, UnityEngine.Object previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
