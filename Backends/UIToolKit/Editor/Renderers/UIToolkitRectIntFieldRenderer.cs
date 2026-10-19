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
    public class UIToolkitRectIntFieldRenderer : IElementRenderer<WU.RectIntField, VisualElement>
    {
        public VisualElement Render(WU.RectIntField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is RectIntField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new RectIntField(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((RectInt)restore);
                    element.Value = (RectInt)restore;
                    element.OnValueChanged?.Invoke((RectInt)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new RectIntFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct RectIntFieldChangeEvent
    {
        public WU.RectIntField Source { get; }
        public RectInt NewValue { get; }
        public RectInt PreviousValue { get; }

        public RectIntFieldChangeEvent(WU.RectIntField source, RectInt newValue, RectInt previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
