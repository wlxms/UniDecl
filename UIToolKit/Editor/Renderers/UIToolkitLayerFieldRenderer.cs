using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitLayerFieldRenderer : IElementRenderer<W.LayerField, VisualElement>
    {
        public VisualElement Render(W.LayerField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is LayerField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new LayerField(element.Label) { value = element.Value };

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
                manager.Dispatch(new LayerFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct LayerFieldChangeEvent
    {
        public W.LayerField Source { get; }
        public int NewValue { get; }
        public int PreviousValue { get; }

        public LayerFieldChangeEvent(W.LayerField source, int newValue, int previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
