using UniDecl.Runtime.Components;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    /// <summary>
    /// A fenced code block widget.  Composes via <see cref="Render"/> using
    /// <see cref="VerticalLayout"/> and <see cref="Label"/>, so no dedicated
    /// renderer is required.
    /// </summary>
    public class CodeBlock : Element
    {
        /// <summary>Optional language hint (e.g. <c>"csharp"</c>, <c>"json"</c>).</summary>
        public string Language { get; set; }

        /// <summary>Raw code text (pre-formatted, no Markdown inline parsing).</summary>
        public string Code { get; set; }

        public CodeBlock() { }

        public CodeBlock(string language, string code)
        {
            Language = language;
            Code = code;
        }

        public override IElement Render()
        {
            var layout = new VerticalLayout();
            layout.With(new InlineStyle("ud-md-codeblock"));

            if (!string.IsNullOrEmpty(Language))
                layout.Add(new Label(Language).With(new InlineStyle("ud-md-codeblock-lang")));

            layout.Add(new Label(Code) { EnableRichText = false }
                .With(new InlineStyle("ud-md-code")));

            return layout;
        }
    }
}
