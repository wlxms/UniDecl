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
    public class UIToolkitGradientFieldRenderer : IElementRenderer<WU.GradientField, VisualElement>
    {
        public VisualElement Render(WU.GradientField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
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
        public WU.GradientField Source { get; }
        public Gradient NewValue { get; }
        public Gradient PreviousValue { get; }

        public GradientFieldChangeEvent(WU.GradientField source, Gradient newValue, Gradient previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
