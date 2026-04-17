using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.MD;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets;

namespace UniDecl.Editor.UIToolKit.Renderers
{
    /// <summary>
    /// UIToolkit renderer for <see cref="W.RichText"/>.
    ///
    /// Fast path (no links/images): a single Unity rich-text <see cref="Label"/>.
    /// Slow path (links present):  a flex-wrap row where each segment is an individual
    /// <see cref="Label"/>, and link/image labels are made clickable.
    ///
    /// URL clicks are forwarded to <see cref="W.RichText.OnUrlClick"/>.
    /// </summary>
    public class UIToolkitRichTextRenderer : IElementRenderer<W.RichText, VisualElement>
    {
        private const string InlineCodeColor = "#c0392b";
        private const string LinkColor = "#5b9bd5";

        public VisualElement Render(W.RichText element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var ve = RenderInlineContainer(element.Inlines, element.OnUrlClick);
            UIToolkitStyleApplier.ApplyElementStyles(element, ve);
            return ve;
        }

        // =====================================================================
        // Inline container
        // =====================================================================

        private VisualElement RenderInlineContainer(List<MdInline> inlines, Action<string> onUrlClick)
        {
            if (!HasClickable(inlines))
            {
                var sb = new StringBuilder();
                BuildRichText(inlines, sb);
                var lbl = new Label(sb.ToString()) { enableRichText = true };
                lbl.AddToClassList("ud-md-text");
                return lbl;
            }

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

        // =====================================================================
        // Fast path: pure rich-text string
        // =====================================================================

        private void BuildRichText(List<MdInline> inlines, StringBuilder sb)
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
                        sb.Append($"<color={InlineCodeColor}>").Append(inline.Text).Append("</color>");
                        break;
                    case MdInlineType.Link:
                        sb.Append($"<color={LinkColor}><u>").Append(EscapeRichText(inline.Text)).Append("</u></color>");
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

        // =====================================================================
        // Slow path: clickable segments in flex-wrap container
        // =====================================================================

        private void RenderInlinesIntoContainer(
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
                    {
                        FlushText();
                        var br = new VisualElement();
                        br.AddToClassList("ud-md-br");
                        container.Add(br);
                        break;
                    }

                    default:
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
                                textBuf.Append($"<color={InlineCodeColor}>").Append(inline.Text).Append("</color>");
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
        /// Wraps text containing angle brackets in a Unity <c>&lt;noparse&gt;</c> tag so that
        /// arbitrary user text does not accidentally trigger the rich-text tag parser.
        /// </summary>
        private static string EscapeRichText(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (text.IndexOf('<') >= 0 || text.IndexOf('>') >= 0)
                return "<noparse>" + text + "</noparse>";
            return text;
        }
    }
}
