using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitResizableTextAreaRenderer : IElementRenderer<W.ResizableTextArea, VisualElement>
    {
        public VisualElement Render(W.ResizableTextArea element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

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
