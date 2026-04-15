using System.Collections.Generic;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class PopupWindow : ContainerElement
    {
        public string Text { get; set; }

        private readonly List<IElement> _children = new List<IElement>();
        public override IEnumerable<IElement> Children => _children;
        public override void Add(IElement element) => _children.Add(element);
        public override IElement Render() => null;

        public PopupWindow(string text = "") { Text = text; }
    }
}
