using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using UITKStyle = UniDecl.UIToolKit.Runtime.UITKStyle;
using W = UniDecl.BuiltIn.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    public class UIToolkitGradientFieldRenderer : IElementRenderer<W.GradientField, VisualElement>,
        IElementUpdater<VisualElement>, IElementUpdater<W.GradientField, VisualElement>
    {
        public VisualElement Render(W.GradientField element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;
            var field = new GradientField(element.Label) { value = element.Value };
            var scope = state?.Scope;

            // Gradient 是引用类型，Unity GradientField 弹窗原地修改对象（prevRef == newRef）。
            // 用 SnapshotBinding + CloneGradient 确保 Record/Undo 拿到的是独立副本。
            // 必须用 ValueStep（走 Register setter）而非 ObjectDiffStep（不走 setter，VE 不会更新）。
            var binding = new SnapshotBinding<Gradient>(scope, element.Key, CloneGradient(element.Value),
                () => CloneGradient(element.Value),
                v => { field.SetValueWithoutNotify(v); element.Value = v; });

            // ChangeEvent 只更新值 + 转发事件，不 Commit 不 NotifyChanged。
            // Gradient 的 Color Picker 是弹窗，拖拽期间持续触发 ChangeEvent，
            // 在此处 Commit/NotifyChanged 会抢走弹窗焦点导致编辑中断。
            field.RegisterValueChangedCallback(evt =>
            {
                element.Value = evt.newValue;
                element.OnValueChanged?.Invoke(evt.newValue);
                manager.Dispatch(new GradientFieldChangeEvent(element, evt.newValue, evt.previousValue));
            });

            // Blur 时才提交——关闭 Color Picker / 离开字段时产生一个 undo step。
            // 不调 NotifyChanged——Gradient 的 Color Picker 是独立 popup，
            // Rebuild 会干扰 popup 焦点。Gradient 字段通常不参与条件依赖（ShowIf 等）。
            field.RegisterCallback<BlurEvent>(_ =>
            {
                binding.Commit();
            });

            UIToolkitStyleApplier.ApplyElementStyles(element, field);
            return field;
        }

        /// <summary>
        /// 深拷贝 Gradient——复制 colorKeys 和 alphaKeys 数组。
        /// Gradient 是引用类型且 Unity 弹窗会原地修改，必须拷贝才能用于 ValueStep 快照。
        /// </summary>
        private static Gradient CloneGradient(Gradient source)
        {
            if (source == null) return new Gradient();
            return new Gradient
            {
                colorKeys = (GradientColorKey[])source.colorKeys.Clone(),
                alphaKeys = (GradientAlphaKey[])source.alphaKeys.Clone(),
                mode = source.mode,
            };
        }

        public bool TryUpdate(W.GradientField element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (existing is GradientField field)
            {
                field.SetValueWithoutNotify(element.Value);
                return true;
            }
            return false;
        }

        public bool TryUpdate(IElement element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
            => element is W.GradientField f && TryUpdate(f, existing, manager, state);
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
