using System;
using System.Collections.Generic;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.MD;

namespace UniDecl.Runtime.Widgets.MD
{
    /// <summary>
    /// Renders a list of parsed Markdown inline elements as styled UI text.
    ///
    /// Fast path (no links/images): a single Unity rich-text Label.
    /// Slow path (links present):  a flex-wrap row of Labels and clickable elements.
    ///
    /// URL clicks are forwarded to <see cref="OnUrlClick"/>.
    /// </summary>
    public class RichText : Element
    {
        /// <summary>Parsed inline elements to render.</summary>
        public List<MdInline> Inlines { get; set; }

        /// <summary>Callback invoked when a hyperlink or image URL is clicked.</summary>
        public Action<string> OnUrlClick { get; set; }

        public RichText() { }

        public RichText(List<MdInline> inlines, Action<string> onUrlClick = null)
        {
            Inlines = inlines;
            OnUrlClick = onUrlClick;
        }

        /// <summary>Rendered entirely by <c>UIToolkitRichTextRenderer</c>.</summary>
        public override IElement Render() => null;
    }
}
