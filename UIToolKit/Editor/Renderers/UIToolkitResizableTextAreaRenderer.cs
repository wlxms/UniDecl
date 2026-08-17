using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitResizableTextAreaRenderer : IElementRenderer<W.ResizableTextArea, VisualElement>
    {
        public VisualElement Render(W.ResizableTextArea element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is UnityEngine.UIElements.TextField reused)
            {
                reused.SetValueWithoutNotify(element.Value ?? string.Empty);
                return reused;
            }

            var field = new UnityEngine.UIElements.TextField(element.Label)
            {
                value = element.Value ?? string.Empty,
                multiline = true,
                isDelayed = true
            };

            // Snapshot 绑定——连续输入型，Blur 时提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value ?? string.Empty,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((string)restore);
                    element.Value = (string)restore;
                    element.OnValueChanged?.Invoke((string)restore, (string)current);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue, evt.previousValue);
                manager.Dispatch(new ResizableTextAreaChangeEvent(element, evt.newValue));
                // isDelayed=true：ChangeEvent 仅在失焦/回车触发一次，即提交点
                binding.Commit();
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

    public struct ResizableTextAreaChangeEvent
    {
        public W.ResizableTextArea Source { get; }
        public string NewValue { get; }

        public ResizableTextAreaChangeEvent(W.ResizableTextArea source, string newValue)
        {
            Source = source;
            NewValue = newValue;
        }
    }
}
