using System.Collections.Generic;
using UniDecl.BuiltIn.Runtime.Core;

namespace UniDecl.BuiltIn.Runtime.Widgets
{
    /// <summary>滚动条可见性（与 UIToolKit ScrollerVisibility 语义对齐）。</summary>
    public enum ScrollerVisibility
    {
        Auto,
        AlwaysVisible,
        Hidden,
    }

    public class ScrollView : ContainerElement
    {
        private readonly List<IElement> _children = new List<IElement>();
        public override IEnumerable<IElement> Children => _children;
        public override void Add(IElement element) => _children.Add(element);
        public override IElement Render() => null;

        /// <summary>横向滚动条可见性。属性面板类窗口应设 Hidden，避免无内容时出现空滚动条。</summary>
        public ScrollerVisibility HorizontalScrollerVisibility { get; set; } = ScrollerVisibility.Auto;

        /// <summary>纵向滚动条可见性。</summary>
        public ScrollerVisibility VerticalScrollerVisibility { get; set; } = ScrollerVisibility.Auto;
    }
}
