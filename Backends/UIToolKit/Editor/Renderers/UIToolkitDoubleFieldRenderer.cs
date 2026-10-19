using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitDoubleFieldRenderer : IElementRenderer<W.DoubleField, VisualElement>
    {
        public VisualElement Render(W.DoubleField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is DoubleField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new DoubleField(element.Label) { value = element.Value };

            // Snapshot 绑定——连续输入型，Blur 时提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((double)restore);
                    element.Value = (double)restore;
                    element.OnValueChanged?.Invoke((double)restore, (double)current);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue, evt.previousValue);
                manager.Dispatch(new DoubleFieldChangeEvent(element, evt.newValue, evt.previousValue));
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

    public struct DoubleFieldChangeEvent
    {
        public W.DoubleField Source { get; }
        public double NewValue { get; }
        public double PreviousValue { get; }

        public DoubleFieldChangeEvent(W.DoubleField source, double newValue, double previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
