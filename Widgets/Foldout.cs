using System.Collections.Generic;
using UniDecl.Core;

namespace UniDecl.Widgets
{
    public class Foldout : ContainerElement
    {
        public string Text { get; set; }
        public bool Value { get; set; } = true;
        private readonly List<IElement> _children = new List<IElement>();
        public override IEnumerable<IElement> Children => _children;
        public override void Add(IElement element) => _children.Add(element);
        public override IElement Render() => null;

        public Foldout(string text) { Text = text; }
        public Foldout(string text, params IElementComponent[] components) : base(components) { Text = text; }
    }
}
