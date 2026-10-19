using WU = UniDecl.Unity.Widgets;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitBoundsIntFieldRenderer : IElementRenderer<WU.BoundsIntField, VisualElement>
    {
        public VisualElement Render(WU.BoundsIntField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is BoundsIntField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new BoundsIntField(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((BoundsInt)restore);
                    element.Value = (BoundsInt)restore;
                    element.OnValueChanged?.Invoke((BoundsInt)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new BoundsIntFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct BoundsIntFieldChangeEvent
    {
        public WU.BoundsIntField Source { get; }
        public BoundsInt NewValue { get; }
        public BoundsInt PreviousValue { get; }

        public BoundsIntFieldChangeEvent(WU.BoundsIntField source, BoundsInt newValue, BoundsInt previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
