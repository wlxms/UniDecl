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
    public class UIToolkitCurveFieldRenderer : IElementRenderer<W.CurveField, VisualElement>
    {
        public VisualElement Render(W.CurveField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is CurveField reused)
            {
                reused.SetValueWithoutNotify(element.Value);
                return reused;
            }

            var field = new CurveField(element.Label) { value = element.Value };
            // 团结引擎无 curveColor；范围经 ranges (xMin, yMin, w, h) 设置
            field.ranges = new Rect(element.MinX, element.MinY,
                element.MaxX - element.MinX, element.MaxY - element.MinY);

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    field.SetValueWithoutNotify((AnimationCurve)restore);
                    element.Value = (AnimationCurve)restore;
                    element.OnValueChanged?.Invoke((AnimationCurve)restore);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new CurveFieldChangeEvent(element, evt.newValue, evt.previousValue));
                binding.Commit();
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
