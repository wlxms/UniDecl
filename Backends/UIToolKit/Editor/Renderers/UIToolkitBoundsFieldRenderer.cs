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
    public class UIToolkitBoundsFieldRenderer : IElementRenderer<WU.BoundsField, VisualElement>
    {
        public VisualElement Render(WU.BoundsField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is BoundsField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new BoundsField(element.Label) { value = element.Value };

            // Snapshot 绑定——瞬时型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((Bounds)restore);
                    element.Value = (Bounds)restore;
                    element.OnValueChanged?.Invoke((Bounds)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new BoundsFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct BoundsFieldChangeEvent
    {
        public WU.BoundsField Source { get; }
        public Bounds NewValue { get; }
        public Bounds PreviousValue { get; }

        public BoundsFieldChangeEvent(WU.BoundsField source, Bounds newValue, Bounds previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
