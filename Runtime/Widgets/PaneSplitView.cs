using System;
using System.Collections.Generic;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    /// <summary>
    /// 多栏分割器选项——控制每个 pane 的尺寸和弹性。
    /// </summary>
    public class PaneOption
    {
        /// <summary>初始固定尺寸（对应原生 TwoPaneSplitView 的 fixedPaneInitialDimension）</summary>
        public float? InitialSize { get; set; }
        public float? MinWidth { get; set; }
        public float? MaxWidth { get; set; }
        public float? FlexGrow { get; set; }
        public float? FlexShrink { get; set; }
    }

    /// <summary>
    /// 通用多栏分割器——支持 N 个 pane + N-1 个可拖拽分割手柄。
    /// 渲染器内部用嵌套原生 TwoPaneSplitView 实现，拖拽由原生控件处理。
    /// </summary>
    public class PaneSplitView : ContainerElement
    {
        public enum Direction { Horizontal, Vertical }

        public Direction SplitDirection { get; set; } = Direction.Horizontal;
        public List<PaneOption> PaneOptions { get; set; } = new List<PaneOption>();

        private readonly List<IElement> _children = new List<IElement>();

        public override IEnumerable<IElement> Children => _children;
        public override void Add(IElement element) => _children.Add(element);
        public override IElement Render() => null;

        public PaneSplitView() { }

        public PaneSplitView WithPane(int index, Action<PaneOption> configure)
        {
            while (PaneOptions.Count <= index)
                PaneOptions.Add(new PaneOption());
            configure(PaneOptions[index]);
            return this;
        }
    }

    /// <summary>水平方向多栏分割器</summary>
    public class HorizontalPaneSplitView : PaneSplitView
    {
        public HorizontalPaneSplitView() { SplitDirection = Direction.Horizontal; }
    }

    /// <summary>垂直方向多栏分割器</summary>
    public class VerticalPaneSplitView : PaneSplitView
    {
        public VerticalPaneSplitView() { SplitDirection = Direction.Vertical; }
    }
}
