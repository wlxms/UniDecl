using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitResizableTextAreaRenderer : IElementRenderer<W.ResizableTextArea, VisualElement>,
        IElementUpdater<W.ResizableTextArea, VisualElement>
    {
        public VisualElement Render(W.ResizableTextArea element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            UnityEngine.Debug.Log($"[Undo] ResizableTextArea.Render: element.Value='{element.Value}', element.Label='{element.Label}'");

            // ResizableTextArea doesn't exist in this Unity version; fallback to multiline TextField
            var field = new TextField(element.Label) {
                value = element.Value ?? "",
                multiline = true
            };

            field.RegisterValueChangedCallback(evt =>
            {
                var oldValue = element.Value ?? "";
                var newValue = evt.newValue ?? "";

                element.Value = newValue;
                element.OnValueChanged?.Invoke(newValue, oldValue);
                manager.Dispatch(new ResizableTextAreaChangeEvent(element, newValue, oldValue));
            });

            field.RegisterCallback<BlurEvent>(_ =>
            {
                element.OnCommit?.Invoke(element.Value ?? "");
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        public bool TryUpdate(W.ResizableTextArea element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is TextField textField)
            {
                UnityEngine.Debug.Log($"[Undo] ResizableTextArea.TryUpdate: element.Value='{element.Value}', textField.value='{textField.value}', textField.text='{textField.text}'");
                textField.SetValueWithoutNotify(element.Value ?? "");
                return true;
            }
            return false;
        }

        bool IElementUpdater<VisualElement>.TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.ResizableTextArea rta && TryUpdate(rta, existing, manager, state);
    }

    public struct ResizableTextAreaChangeEvent
    {
        public W.ResizableTextArea Source { get; }
        public string NewValue { get; }
        public string PreviousValue { get; }

        public ResizableTextAreaChangeEvent(W.ResizableTextArea source, string newValue, string previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
