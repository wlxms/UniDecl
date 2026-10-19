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
    public class UIToolkitVector3FieldRenderer : IElementRenderer<WU.Vector3Field, VisualElement>
    {
        public VisualElement Render(WU.Vector3Field element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is Vector3Field reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new Vector3Field(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((Vector3)restore);
                    element.Value = (Vector3)restore;
                    element.OnValueChanged?.Invoke((Vector3)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new Vector3FieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct Vector3FieldChangeEvent
    {
        public WU.Vector3Field Source { get; }
        public Vector3 NewValue { get; }
        public Vector3 PreviousValue { get; }

        public Vector3FieldChangeEvent(WU.Vector3Field source, Vector3 newValue, Vector3 previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
