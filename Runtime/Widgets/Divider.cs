using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    /// <summary>
    /// A horizontal rule / thematic break.  Rendered as a single-pixel line via
    /// the <c>ud-md-hr</c> CSS class by <c>UIToolkitDividerRenderer</c>.
    /// </summary>
    public class Divider : Element
    {
        /// <summary>Rendered entirely by <c>UIToolkitDividerRenderer</c>.</summary>
        public override IElement Render() => null;
    }
}
