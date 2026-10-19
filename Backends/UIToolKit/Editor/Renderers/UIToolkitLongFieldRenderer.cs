using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitLongFieldRenderer : IElementRenderer<W.LongField, VisualElement>
    {
        public VisualElement Render(W.LongField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is LongField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new LongField(element.Label) { value = element.Value };

            // Snapshot 绑定——连续输入型，Blur 时提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((long)restore);
                    element.Value = (long)restore;
                    element.OnValueChanged?.Invoke((long)restore, (long)current);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue, evt.previousValue);
                manager.Dispatch(new LongFieldChangeEvent(element, evt.newValue, evt.previousValue));
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

    public struct LongFieldChangeEvent
    {
        public W.LongField Source { get; }
        public long NewValue { get; }
        public long PreviousValue { get; }

        public LongFieldChangeEvent(W.LongField source, long newValue, long previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
