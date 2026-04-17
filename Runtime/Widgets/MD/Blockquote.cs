using System.Collections.Generic;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets.MD
{
    /// <summary>
    /// A block-quote container widget.  Children are added by the caller and rendered
    /// with a left-border accent via the <c>ud-md-blockquote</c> CSS class.
    ///
    /// Rendered by <c>UIToolkitBlockquoteRenderer</c>.
    /// </summary>
    public class Blockquote : ContainerElement
    {
        private readonly List<IElement> _children = new List<IElement>();

        public override IEnumerable<IElement> Children => _children;

        public override void Add(IElement element) => _children.Add(element);

        /// <summary>Rendered entirely by <c>UIToolkitBlockquoteRenderer</c>.</summary>
        public override IElement Render() => null;
    }
}
