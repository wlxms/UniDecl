using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitEnumFlagsFieldRenderer : IElementRenderer<W.EnumFlagsField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.EnumFlagsField, VisualElement>
    {
        public VisualElement Render(W.EnumFlagsField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var enumType = element.EnumType ?? typeof(int);
            var enumValues = Enum.GetValues(enumType);
            var currentValue = enumValues.GetValue(element.Value);

            var field = new EnumFlagsField(element.Label, (Enum)currentValue);

            // Snapshot 绑定——瞬时选择型，ChangeEvent 即提交
            var binding = new SnapshotBinding<int>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v =>
                {
                    var ev = enumValues.GetValue(v);
                    field.SetValueWithoutNotify((Enum)ev);
                    element.Value = v;
                });

            field.RegisterValueChangedCallback<Enum>(evt =>
            {
                element.Value = Convert.ToInt32(evt.newValue);
                element.OnValueChanged?.Invoke(element.Value);
                manager.Dispatch(new EnumFlagsFieldChangeEvent(element, element.Value));
                binding.Commit();  // 瞬时型：ChangeEvent 即提交
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        public bool TryUpdate(W.EnumFlagsField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is EnumFlagsField field)
            {
                field.SetValueWithoutNotify((Enum)Enum.ToObject(field.value?.GetType() ?? typeof(Enum), element.Value));
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.EnumFlagsField f && TryUpdate(f, existing, manager, state);
    }

    public struct EnumFlagsFieldChangeEvent
    {
        public W.EnumFlagsField Source { get; }
        public int Value { get; }
        
        public EnumFlagsFieldChangeEvent(W.EnumFlagsField source, int value)
        {
            Source = source;
            Value = value;
        }
    }
}
