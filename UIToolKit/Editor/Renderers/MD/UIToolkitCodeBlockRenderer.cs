using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets.MD;

namespace UniDecl.Editor.UIToolKit.Renderers.MD
{
    /// <summary>
    /// UIToolkit renderer for <see cref="W.CodeBlock"/>.
    /// Produces a dark-background code container with optional language label,
    /// a copy-to-clipboard icon (right-aligned inside the label bar),
    /// and syntax-highlighted code text.
    /// </summary>
    public class UIToolkitCodeBlockRenderer : IElementRenderer<W.CodeBlock, VisualElement>
    {
        private const string DefaultCodeColor = "#d4d4d4";

        public VisualElement Render(W.CodeBlock element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.AddToClassList("ud-md-codeblock");
            container.style.flexDirection = FlexDirection.Column;

            // ── Label bar: language text (left) + copy icon (right) ────
            bool hasLabelBar = !string.IsNullOrEmpty(element.Language) || element.ShowCopyButton;
            if (hasLabelBar)
            {
                var labelBar = new VisualElement();
                labelBar.AddToClassList("ud-md-codeblock-lang");
                labelBar.style.flexDirection = FlexDirection.Row;
                labelBar.style.justifyContent = Justify.SpaceBetween;
                labelBar.style.alignItems = Align.Center;

                if (!string.IsNullOrEmpty(element.Language))
                {
                    var langText = new Label(element.Language);
                    langText.style.unityFontStyleAndWeight = FontStyle.Bold;
                    labelBar.Add(langText);
                }
                else
                {
                    var spacer = new VisualElement();
                    spacer.style.flexGrow = 1;
                    labelBar.Add(spacer);
                }

                if (element.ShowCopyButton)
                {
                    var copyBtn = CreateCopyIconButton(element.Code ?? "", element.OnCopy);
                    labelBar.Add(copyBtn);
                }

                container.Add(labelBar);
            }

            // ── Code content ────────────────────────────────────────────
            string displayText;
            bool richText;

            if (element.EnableHighlighting && !string.IsNullOrEmpty(element.Language))
            {
                displayText = W.CodeHighlighter.Highlight(element.Code ?? "", element.Language);
                richText = true;
            }
            else
            {
                displayText = element.Code ?? "";
                richText = false;
            }

            var codeLabel = new Label(displayText)
            {
                enableRichText = richText,
                parseEscapeSequences = false,
            };
            codeLabel.AddToClassList("ud-md-code");
            codeLabel.selection.isSelectable = true;

            if (!richText)
                codeLabel.style.color = ColorUtility.TryParseHtmlString(DefaultCodeColor, out var dc) ? dc : Color.white;

            container.Add(codeLabel);

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }

        // =================================================================
        // Copy icon — small transparent Image button using built-in icons
        // =================================================================

        private static VisualElement CreateCopyIconButton(string code, System.Action<string> onCopy)
        {
            var icon = EditorGUIUtility.IconContent("TreeEditor.Duplicate")?.image
                       ?? EditorGUIUtility.FindTexture("TreeEditor.Duplicate");

            var btn = new Button();
            btn.AddToClassList("ud-md-codeblock-copy-btn");
            btn.style.width = 20;
            btn.style.height = 20;
            btn.style.alignItems = Align.Center;
            btn.style.justifyContent = Justify.Center;
            btn.Clear();

            if (icon != null)
            {
                var img = new Image { image = icon, scaleMode = ScaleMode.ScaleToFit };
                img.style.width = 20;
                img.style.height = 20;
                btn.Add(img);
            }
            else
            {
                btn.text = "\u2398";
                btn.style.fontSize = 18;
                btn.style.unityTextAlign = TextAnchor.MiddleCenter;
            }

            btn.tooltip = "Copy to clipboard";
            Texture2D capturedIcon = icon as Texture2D;

            btn.clicked += () =>
            {
                if (onCopy != null) onCopy(code);
                else GUIUtility.systemCopyBuffer = code;

                // Briefly show green checkmark
                btn.Clear();
                var checkIcon = EditorGUIUtility.FindTexture("d_Valid");
                if (checkIcon != null)
                {
                    var ci = new Image { image = checkIcon as Texture2D, scaleMode = ScaleMode.ScaleToFit };
                    ci.style.width = 20; ci.style.height = 20;
                    ci.style.unityBackgroundImageTintColor = new StyleColor(new Color(0.4f, 0.8f, 0.4f));
                    btn.Add(ci);
                }
                else
                {
                    btn.text = "\u2713";
                    btn.style.color = new Color(0.4f, 0.8f, 0.4f);
                }

                btn.schedule.Execute(() =>
                {
                    btn.Clear();
                    if (capturedIcon != null)
                    {
                        var img2 = new Image { image = capturedIcon, scaleMode = ScaleMode.ScaleToFit };
                        img2.style.width = 20; img2.style.height = 20;
                        btn.Add(img2);
                    }
                    else
                    {
                        btn.text = "\u2398";
                    }
                }).ExecuteLater(1500);
            };

            return btn;
        }
    }
}
