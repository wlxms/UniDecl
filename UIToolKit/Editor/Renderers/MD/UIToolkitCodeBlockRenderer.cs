using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.BuiltIn.Runtime.Widgets.MD;

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
        const string CopyBtnClass = "ud-md-codeblock-copy-btn";
        const string CopyBtnOkClass = "ud-md-codeblock-copy-btn--ok";
        const string IconClass = "ud-md-codeblock-icon";
        const string IconOkClass = "ud-md-codeblock-icon--ok";

        public VisualElement Render(W.CodeBlock element, VisualElement existing, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            // 外观（底色 / 描边 / 圆角 / 内边距 / 字体）全部由 .ud-md-codeblock* 承担，
            // 此处只负责结构：容器 → 标签条 → 代码文本。
            var container = new VisualElement();
            container.AddToClassList("ud-md-codeblock");

            // ── Label bar: language text (left) + copy icon (right) ────
            bool hasLabelBar = !string.IsNullOrEmpty(element.Language) || element.ShowCopyButton;
            if (hasLabelBar)
            {
                var labelBar = new VisualElement();
                labelBar.AddToClassList("ud-md-codeblock-lang");

                if (!string.IsNullOrEmpty(element.Language))
                {
                    var langText = new Label(element.Language);
                    langText.AddToClassList("ud-md-codeblock-lang-text");
                    labelBar.Add(langText);
                }
                else
                {
                    var spacer = new VisualElement();
                    spacer.AddToClassList("ud-md-codeblock-lang-spacer");
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
            codeLabel.AddToClassList("ud-md-code");   // 前景色取自 --ud-text-code
            codeLabel.selection.isSelectable = true;

            container.Add(codeLabel);

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }

        // =================================================================
        // Copy icon — small transparent Image button using built-in icons
        // =================================================================

        static VisualElement CreateCopyIconButton(string code, System.Action<string> onCopy)
        {
            var icon = EditorGUIUtility.IconContent("TreeEditor.Duplicate")?.image
                       ?? EditorGUIUtility.FindTexture("TreeEditor.Duplicate");

            var btn = new Button();
            btn.AddToClassList(CopyBtnClass);
            btn.Clear();
            SetCopyIcon(btn, icon as Texture2D);

            btn.tooltip = "Copy to clipboard";
            Texture2D capturedIcon = icon as Texture2D;

            btn.clicked += () =>
            {
                if (onCopy != null) onCopy(code);
                else GUIUtility.systemCopyBuffer = code;

                // Briefly show green checkmark
                btn.AddToClassList(CopyBtnOkClass);
                SetCheckIcon(btn);

                btn.schedule.Execute(() =>
                {
                    btn.RemoveFromClassList(CopyBtnOkClass);
                    SetCopyIcon(btn, capturedIcon);
                }).ExecuteLater(1500);
            };

            return btn;
        }

        static void SetCopyIcon(Button btn, Texture2D icon)
        {
            btn.Clear();
            btn.text = string.Empty;
            if (icon != null)
            {
                var img = new Image { image = icon, scaleMode = ScaleMode.ScaleToFit };
                img.AddToClassList(IconClass);
                btn.Add(img);
            }
            else
            {
                btn.text = "\u2398";
            }
        }

        static void SetCheckIcon(Button btn)
        {
            btn.Clear();
            btn.text = string.Empty;
            var checkIcon = EditorGUIUtility.FindTexture("d_Valid") as Texture2D;
            if (checkIcon != null)
            {
                var ci = new Image { image = checkIcon, scaleMode = ScaleMode.ScaleToFit };
                ci.AddToClassList(IconClass);
                ci.AddToClassList(IconOkClass);
                btn.Add(ci);
            }
            else
            {
                btn.text = "\u2713";
            }
        }
    }
}
