using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitLongFieldRenderer : IElementRenderer<W.LongField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.LongField, VisualElement>
    {
        public VisualElement Render(W.LongField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new LongField { value = element.Value };

            // Snapshot 绑定——Register setter + 提供 Commit() 方法
            var binding = new SnapshotBinding<long>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue, evt.previousValue);
                manager.Dispatch(new LongFieldChangeEvent(element, evt.newValue, evt.previousValue));
            });

            field.RegisterCallback<BlurEvent>(_ =>
            {
                binding.Commit();
                element.OnCommit?.Invoke(element.Value);
                element.NotifyChanged();
            });

            field.RegisterCallback<KeyUpEvent>(e =>
            {
                if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    binding.Commit();
                    element.OnCommit?.Invoke(element.Value);
                    element.NotifyChanged();
                }
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        public bool TryUpdate(W.LongField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is LongField field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        bool IElementUpdater<VisualElement>.TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.LongField f && TryUpdate(f, existing, manager, state);
    }

    public struct LongFieldChangeEvent
    {
        public W.LongField Source { get; }
        public long NewValue { get; }
        public long PreviousValue { get; }

        public LongFieldChangeEvent(W.LongField source, long newValue, long previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
