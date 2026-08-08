using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitMaskFieldRenderer : IElementRenderer<W.MaskField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.MaskField, VisualElement>
    {
        public VisualElement Render(W.MaskField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var choices = element.Choices != null
                ? new System.Collections.Generic.List<string>(element.Choices)
                : new System.Collections.Generic.List<string>();
            var field = new MaskField(choices, element.Value, null);
            field.label = element.Label;

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding<int>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            field.RegisterValueChangedCallback<int>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new MaskFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();  // 瞬时型：ChangeEvent 即提交
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        public bool TryUpdate(W.MaskField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is MaskField field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.MaskField f && TryUpdate(f, existing, manager, state);
    }

    public struct MaskFieldChangeEvent
    {
        public W.MaskField Source { get; }
        public int NewValue { get; }
        public int PreviousValue { get; }
        
        public MaskFieldChangeEvent(W.MaskField source, int newV, int prevV)
        {
            Source = source;
            NewValue = newV;
            PreviousValue = prevV;
        }
    }
}
