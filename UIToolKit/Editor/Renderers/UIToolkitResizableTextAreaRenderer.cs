using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitResizableTextAreaRenderer : IElementRenderer<W.ResizableTextArea, VisualElement>,
        IElementUpdater<W.ResizableTextArea, VisualElement>
    {
        public VisualElement Render(W.ResizableTextArea element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            // ResizableTextArea doesn't exist in this Unity version; fallback to multiline TextField
            var field = new TextField(element.Label) {
                value = element.Value ?? "",
                multiline = true
            };

            // Snapshot 绑定——Register setter + 提供 Commit() 方法
            var binding = new SnapshotBinding<string>(state?.Scope, element.Key, element.Value ?? "",
                () => element.Value,
                v => { field.SetValueWithoutNotify(v ?? ""); element.Value = v; });

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
                binding.Commit();
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
