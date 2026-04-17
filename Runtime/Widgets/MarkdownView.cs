using System;
using UniDecl.Runtime.Components;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.MD;

namespace UniDecl.Runtime.Widgets
{
    /// <summary>
    /// A widget that parses a Markdown string and renders it as a tree of semantic
    /// widget children — no factory, no backend-specific renderer required.
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
    /// <see cref="Render"/> parses <see cref="Markdown"/> and returns a
    /// <see cref="ScrollView"/> that contains the composed widget tree.  The framework
    /// renders the tree using the registered renderers for each widget type.
    /// </summary>
    public class MarkdownView : Element
    {
        /// <summary>The Markdown source text to parse and render.</summary>
        public string Markdown { get; set; }

        /// <summary>
        /// Optional callback invoked when any hyperlink or image URL in the document is
        /// clicked by the user.  The argument is the raw URL string from the link target.
        /// </summary>
        public Action<string> OnUrlClick { get; set; }

        public MarkdownView() { }

        public MarkdownView(string markdown) { Markdown = markdown; }

        public MarkdownView(string markdown, Action<string> onUrlClick)
        {
            Markdown = markdown;
            OnUrlClick = onUrlClick;
        }

        /// <summary>
        /// Parses <see cref="Markdown"/> and composes a <see cref="ScrollView"/> whose
        /// children are semantic widgets (<see cref="H1"/>–<see cref="H6"/>,
        /// <see cref="RichText"/>, <see cref="CodeBlock"/>, <see cref="Blockquote"/>,
        /// <see cref="Divider"/>).  No factory or backend renderer is involved here.
        /// </summary>
        public override IElement Render()
        {
            var scrollView = new ScrollView();
            scrollView.With(new InlineStyle("ud-markdown"));

            if (!string.IsNullOrEmpty(Markdown))
            {
                var blocks = MdParser.Parse(Markdown);
                foreach (var block in blocks)
                {
                    var child = BlockToElement(block, OnUrlClick);
                    if (child != null)
                        scrollView.Add(child);
                }
            }

            return scrollView;
        }

        // =====================================================================
        // Block → widget mapping (inline, no factory)
        // =====================================================================

        private static IElement BlockToElement(MdBlock block, Action<string> onUrlClick)
        {
            switch (block.Type)
            {
                case MdBlockType.Heading:
                    return HeadingElement(block.Level, block.RawText);

                case MdBlockType.Paragraph:
                    return new RichText(block.Inlines, onUrlClick)
                        .With(new InlineStyle("ud-md-paragraph"));

                case MdBlockType.CodeBlock:
                    return new CodeBlock(block.Language, block.RawText);

                case MdBlockType.HorizontalRule:
                    return new Divider();

                case MdBlockType.Blockquote:
                {
                    var bq = new Blockquote();
                    foreach (var inner in block.Blocks)
                    {
                        var child = BlockToElement(inner, onUrlClick);
                        if (child != null)
                            bq.Add(child);
                    }
                    return bq;
                }

                case MdBlockType.UnorderedList:
                case MdBlockType.OrderedList:
                {
                    bool ordered = block.Type == MdBlockType.OrderedList;
                    var list = new VerticalLayout();
                    list.With(new InlineStyle(ordered ? "ud-md-ol" : "ud-md-ul"));
                    foreach (var item in block.Items)
                    {
                        var row = new HorizontalLayout();
                        row.With(new InlineStyle("ud-md-li"));

                        var bullet = new Label(ordered ? $"{item.OrderIndex}." : "•");
                        bullet.With(new InlineStyle("ud-md-li-bullet"));
                        row.Add(bullet);

                        var content = new RichText(item.Inlines, onUrlClick);
                        content.With(new InlineStyle("ud-md-li-content"));
                        row.Add(content);

                        list.Add(row);
                    }
                    return list;
                }

                default:
                    return null;
            }
        }

        private static IElement HeadingElement(int level, string text)
        {
            switch (level)
            {
                case 1: return new H1(text);
                case 2: return new H2(text);
                case 3: return new H3(text);
                case 4: return new H4(text);
                case 5: return new H5(text);
                default: return new H6(text);
            }
        }
    }
}
