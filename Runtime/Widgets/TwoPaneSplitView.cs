using System.Collections.Generic;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public enum SplitViewOrientation
    {
        Horizontal = 0,
        Vertical = 1,
    }

    public class TwoPaneSplitView : ContainerElement
    {
        public int FixedPaneIndex { get; set; } = 0;
        public SplitViewOrientation Orientation { get; set; } = SplitViewOrientation.Horizontal;
        public float FixedPaneInitialDimension { get; set; } = 100f;

        private readonly List<IElement> _children = new List<IElement>();
        public override IEnumerable<IElement> Children => _children;
        public override void Add(IElement element) => _children.Add(element);
        public override IElement Render() => null;

        public TwoPaneSplitView(int fixedPaneIndex = 0, SplitViewOrientation orientation = SplitViewOrientation.Horizontal, float fixedPaneInitialDimension = 100f)
        {
            FixedPaneIndex = fixedPaneIndex;
            Orientation = orientation;
            FixedPaneInitialDimension = fixedPaneInitialDimension;
        }
    }
}
