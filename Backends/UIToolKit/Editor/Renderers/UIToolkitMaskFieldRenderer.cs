using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitMaskFieldRenderer : IElementRenderer<W.MaskField, VisualElement>
    {
        public VisualElement Render(W.MaskField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is MaskField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new MaskField(element.Label, new System.Collections.Generic.List<string>(element.Choices), element.Value);

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((int)restore);
                    element.Value = (int)restore;
                    element.OnValueChanged?.Invoke((int)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new MaskFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct MaskFieldChangeEvent
    {
        public W.MaskField Source { get; }
        public int NewValue { get; }
        public int PreviousValue { get; }

        public MaskFieldChangeEvent(W.MaskField source, int newValue, int previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
