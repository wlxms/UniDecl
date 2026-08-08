using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitVector2IntFieldRenderer : IElementRenderer<W.Vector2IntField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.Vector2IntField, VisualElement>
    {
        public VisualElement Render(W.Vector2IntField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var field = new Vector2IntField(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时选择型，Commit 在 ChangeEvent 回调里调用
            var binding = new SnapshotBinding<Vector2Int>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new Vector2IntFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        public bool TryUpdate(W.Vector2IntField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is Vector2IntField field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.Vector2IntField f && TryUpdate(f, existing, manager, state);
    }

    public struct Vector2IntFieldChangeEvent
    {
        public W.Vector2IntField Source { get; }
        public Vector2Int NewValue { get; }
        public Vector2Int PreviousValue { get; }

        public Vector2IntFieldChangeEvent(W.Vector2IntField source, Vector2Int newValue, Vector2Int previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
