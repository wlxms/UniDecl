using System;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    /// <summary>
    /// A widget that parses a Markdown string and renders it as styled UI elements.
    ///
    /// Supported Markdown syntax:
    ///   • Headings (# H1 … ###### H6, or setext ====/----)
    ///   • Paragraphs with inline bold (**), italic (*), bold-italic (***), inline code (`)
    ///   • Fenced code blocks (``` or ~~~) with optional language hint
    ///   • Unordered lists (-, *, +) and ordered lists (1., 2., …)
    ///   • Blockquotes (&gt;)
    ///   • Horizontal rules (---, ***, ___)
    ///   • Hyperlinks ([text](url)) — fires <see cref="OnUrlClick"/> when clicked
    ///   • Images (![alt](url)) — fires <see cref="OnUrlClick"/> when clicked
    ///
    /// The widget is rendered entirely by its backend renderer
    /// (<c>UIToolkitMarkdownViewRenderer</c>) and does not produce child elements
    /// declaratively; set <see cref="Markdown"/> and optionally <see cref="OnUrlClick"/>.
    /// </summary>
    public class MarkdownView : Element
    {
        /// <summary>The Markdown source text to parse and render.</summary>
        public string Markdown { get; set; }

        /// <summary>
        /// Optional callback invoked when any hyperlink or image URL in the document is
        /// clicked by the user.  The argument is the raw URL string from the link target.
        ///
        /// Use this to implement custom navigation, deep-link handling, or URL validation
        /// instead of relying on the default system browser behaviour.
        /// </summary>
        public Action<string> OnUrlClick { get; set; }

        /// <summary>
        /// MarkdownView is rendered entirely by its backend renderer.
        /// </summary>
        public override IElement Render() => null;

        public MarkdownView() { }

        public MarkdownView(string markdown) { Markdown = markdown; }

        public MarkdownView(string markdown, Action<string> onUrlClick)
        {
            Markdown = markdown;
            OnUrlClick = onUrlClick;
        }
    }
}
