using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitTextFieldRenderer : IElementRenderer<W.TextField, VisualElement>
    {
        public VisualElement Render(W.TextField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is UnityEngine.UIElements.TextField reused)
            {
                reused.SetValueWithoutNotify(element.Value ?? string.Empty);
                return reused;
            }

            var field = new UnityEngine.UIElements.TextField
            {
                value = element.Value ?? string.Empty,
                isDelayed = element.IsDelayed,
                isPasswordField = element.IsPassword,
                multiline = element.IsMultiline,
                isReadOnly = element.IsReadOnly,
                maxLength = element.MaxLength
            };

            // Snapshot 绑定——连续输入型，Blur 时提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value ?? string.Empty,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((string)restore);
                    element.Value = (string)restore;
                    element.OnValueChange?.Invoke((string)restore, (string)current);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChange?.Invoke(evt.newValue, evt.previousValue);
                manager.Dispatch(new TextFieldChangeEvent(element, evt.newValue, evt.previousValue));
                // delayed：ChangeEvent 仅在失焦/回车触发一次，即提交点（显式动作 → 打断合并）
                if (element.IsDelayed)
                {
                    binding.BreakMerge();
                    binding.Commit();
                }
                element.NotifyChanged();
            });
            field.RegisterCallback<BlurEvent>(_ =>
            {
                binding.BreakMerge(); // 显式提交点：与上次编辑隔离
                binding.Commit();
                element.OnCommit?.Invoke(element.Value ?? string.Empty);
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct TextFieldChangeEvent
    {
        public W.TextField Source { get; }
        public string NewValue { get; }
        public string PreviousValue { get; }

        public TextFieldChangeEvent(W.TextField source, string newValue, string previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
