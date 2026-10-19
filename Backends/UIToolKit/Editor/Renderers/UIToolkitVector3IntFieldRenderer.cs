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
    public class UIToolkitVector3IntFieldRenderer : IElementRenderer<WU.Vector3IntField, VisualElement>
    {
        public VisualElement Render(WU.Vector3IntField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is Vector3IntField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new Vector3IntField(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((Vector3Int)restore);
                    element.Value = (Vector3Int)restore;
                    element.OnValueChanged?.Invoke((Vector3Int)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new Vector3IntFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct Vector3IntFieldChangeEvent
    {
        public WU.Vector3IntField Source { get; }
        public Vector3Int NewValue { get; }
        public Vector3Int PreviousValue { get; }

        public Vector3IntFieldChangeEvent(WU.Vector3IntField source, Vector3Int newValue, Vector3Int previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
