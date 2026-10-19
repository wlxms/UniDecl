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
    public class UIToolkitColorFieldRenderer : IElementRenderer<WU.ColorField, VisualElement>
    {
        public VisualElement Render(WU.ColorField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is ColorField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new ColorField(element.Label)
            {
                value = element.Value,
                showAlpha = element.ShowAlpha,
                showEyeDropper = element.ShowEyeDropper
            };

            // Snapshot 绑定——瞬时型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((Color)restore);
                    element.Value = (Color)restore;
                    element.OnValueChanged?.Invoke((Color)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ColorFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct ColorFieldChangeEvent
    {
        public WU.ColorField Source { get; }
        public Color NewValue { get; }
        public Color PreviousValue { get; }

        public ColorFieldChangeEvent(WU.ColorField source, Color newValue, Color previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
