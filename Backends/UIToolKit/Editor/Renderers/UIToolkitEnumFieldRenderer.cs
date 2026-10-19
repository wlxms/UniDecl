using UnityEditor.UIElements;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitEnumFieldRenderer : IElementRenderer<W.EnumField, VisualElement>
    {
        public VisualElement Render(W.EnumField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is EnumField reused)
            {
                reused.SetValueWithoutNotify((Enum)Enum.ToObject(element.EnumType, element.Value));
                return reused;
            }

            var field = new EnumField(element.Label,
                (Enum)Enum.ToObject(element.EnumType, element.Value));

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    var v = (int)restore;
                    field.SetValueWithoutNotify((Enum)Enum.ToObject(element.EnumType, v));
                    element.Value = v;
                    element.OnValueChanged?.Invoke(v);
                });

            field.RegisterValueChangedCallback(evt =>
            {
                var newValue = Convert.ToInt32(evt.newValue);
                element.Value = newValue;
                element.OnValueChanged?.Invoke(newValue);
                manager.Dispatch(new EnumFieldChangeEvent(element, newValue));
                binding.BreakMerge(); binding.Commit(); // 离散提交点：打断时间窗合并
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct EnumFieldChangeEvent
    {
        public W.EnumField Source { get; }
        public int NewValue { get; }

        public EnumFieldChangeEvent(W.EnumField source, int newValue)
        {
            Source = source;
            NewValue = newValue;
        }
    }
}
