using UniDecl.BuiltIn.Runtime.Core;
using UniDecl.BuiltIn.Runtime.Components;

namespace UniDecl.BuiltIn.Runtime.Widgets.MD
{
    public class H2 : Element
    {
        public string Text { get; set; }
        public bool EnableRichText { get; set; } = true;

        public H2(string text) { Text = text; }

        public override IElement Render() =>
            new Label(Text) { EnableRichText = EnableRichText }
                .With(new InlineStyle("ud-heading", "ud-h2"));
    }
}
