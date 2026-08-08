using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitTagFieldRenderer : IElementRenderer<W.TagField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.TagField, VisualElement>
    {
        public VisualElement Render(W.TagField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new TagField(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding<string>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            field.RegisterValueChangedCallback<string>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new TagFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();  // 瞬时型：ChangeEvent 即提交
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        public bool TryUpdate(W.TagField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is TagField field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.TagField f && TryUpdate(f, existing, manager, state);
    }

    public struct TagFieldChangeEvent
    {
        public W.TagField Source { get; }
        public string NewValue { get; }
        public string PreviousValue { get; }
        
        public TagFieldChangeEvent(W.TagField source, string newV, string prevV)
        {
            Source = source;
            NewValue = newV;
            PreviousValue = prevV;
        }
    }
}
