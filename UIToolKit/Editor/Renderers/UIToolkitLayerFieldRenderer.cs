using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitLayerFieldRenderer : IElementRenderer<W.LayerField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.LayerField, VisualElement>
    {
        public VisualElement Render(W.LayerField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new LayerField(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding<int>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            field.RegisterValueChangedCallback<int>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new LayerFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();  // 瞬时型：ChangeEvent 即提交
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        public bool TryUpdate(W.LayerField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is LayerField field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.LayerField f && TryUpdate(f, existing, manager, state);
    }

    public struct LayerFieldChangeEvent
    {
        public W.LayerField Source { get; }
        public int NewValue { get; }
        public int PreviousValue { get; }
        
        public LayerFieldChangeEvent(W.LayerField source, int newV, int prevV)
        {
            Source = source;
            NewValue = newV;
            PreviousValue = prevV;
        }
    }
}
