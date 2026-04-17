using UniDecl.Runtime.Core;
using UniDecl.Runtime.Components;

namespace UniDecl.Runtime.Widgets.MD
{
    public class H4 : Element
    {
        public string Text { get; set; }
        public bool EnableRichText { get; set; } = true;

        public H4(string text) { Text = text; }

        public override IElement Render() =>
            new Label(Text) { EnableRichText = EnableRichText }
                .With(new InlineStyle("ud-heading", "ud-h4"));
    }
}
