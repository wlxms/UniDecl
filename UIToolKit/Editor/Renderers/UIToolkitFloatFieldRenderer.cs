using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitFloatFieldRenderer : IElementRenderer<W.FloatField, VisualElement>
    {
        public VisualElement Render(W.FloatField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is FloatField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new FloatField(element.Label) { value = element.Value };

            // Snapshot 绑定——连续输入型，Blur 时提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((float)restore);
                    element.Value = (float)restore;
                    element.OnValueChanged?.Invoke((float)restore, (float)current);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue, evt.previousValue);
                manager.Dispatch(new FloatFieldChangeEvent(element, evt.newValue, evt.previousValue));
                element.NotifyChanged();
            });
            field.RegisterCallback<BlurEvent>(_ =>
            {
                binding.Commit();
                element.OnCommit?.Invoke(element.Value);
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct FloatFieldChangeEvent
    {
        public W.FloatField Source { get; }
        public float NewValue { get; }
        public float PreviousValue { get; }

        public FloatFieldChangeEvent(W.FloatField source, float newValue, float previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
