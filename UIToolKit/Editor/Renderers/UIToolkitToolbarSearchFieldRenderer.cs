using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitToolbarSearchFieldRenderer : IElementRenderer<W.ToolbarSearchField, VisualElement>
    {
        public VisualElement Render(W.ToolbarSearchField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is ToolbarSearchField reused)
            {
                reused.SetValueWithoutNotify(element.Value ?? string.Empty);
                return reused;
            }

            var field = new ToolbarSearchField { value = element.Value ?? string.Empty };

            // Snapshot 绑定——连续输入型，Blur 时提交
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
                manager.Dispatch(new ToolbarSearchFieldChangeEvent(element, evt.newValue));
                element.NotifyChanged();
            });
            field.RegisterCallback<BlurEvent>(_ =>
            {
                binding.Commit();
                element.OnCommit?.Invoke(element.Value ?? string.Empty);
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct ToolbarSearchFieldChangeEvent
    {
        public W.ToolbarSearchField Source { get; }
        public string NewValue { get; }

        public ToolbarSearchFieldChangeEvent(W.ToolbarSearchField source, string newValue)
        {
            Source = source;
            NewValue = newValue;
        }
    }
}
