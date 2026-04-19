using System.Collections.Generic;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class VerticalLayout : ContainerElement
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

        public VerticalLayout() { }
        public VerticalLayout(params IElementComponent[] components) : base(components) { }
    }
}