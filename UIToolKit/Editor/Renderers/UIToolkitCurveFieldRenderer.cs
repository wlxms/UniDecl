using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitCurveFieldRenderer : IElementRenderer<W.CurveField, VisualElement>
    {
        public VisualElement Render(W.CurveField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var field = new CurveField(element.Label) { value = element.Value };
            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new CurveFieldChangeEvent(element, evt.newValue, evt.previousValue));
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct CurveFieldChangeEvent
    {
        public W.CurveField Source { get; }
        public AnimationCurve NewValue { get; }
        public AnimationCurve PreviousValue { get; }

        public CurveFieldChangeEvent(W.CurveField source, AnimationCurve newValue, AnimationCurve previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
