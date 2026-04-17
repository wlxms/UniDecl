using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Runtime.MD;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets.MD;

namespace UniDecl.Editor.UIToolKit.Renderers.MD
{
    /// <summary>
    /// UIToolkit renderer for <see cref="W.RichText"/>.
    ///
    /// Renders all inline segments as a flex-wrap row where each segment is an individual
    /// <see cref="Label"/>, and link/image labels are made clickable.
    ///
    /// URL clicks are forwarded to <see cref="W.RichText.OnUrlClick"/>.
    /// </summary>
    public class UIToolkitRichTextRenderer : IElementRenderer<W.RichText, VisualElement>
    {
        private const string InlineCodeColor = "#c0392b";

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
            // Always use slow path to ensure links/images are clickable
            var container = new VisualElement();
            container.AddToClassList("ud-md-text-row");
            container.style.flexDirection = FlexDirection.Row;
            container.style.flexWrap = Wrap.Wrap;
            RenderInlinesIntoContainer(inlines, container, onUrlClick);
            return container;
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
                lbl.selection.isSelectable = true;
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
                        link.selection.isSelectable = true;
                        if (!string.IsNullOrEmpty(inline.Url))
                        {
                            var url = inline.Url;
                            link.RegisterCallback<ClickEvent>(_ =>
                            {
                                if (onUrlClick != null) onUrlClick(url);
                                else Application.OpenURL(url);
                            });
                            link.AddToClassList("ud-md-link--clickable");
                        }
                        container.Add(link);
                        break;
                    }

                    case MdInlineType.Image:
                    {
                        FlushText();
                        var imgContainer = new VisualElement();
                        imgContainer.AddToClassList("ud-md-image-container");

                        // Prefer a guaranteed type icon, then fallback to another builtin icon key.
                        var iconTex = EditorGUIUtility.ObjectContent(null, typeof(Texture2D))?.image as Texture2D
                                      ?? EditorGUIUtility.FindTexture("d_FilterByType");
                        if (iconTex != null)
                        {
                            var imgIcon = new Image { image = iconTex };
                            imgIcon.AddToClassList("ud-md-image-icon");
                            imgContainer.Add(imgIcon);
                        }

                        var imgLabel = new Label(inline.Text ?? "Image");
                        imgLabel.AddToClassList("ud-md-image-alt");

                        imgContainer.Add(imgLabel);

                        if (!string.IsNullOrEmpty(inline.Url))
                        {
                            var url = inline.Url;
                            imgContainer.RegisterCallback<ClickEvent>(_ =>
                            {
                                if (onUrlClick != null) onUrlClick(url);
                                else Application.OpenURL(url);
                            });
                            imgContainer.AddToClassList("ud-md-link--clickable");
                        }

                        container.Add(imgContainer);
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
