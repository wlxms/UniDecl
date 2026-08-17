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
    public class UIToolkitGradientFieldRenderer : IElementRenderer<W.GradientField, VisualElement>
    {
        public VisualElement Render(W.GradientField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is GradientField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new GradientField(element.Label) { value = element.Value };

            // Snapshot 绑定——用 CloneGradient 确保 Record/Undo 拿到的是独立副本。
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => CloneGradient(element.Value),
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((Gradient)restore);
                    element.Value = (Gradient)restore;
                    element.OnValueChanged?.Invoke((Gradient)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = CloneGradient(evt.newValue);
                element.OnValueChanged?.Invoke(element.Value);
                manager.Dispatch(new GradientFieldChangeEvent(element, element.Value, evt.previousValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        private static Gradient CloneGradient(Gradient source)
        {
            if (source == null) return null;
            var copy = new Gradient();
            copy.SetKeys(source.colorKeys, source.alphaKeys);
            copy.mode = source.mode;
            return copy;
        }
    }

    public struct GradientFieldChangeEvent
    {
        public W.GradientField Source { get; }
        public Gradient NewValue { get; }
        public Gradient PreviousValue { get; }

        public GradientFieldChangeEvent(W.GradientField source, Gradient newValue, Gradient previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
