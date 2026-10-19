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
    public class UIToolkitCurveFieldRenderer : IElementRenderer<WU.CurveField, VisualElement>
    {
        public VisualElement Render(WU.CurveField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
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
        public WU.CurveField Source { get; }
        public AnimationCurve NewValue { get; }
        public AnimationCurve PreviousValue { get; }

        public CurveFieldChangeEvent(WU.CurveField source, AnimationCurve newValue, AnimationCurve previousValue)
        {
            Source = source;
            NewValue = newValue;
            PreviousValue = previousValue;
        }
    }
}
