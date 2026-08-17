using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UniDecl.BuiltIn.Runtime.Snapshot;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitColorFieldRenderer : IElementRenderer<W.ColorField, VisualElement>
    {
        public VisualElement Render(W.ColorField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
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
        public W.ColorField Source { get; }
        public Color NewValue { get; }
        public Color PreviousValue { get; }

        public ColorFieldChangeEvent(W.ColorField source, Color newValue, Color previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
