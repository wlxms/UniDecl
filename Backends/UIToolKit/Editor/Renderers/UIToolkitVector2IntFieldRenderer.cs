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
    public class UIToolkitVector2IntFieldRenderer : IElementRenderer<WU.Vector2IntField, VisualElement>
    {
        public VisualElement Render(WU.Vector2IntField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is Vector2IntField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new Vector2IntField(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((Vector2Int)restore);
                    element.Value = (Vector2Int)restore;
                    element.OnValueChanged?.Invoke((Vector2Int)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new Vector2IntFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct Vector2IntFieldChangeEvent
    {
        public WU.Vector2IntField Source { get; }
        public Vector2Int NewValue { get; }
        public Vector2Int PreviousValue { get; }

        public Vector2IntFieldChangeEvent(WU.Vector2IntField source, Vector2Int newValue, Vector2Int previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
