using UnityEditor.UIElements;
using System;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UniDecl.Snapshot;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    public class UIToolkitEnumFlagsFieldRenderer : IElementRenderer<W.EnumFlagsField, VisualElement>
    {
        public VisualElement Render(W.EnumFlagsField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            if (existing is EnumFlagsField reused)
            {
                reused.SetValueWithoutNotify((Enum)Enum.ToObject(element.EnumType, element.Value));
                return reused;
            }

            var field = new EnumFlagsField(element.Label,
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
                manager.Dispatch(new EnumFlagsFieldChangeEvent(element, newValue));
                binding.Commit();
                element.NotifyChanged();
            });
            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }
    }

    public struct EnumFlagsFieldChangeEvent
    {
        public W.EnumFlagsField Source { get; }
        public int NewValue { get; }

        public EnumFlagsFieldChangeEvent(W.EnumFlagsField source, int newValue)
        {
            Source = source;
            NewValue = newValue;
        }
    }
}
