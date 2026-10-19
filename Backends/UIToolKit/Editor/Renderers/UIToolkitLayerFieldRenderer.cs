using WU = UniDecl.Unity.Widgets;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitLayerFieldRenderer : IElementRenderer<WU.LayerField, VisualElement>
    {
        public VisualElement Render(WU.LayerField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
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
        public WU.LayerField Source { get; }
        public int NewValue { get; }
        public int PreviousValue { get; }

        public LayerFieldChangeEvent(WU.LayerField source, int newValue, int previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
