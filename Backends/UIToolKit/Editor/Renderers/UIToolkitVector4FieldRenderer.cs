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
    public class UIToolkitVector4FieldRenderer : IElementRenderer<WU.Vector4Field, VisualElement>
    {
        public VisualElement Render(WU.Vector4Field element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is Vector4Field reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new Vector4Field(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((Vector4)restore);
                    element.Value = (Vector4)restore;
                    element.OnValueChanged?.Invoke((Vector4)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new Vector4FieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct Vector4FieldChangeEvent
    {
        public WU.Vector4Field Source { get; }
        public Vector4 NewValue { get; }
        public Vector4 PreviousValue { get; }

        public Vector4FieldChangeEvent(WU.Vector4Field source, Vector4 newValue, Vector4 previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
