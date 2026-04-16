using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.MD;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    /// <summary>
    /// UIToolkit renderer for <see cref="W.MarkdownView"/>.
    /// Parses the <see cref="W.MarkdownView.Markdown"/> string with <see cref="MdParser"/>
    /// and builds a <see cref="ScrollView"/> containing styled <see cref="VisualElement"/>s.
    ///
    /// URL forwarding: every hyperlink and image fires
    /// <see cref="W.MarkdownView.OnUrlClick"/> when clicked.
    /// </summary>
    public class UIToolkitMarkdownViewRenderer : IElementRenderer<W.MarkdownView, VisualElement>
    {
        public VisualElement Render(W.MarkdownView element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var root = new ScrollView();
            root.AddToClassList("ud-markdown");

            if (!string.IsNullOrEmpty(element.Markdown))
            {
                var blocks = MdParser.Parse(element.Markdown);
                foreach (var block in blocks)
                {
                    var ve = RenderBlock(block, element.OnUrlClick);
                    if (ve != null)
                        root.Add(ve);
                }
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, root);
            return root;
        }

        // =====================================================================
        // Block rendering
        // =====================================================================

        private static VisualElement RenderBlock(MdBlock block, Action<string> onUrlClick)
        {
            switch (block.Type)
            {
                case MdBlockType.Heading:      return RenderHeading(block, onUrlClick);
                case MdBlockType.Paragraph:    return RenderParagraph(block, onUrlClick);
                case MdBlockType.CodeBlock:    return RenderCodeBlock(block);
                case MdBlockType.UnorderedList: return RenderList(block, onUrlClick, ordered: false);
                case MdBlockType.OrderedList:  return RenderList(block, onUrlClick, ordered: true);
                case MdBlockType.Blockquote:   return RenderBlockquote(block, onUrlClick);
                case MdBlockType.HorizontalRule: return RenderHorizontalRule();
                default: return null;
            }
        }

        private static VisualElement RenderHeading(MdBlock block, Action<string> onUrlClick)
        {
            var ve = RenderInlineContainer(block.Inlines, onUrlClick);
            ve.AddToClassList("ud-heading");
            ve.AddToClassList($"ud-h{Mathf.Clamp(block.Level, 1, 6)}");
            return ve;
        }

        private static VisualElement RenderParagraph(MdBlock block, Action<string> onUrlClick)
        {
            var ve = RenderInlineContainer(block.Inlines, onUrlClick);
            ve.AddToClassList("ud-md-paragraph");
            return ve;
        }

        private static VisualElement RenderCodeBlock(MdBlock block)
        {
            var container = new VisualElement();
            container.AddToClassList("ud-md-codeblock");
            container.style.flexDirection = FlexDirection.Column;

            if (!string.IsNullOrEmpty(block.Language))
            {
                var langLabel = new Label(block.Language);
                langLabel.AddToClassList("ud-md-codeblock-lang");
                container.Add(langLabel);
            }

            var codeLabel = new Label(block.RawText) { enableRichText = false };
            codeLabel.AddToClassList("ud-md-code");
            container.Add(codeLabel);

            return container;
        }

        private static VisualElement RenderList(MdBlock block, Action<string> onUrlClick, bool ordered)
        {
            var container = new VisualElement();
            container.AddToClassList(ordered ? "ud-md-ol" : "ud-md-ul");
            container.style.flexDirection = FlexDirection.Column;

            foreach (var item in block.Items)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.AddToClassList("ud-md-li");

                var bullet = new Label(ordered ? $"{item.OrderIndex}." : "•");
                bullet.AddToClassList("ud-md-li-bullet");
                row.Add(bullet);

                var content = RenderInlineContainer(item.Inlines, onUrlClick);
                content.AddToClassList("ud-md-li-content");
                row.Add(content);

                container.Add(row);
            }

            return container;
        }

        private static VisualElement RenderBlockquote(MdBlock block, Action<string> onUrlClick)
        {
            var container = new VisualElement();
            container.AddToClassList("ud-md-blockquote");
            container.style.flexDirection = FlexDirection.Column;

            foreach (var inner in block.Blocks)
            {
                var ve = RenderBlock(inner, onUrlClick);
                if (ve != null)
                    container.Add(ve);
            }

            return container;
        }

        private static VisualElement RenderHorizontalRule()
        {
            var hr = new VisualElement();
            hr.AddToClassList("ud-md-hr");
            return hr;
        }

        // =====================================================================
        // Inline rendering
        // =====================================================================

        /// <summary>
        /// Renders a list of inlines.  When there are no links/images the result is a single
        /// rich-text Label.  When links are present a flex-wrap container is used so that
        /// clickable segments can be individual VisualElements.
        /// </summary>
        private static VisualElement RenderInlineContainer(List<MdInline> inlines, Action<string> onUrlClick)
        {
            if (!HasClickable(inlines))
            {
                // Fast path: pure rich text label
                var sb = new StringBuilder();
                BuildRichText(inlines, sb);
                var lbl = new Label(sb.ToString()) { enableRichText = true };
                lbl.AddToClassList("ud-md-text");
                return lbl;
            }

            // Slow path: mixed content with clickable links / images
            var container = new VisualElement();
            container.AddToClassList("ud-md-text-row");
            container.style.flexDirection = FlexDirection.Row;
            container.style.flexWrap = Wrap.Wrap;

            RenderInlinesIntoContainer(inlines, container, onUrlClick);
            return container;
        }

        private static bool HasClickable(List<MdInline> inlines)
        {
            if (inlines == null) return false;
            foreach (var inline in inlines)
                if (inline.Type == MdInlineType.Link || inline.Type == MdInlineType.Image)
                    return true;
            return false;
        }

        // Builds a Unity rich-text string for the given inlines (no link interactivity).
        private static void BuildRichText(List<MdInline> inlines, StringBuilder sb)
        {
            if (inlines == null) return;

            foreach (var inline in inlines)
            {
                switch (inline.Type)
                {
                    case MdInlineType.Text:
                        sb.Append(EscapeRichText(inline.Text));
                        break;
                    case MdInlineType.Bold:
                        sb.Append("<b>").Append(EscapeRichText(inline.Text)).Append("</b>");
                        break;
                    case MdInlineType.Italic:
                        sb.Append("<i>").Append(EscapeRichText(inline.Text)).Append("</i>");
                        break;
                    case MdInlineType.BoldItalic:
                        sb.Append("<b><i>").Append(EscapeRichText(inline.Text)).Append("</i></b>");
                        break;
                    case MdInlineType.Code:
                        sb.Append("<color=#c0392b>").Append(inline.Text).Append("</color>");
                        break;
                    case MdInlineType.Link:
                        // Render as underlined blue text (not clickable in this path, but
                        // HasClickable should have returned true — kept for safety)
                        sb.Append("<color=#5b9bd5><u>").Append(EscapeRichText(inline.Text)).Append("</u></color>");
                        break;
                    case MdInlineType.Image:
                        sb.Append("[").Append(EscapeRichText(inline.Text)).Append("]");
                        break;
                    case MdInlineType.LineBreak:
                        sb.Append('\n');
                        break;
                }
            }
        }

        // Renders inlines into a flex-wrap container, making links clickable VisualElements.
        private static void RenderInlinesIntoContainer(
            List<MdInline> inlines, VisualElement container, Action<string> onUrlClick)
        {
            if (inlines == null) return;

            var textBuf = new StringBuilder();

            void FlushText()
            {
                if (textBuf.Length == 0) return;
                var lbl = new Label(textBuf.ToString()) { enableRichText = true };
                lbl.AddToClassList("ud-md-text");
                container.Add(lbl);
                textBuf.Clear();
            }

            foreach (var inline in inlines)
            {
                switch (inline.Type)
                {
                    case MdInlineType.Link:
                    {
                        FlushText();
                        var link = new Label(inline.Text ?? inline.Url);
                        link.AddToClassList("ud-md-link");
                        if (onUrlClick != null && !string.IsNullOrEmpty(inline.Url))
                        {
                            var url = inline.Url;
                            link.RegisterCallback<ClickEvent>(_ => onUrlClick(url));
                            link.AddToClassList("ud-md-link--clickable");
                        }
                        container.Add(link);
                        break;
                    }

                    case MdInlineType.Image:
                    {
                        FlushText();
                        var placeholder = new Label($"[{inline.Text}]");
                        placeholder.AddToClassList("ud-md-image-placeholder");
                        if (onUrlClick != null && !string.IsNullOrEmpty(inline.Url))
                        {
                            var url = inline.Url;
                            placeholder.RegisterCallback<ClickEvent>(_ => onUrlClick(url));
                            placeholder.AddToClassList("ud-md-link--clickable");
                        }
                        container.Add(placeholder);
                        break;
                    }

                    case MdInlineType.LineBreak:
                        FlushText();
                        // Insert an invisible full-width element to force a line wrap
                        var br = new VisualElement();
                        br.AddToClassList("ud-md-br");
                        container.Add(br);
                        break;

                    default:
                        // Accumulate as rich text segment
                        switch (inline.Type)
                        {
                            case MdInlineType.Text:
                                textBuf.Append(EscapeRichText(inline.Text));
                                break;
                            case MdInlineType.Bold:
                                textBuf.Append("<b>").Append(EscapeRichText(inline.Text)).Append("</b>");
                                break;
                            case MdInlineType.Italic:
                                textBuf.Append("<i>").Append(EscapeRichText(inline.Text)).Append("</i>");
                                break;
                            case MdInlineType.BoldItalic:
                                textBuf.Append("<b><i>").Append(EscapeRichText(inline.Text)).Append("</i></b>");
                                break;
                            case MdInlineType.Code:
                                textBuf.Append("<color=#c0392b>").Append(inline.Text).Append("</color>");
                                break;
                        }
                        break;
                }
            }

            FlushText();
        }

        // =====================================================================
        // Helpers
        // =====================================================================

        /// <summary>
        /// Escapes angle brackets so that arbitrary text does not accidentally activate
        /// Unity's rich-text tag parser inside a Label with enableRichText = true.
        /// Unity TextCore supports the &lt;noparse&gt; tag for exactly this purpose.
        /// </summary>
        private static string EscapeRichText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            // Use noparse to wrap the entire segment when angle brackets are present.
            if (text.IndexOf('<') >= 0 || text.IndexOf('>') >= 0)
                return "<noparse>" + text + "</noparse>";
            return text;
        }
    }
}
