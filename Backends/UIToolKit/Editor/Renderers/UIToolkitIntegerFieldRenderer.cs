using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitIntegerFieldRenderer : IElementRenderer<W.IntegerField, VisualElement>
    {
        public VisualElement Render(W.IntegerField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is IntegerField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new IntegerField(element.Label) { value = element.Value };

            // Snapshot 绑定——连续输入型，Blur 时提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((int)restore);
                    element.Value = (int)restore;
                    element.OnValueChanged?.Invoke((int)restore, (int)current);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue, evt.previousValue);
                manager.Dispatch(new IntegerFieldChangeEvent(element, evt.newValue, evt.previousValue));
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

    public struct IntegerFieldChangeEvent
    {
        public W.IntegerField Source { get; }
        public int NewValue { get; }
        public int PreviousValue { get; }

        public IntegerFieldChangeEvent(W.IntegerField source, int newValue, int previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
