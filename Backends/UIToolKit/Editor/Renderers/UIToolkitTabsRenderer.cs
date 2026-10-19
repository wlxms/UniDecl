using System;
using System.Linq;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UniDecl.Core;
using UniDecl.Snapshot;
using UniDecl.Backend.UIToolKit.Editor.Style;
using UITKStyle = UniDecl.Backend.UIToolKit.UITKStyle;
using W = UniDecl.Widgets;

namespace UniDecl.Backend.UIToolKit.Editor.Renderers
{
    /// <summary>
    /// Tabs 渲染器——Toolbar 页签条（等宽 ToolbarToggle，可换行可收缩）+ 当前页内容。
    /// 页内容元素经 ContainerElement.Children 进入 DOM 树，此处对选中页 RenderElement。
    /// 切页通过 element.Rebuild() 走统一的增量重建路径，复用已有 VisualElement。
    /// 切页同时纳入 Undo/Redo（Tabs.SelectionUndoable）：离散动作，BreakMerge 打断合并后立即成 step。
    /// </summary>
    public class UIToolkitTabsRenderer : IElementRenderer<W.Tabs, VisualElement>
    {
        public VisualElement Render(W.Tabs element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null || element.Pages.Count == 0) return null;

            var root = existing ?? new VisualElement();
            root.Clear();
            // 布局/皮肤由 .ud-tabs 与 .ud-tabs__* 承担（见 DefaultStyle.uss），此处只搭建结构

            var strip = new Toolbar();
            strip.AddToClassList("ud-tabs__strip");
            var content = new VisualElement();
            content.AddToClassList("ud-tabs__content");
            root.Add(strip);
            root.Add(content);

            var selected = Mathf.Clamp(element.SelectedIndex, 0, element.Pages.Count - 1);
            element.SelectedIndex = selected;

            // 撤销路径：Key 稳定（PropertyGrid 用 tabview_<字段路径>），缺失时按页签标题合成。
            // 每次 Render 重建 binding——Manager 按 Path 换代覆盖，历史 step 由 Path 兜底到当代元素。
            SnapshotBinding undoBinding = null;
            if (element.SelectionUndoable)
            {
                var undoPath = !string.IsNullOrEmpty(element.Key)
                    ? element.Key
                    : "tabs_" + string.Join("|", element.Pages.Select(p => p.Title));
                undoBinding = new SnapshotBinding(state?.Scope, undoPath,
                    () => element.SelectedIndex,
                    (restore, current, changes) =>
                    {
                        var index = restore is int i ? i : 0;
                        if (index < 0 || index >= element.Pages.Count) index = 0;
                        element.SelectedIndex = index;
                        // 同步通知：宿主借此更新跨重建的选中记忆，Refresh 后仍停留在恢复页
                        element.OnSelectionChanged?.Invoke(index);
                    });
            }

            for (int i = 0; i < element.Pages.Count; i++)
            {
                var page = element.Pages[i];
                var toggle = new ToolbarToggle { text = page.Title, value = i == selected };
                toggle.AddToClassList("ud-tabs__toggle");
                var index = i;
                toggle.RegisterValueChangedCallback(evt =>
                {
                    // 保持单选：非选中页签被取消勾选时恢复；切换时更新选中页并整树重渲染
                    if (!evt.newValue) { toggle.SetValueWithoutNotify(true); return; }
                    if (element.SelectedIndex == index) return;
                    element.SelectedIndex = index;
                    element.OnSelectionChanged?.Invoke(index);
                    // 切页是离散动作：打断时间窗合并，立即落一个 undo step。
                    // Rebuild 之前提交——重建会换代出新 binding，此处仍属当代绑定。
                    undoBinding?.BreakMerge();
                    undoBinding?.Commit();
                    element.Rebuild();
                });
                strip.Add(toggle);
            }

            var pageVE = manager.RenderElement(element.Pages[selected].Content);
            if (pageVE != null)
                content.Add(pageVE);

            UIToolkitStyleApplier.ApplyElementStyles(element, root);
            return root;
        }
    }
}
