using System.Collections.Generic;
using UniDecl.Core;

namespace UniDecl.Widgets
{
    public class HorizontalLayout : ContainerElement
    {
        private readonly List<IElement> _children = new List<IElement>();
        public override IEnumerable<IElement> Children => _children;
        public override void Add(IElement element)
        {
            _children.Add(element);
        }

        public override IElement Render()
        {
            return null;
        }

        public HorizontalLayout() { }
        public HorizontalLayout(params IElementComponent[] components) : base(components) { }
    }
}