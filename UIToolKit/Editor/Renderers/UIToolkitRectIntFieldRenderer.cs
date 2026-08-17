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
    public class UIToolkitRectIntFieldRenderer : IElementRenderer<W.RectIntField, VisualElement>
    {
        public VisualElement Render(W.RectIntField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
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
        public W.RectIntField Source { get; }
        public RectInt NewValue { get; }
        public RectInt PreviousValue { get; }

        public RectIntFieldChangeEvent(W.RectIntField source, RectInt newValue, RectInt previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
