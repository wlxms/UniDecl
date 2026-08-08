using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitColorFieldRenderer : IElementRenderer<W.ColorField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.ColorField, VisualElement>
    {
        public VisualElement Render(W.ColorField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var field = new ColorField(element.Label)
            {
                value = element.Value,
                showAlpha = element.ShowAlpha,
                showEyeDropper = element.ShowEyeDropper,
            };

            // Snapshot 绑定——Register setter + 提供 Commit() 方法
            var binding = new SnapshotBinding<Color>(state?.Scope, element.Key, element.Value,
                () => element.Value,
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            field.RegisterValueChangedCallback<Color>(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new ColorFieldChangeEvent(element, evt.newValue, evt.previousValue));
            });

            // ColorField 的 Color Picker 拖拽期间会持续触发 ChangeEvent（每帧一次），
            // 用 Blur 作为提交点——关闭 Color Picker / 离开字段时才产生一个 undo step。
            field.RegisterCallback<BlurEvent>(_ =>
            {
                binding.Commit();
                element.NotifyChanged();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        public bool TryUpdate(W.ColorField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is ColorField field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.ColorField f && TryUpdate(f, existing, manager, state);
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
