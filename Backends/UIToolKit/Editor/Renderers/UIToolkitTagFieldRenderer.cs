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
    public class UIToolkitTagFieldRenderer : IElementRenderer<WU.TagField, VisualElement>
    {
        public VisualElement Render(WU.TagField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is TagField reused)
            {
                reused.SetValueWithoutNotify(element.Value ?? string.Empty);
                return reused;
            }

            var field = new TagField(element.Label) { value = element.Value ?? string.Empty };

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value ?? string.Empty,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((string)restore);
                    element.Value = (string)restore;
                    element.OnValueChanged?.Invoke((string)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new TagFieldChangeEvent(element, evt.newValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct TagFieldChangeEvent
    {
        public WU.TagField Source { get; }
        public string NewValue { get; }

        public TagFieldChangeEvent(WU.TagField source, string newValue)
        {
            Source = source;
            NewValue = newValue;
        }
    }
}
