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
    /// <summary>
    /// 枚举按钮组渲染器——等宽 Button 平铺网格：允许换行、允许收缩，
    /// 值多时折行而不是撑破 Value 区域。
    /// 底色 / 描边 / 选中态 / 悬停态全部由 .ud-enum-toggle-group__button* 承担（见 DefaultStyle.uss），
    /// 值变化时只切换 class，不再直接写 style.color。
    /// </summary>
    public class UIToolkitEnumToggleGroupRenderer : IElementRenderer<W.EnumToggleGroup, VisualElement>
    {
        const string ButtonClass = "ud-enum-toggle-group__button";
        const string SelectedClass = "ud-enum-toggle-group__button--selected";

        public VisualElement Render(W.EnumToggleGroup element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null || element.EnumType == null || !element.EnumType.IsEnum) return null;

            var root = existing is VisualElement ve ? ve : new VisualElement();
            root.Clear();
            // 方向 / 换行 / 伸缩由 .ud-enum-toggle-group 承担（见 DefaultStyle.uss）

            var names = element.GetChoiceNames();
            var selected = element.IndexOfValue();

            var binding = new SnapshotBinding(state?.Scope, element.Key,
                () => element.Value,
                (restore, current, changes) =>
                {
                    element.Value = (int)restore;
                    element.OnValueChanged?.Invoke(element.Value);
                    element.Rebuild();
                });

            for (int i = 0; i < names.Length; i++)
            {
                var index = i;
                var btn = new UnityEngine.UIElements.Button(ApplySelection) { text = names[i] };
                btn.AddToClassList(ButtonClass);
                if (i == selected)
                    btn.AddToClassList(SelectedClass);
                root.Add(btn);

                void ApplySelection()
                {
                    var newValue = element.ValueAtIndex(index);
                    if (newValue == element.Value) return;
                    element.Value = newValue;
                    element.OnValueChanged?.Invoke(newValue);
                    manager.Dispatch(new EnumToggleGroupChangeEvent(element, newValue));
                    binding.BreakMerge(); binding.Commit(); // 离散提交点：打断时间窗合并
                    element.Rebuild(); // 重渲染刷新各按钮高亮
                }
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, root);
            return root;
        }
    }

    public struct EnumToggleGroupChangeEvent
    {
        public W.EnumToggleGroup Source { get; }
        public int NewValue { get; }

        public EnumToggleGroupChangeEvent(W.EnumToggleGroup source, int newValue)
        {
            Source = source;
            NewValue = newValue;
        }
    }
}
