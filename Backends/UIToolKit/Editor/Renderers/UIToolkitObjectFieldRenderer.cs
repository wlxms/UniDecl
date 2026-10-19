using WU = UniDecl.Unity.Widgets;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitObjectFieldRenderer : IElementRenderer<WU.ObjectField, VisualElement>
    {
        public VisualElement Render(WU.ObjectField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
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
        public WU.ObjectField Source { get; }
        public UnityEngine.Object NewValue { get; }
        public UnityEngine.Object PreviousValue { get; }

        public ObjectFieldChangeEvent(WU.ObjectField source, UnityEngine.Object newValue, UnityEngine.Object previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
