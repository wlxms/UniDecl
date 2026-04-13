using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolkit.Renderers
{
    public class UIToolkitTextFieldRenderer : IElementRenderer<W.TextField, VisualElement>
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
                        element.OnCommit?.Invoke(element.Value);

                        if (element.IsDelayed)
                            element.NotifyChanged();
                    }
                });
            }

            
            return textField;
        }
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
