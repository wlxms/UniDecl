using System.Collections.Generic;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    /// <summary>
    /// PaneSplitView 渲染器——递归嵌套原生 TwoPaneSplitView 实现多栏可拖拽布局。
    /// N 个 children → 嵌套 N-1 个原生 TwoPaneSplitView，拖拽由原生控件处理。
    /// </summary>
    public class UIToolkitPaneSplitViewRenderer : IElementRenderer<W.PaneSplitView, VisualElement>
    {
        public VisualElement Render(W.PaneSplitView element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var children = new List<IElement>(element.Children);
            if (children.Count == 0) return new VisualElement();

            // 单个子元素，直接渲染
            if (children.Count == 1)
            {
                var single = manager.RenderElement(children[0]) ?? new VisualElement();
                single.style.flexGrow = 1;
                ApplyPaneOption(single, element.PaneOptions, 0);
                UIToolkitStyleApplier.ApplyElementStyles(element, single);
                return single;
            }

            var orientation = element.SplitDirection == W.PaneSplitView.Direction.Horizontal
                ? TwoPaneSplitViewOrientation.Horizontal
                : TwoPaneSplitViewOrientation.Vertical;

            var root = BuildNestedSplitView(children, element.PaneOptions, orientation, manager, 0);
            if (root != null)
            {
                root.style.flexGrow = 1;
                UIToolkitStyleApplier.ApplyElementStyles(element, root);
            }
            return root;
        }

        private VisualElement BuildNestedSplitView(
            List<IElement> children, List<W.PaneOption> options,
            TwoPaneSplitViewOrientation orientation,
            IElementRenderHost<VisualElement> manager, int index)
        {
            if (index >= children.Count) return null;

            var firstPane = manager.RenderElement(children[index]) ?? new VisualElement();
            ApplyPaneOption(firstPane, options, index);

            // 最后一个子元素，直接返回
            if (index == children.Count - 1)
            {
                firstPane.style.flexGrow = 1;
                return firstPane;
            }

            // 渲染剩余部分的嵌套容器
            var remainingContainer = BuildNestedSplitView(children, options, orientation, manager, index + 1)
                ?? new VisualElement();

            // 创建原生 TwoPaneSplitView (构造函数: fixedIndex, fixedSize, orientation)
            var splitView = new UnityEngine.UIElements.TwoPaneSplitView(
                0,
                GetInitialDimension(options, index),
                orientation);
            splitView.style.flexGrow = 1;

            splitView.Add(firstPane);
            splitView.Add(remainingContainer);

            return splitView;
        }

        private float GetInitialDimension(List<W.PaneOption> options, int index)
        {
            var opt = index < options.Count ? options[index] : null;
            return opt?.InitialSize ?? 200f;
        }

        private void ApplyPaneOption(VisualElement pane, List<W.PaneOption> options, int index)
        {
            var opt = index < options.Count ? options[index] : null;
            if (opt == null) return;
            if (opt.MinWidth.HasValue) pane.style.minWidth = opt.MinWidth.Value;
            if (opt.MaxWidth.HasValue) pane.style.maxWidth = opt.MaxWidth.Value;
            if (opt.FlexGrow.HasValue) pane.style.flexGrow = opt.FlexGrow.Value;
            if (opt.FlexShrink.HasValue) pane.style.flexShrink = opt.FlexShrink.Value;
        }
    }
}
