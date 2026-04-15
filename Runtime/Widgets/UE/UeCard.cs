using System.Collections.Generic;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets.UE
{
    public class UeCard : ContainerElement
    {
        private readonly List<IElement> _children = new List<IElement>();

        public string Title { get; set; }

        public override IEnumerable<IElement> Children => _children;

        public override void Add(IElement element)
        {
            if (element != null)
                _children.Add(element);
        }

        public override IElement Render() => null;

        public UeCard() { }

        public UeCard(string title)
        {
            Title = title;
        }
    }
}
