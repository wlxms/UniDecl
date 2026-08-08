using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitDoubleFieldRenderer : IElementRenderer<W.DoubleField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.DoubleField, VisualElement>
    {
        public VisualElement Render(W.DoubleField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new DoubleField { value = element.Value };

            // Snapshot 绑定——Register setter + 提供 Commit() 方法
            var binding = new SnapshotBinding<double>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue, evt.previousValue);
                manager.Dispatch(new DoubleFieldChangeEvent(element, evt.newValue, evt.previousValue));
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

        public bool TryUpdate(W.DoubleField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is DoubleField field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        bool IElementUpdater<VisualElement>.TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.DoubleField f && TryUpdate(f, existing, manager, state);
    }

    public struct DoubleFieldChangeEvent
    {
        public W.DoubleField Source { get; }
        public double NewValue { get; }
        public double PreviousValue { get; }

        public DoubleFieldChangeEvent(W.DoubleField source, double newValue, double previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
