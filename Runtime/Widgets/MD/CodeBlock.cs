using System;
using UniDecl.Runtime.Components;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets.MD
{
    /// <summary>
    /// A fenced code block widget with optional syntax highlighting and a copy button.
    /// Rendered by <c>UIToolkitCodeBlockRenderer</c>.
    /// </summary>
    public class CodeBlock : Element
    {
        /// <summary>Optional language hint (e.g. <c>"csharp"</c>, <c>"json"</c>).</summary>
        public string Language { get; set; }

        /// <summary>Raw code text (pre-formatted, no Markdown inline parsing).</summary>
        public string Code { get; set; }

        /// <summary>Whether to apply syntax highlighting. Default is <c>true</c>.</summary>
        public bool EnableHighlighting { get; set; } = true;

        /// <summary>Whether to show a copy-to-clipboard button. Default is <c>true</c>.</summary>
        public bool ShowCopyButton { get; set; } = true;

        /// <summary>
        /// Optional callback invoked when the copy button is clicked with the code text as argument.
        /// When null the renderer falls back to <c>GUIUtility.systemCopyBuffer</c>.
        /// </summary>
        public Action<string> OnCopy { get; set; }

        public CodeBlock() { }

        public CodeBlock(string language, string code)
        {
            Language = language;
            Code = code;
        }

        /// <summary>Intentionally returns <c>null</c> — rendering is handled by the dedicated UIToolkit renderer.</summary>
        public override IElement Render() => null;
    }
}
