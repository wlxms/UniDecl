using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitFloatFieldRenderer : IElementRenderer<W.FloatField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.FloatField, VisualElement>
    {
        public VisualElement Render(W.FloatField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new FloatField { value = element.Value };

            // Snapshot 绑定——Register setter + 提供 Commit() 方法
            var binding = new SnapshotBinding<float>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue, evt.previousValue);
                manager.Dispatch(new FloatFieldChangeEvent(element, evt.newValue, evt.previousValue));
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

        public bool TryUpdate(W.FloatField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is FloatField field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        bool IElementUpdater<VisualElement>.TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.FloatField f && TryUpdate(f, existing, manager, state);
    }

    public struct FloatFieldChangeEvent
    {
        public W.FloatField Source { get; }
        public float NewValue { get; }
        public float PreviousValue { get; }

        public FloatFieldChangeEvent(W.FloatField source, float newValue, float previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
