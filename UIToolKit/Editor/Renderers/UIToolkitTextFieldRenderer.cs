using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitTextFieldRenderer : IElementRenderer<W.TextField, VisualElement>,
        IElementUpdater<W.TextField, VisualElement>
    {
        public VisualElement Render(W.TextField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var textField = new TextField(element.Placeholder ?? "")
            {
                value = element.Value ?? string.Empty,
                isPasswordField = element.IsPassword,
                multiline = element.IsMultiline,
                isReadOnly = element.IsReadOnly,
                isDelayed = element.IsDelayed,
            };

            if (element.MaxLength >= 0)
                textField.maxLength = element.MaxLength;

            // Snapshot 绑定——Register setter + 提供 Commit() 方法
            var binding = new SnapshotBinding<string>(state?.Scope, element.Key, element.Value ?? string.Empty,
                () => element.Value,
                v => { textField.SetValueWithoutNotify(v ?? string.Empty); element.Value = v; });

            textField.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChange?.Invoke(evt.newValue, evt.previousValue);
                manager.Dispatch(new TextFieldChangeEvent(element, evt.newValue, evt.previousValue));

                // 非 delayed 模式下，输入变更即触发增量重建
                if (!element.IsDelayed)
                    element.NotifyChanged();
            });

            textField.RegisterCallback<BlurEvent>(_ =>
            {
                binding.Commit();
                element.OnCommit?.Invoke(element.Value);

                // delayed 模式下沿用提交触发重建
                if (element.IsDelayed)
                    element.NotifyChanged();
            });

            if (!element.IsMultiline && !element.IsPassword)
            {
                textField.RegisterCallback<KeyUpEvent>(e =>
                {
                    if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                    {
                        binding.Commit();
                        element.OnCommit?.Invoke(element.Value);

                        if (element.IsDelayed)
                            element.NotifyChanged();
                    }
                });
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, textField);
            return textField;
        }

        public bool TryUpdate(W.TextField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is TextField textField)
            {
                textField.SetValueWithoutNotify(element.Value ?? "");
                return true;
            }
            return false;
        }

        bool IElementUpdater<VisualElement>.TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.TextField tf && TryUpdate(tf, existing, manager, state);
    }

    public struct TextFieldChangeEvent
    {
        public W.TextField SourceTextField { get; }
        public string NewValue { get; }
        public string PreviousValue { get; }

        public TextFieldChangeEvent(W.TextField source, string newValue, string previousValue)
        {
            SourceTextField = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
