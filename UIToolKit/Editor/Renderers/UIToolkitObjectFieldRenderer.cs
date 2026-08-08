using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitObjectFieldRenderer : IElementRenderer<W.ObjectField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.ObjectField, VisualElement>
    {
        public VisualElement Render(W.ObjectField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new ObjectField(element.Label) {
                objectType = element.ObjectType,
                value = element.Value,
                allowSceneObjects = element.AllowSceneObjects
            };

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding<UnityEngine.Object>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            field.RegisterValueChangedCallback<UnityEngine.Object>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ObjectFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();  // 瞬时型：ChangeEvent 即提交
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        public bool TryUpdate(W.ObjectField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is ObjectField field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.ObjectField f && TryUpdate(f, existing, manager, state);
    }

    public struct ObjectFieldChangeEvent
    {
        public W.ObjectField Source { get; }
        public UnityEngine.Object NewValue { get; }
        public UnityEngine.Object PreviousValue { get; }
        
        public ObjectFieldChangeEvent(W.ObjectField source, UnityEngine.Object newV, UnityEngine.Object prevV)
        {
            Source = source;
            NewValue = newV;
            PreviousValue = prevV;
        }
    }
}
