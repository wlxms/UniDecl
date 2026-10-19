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
    public class UIToolkitVector2FieldRenderer : IElementRenderer<WU.Vector2Field, VisualElement>
    {
        public VisualElement Render(WU.Vector2Field element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is Vector2Field reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new Vector2Field(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((Vector2)restore);
                    element.Value = (Vector2)restore;
                    element.OnValueChanged?.Invoke((Vector2)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new Vector2FieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct Vector2FieldChangeEvent
    {
        public WU.Vector2Field Source { get; }
        public Vector2 NewValue { get; }
        public Vector2 PreviousValue { get; }

        public Vector2FieldChangeEvent(WU.Vector2Field source, Vector2 newValue, Vector2 previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
