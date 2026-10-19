using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
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

            // 行数约束：UITK 无原生行数 API，按单行行高（约 20px）换算 min/max 高度
            const float lineHeight = 20f;
            if (element.MinLines > 0)
                field.style.minHeight = element.MinLines * lineHeight;
            if (element.MaxLines > 0)
                field.style.maxHeight = element.MaxLines * lineHeight;

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
                binding.BreakMerge(); binding.Commit(); // 离散提交点：打断时间窗合并
                element.NotifyChanged();
            });
            field.RegisterCallback<BlurEvent>(_ =>
            {
                binding.BreakMerge(); binding.Commit(); // 离散提交点：打断时间窗合并
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
