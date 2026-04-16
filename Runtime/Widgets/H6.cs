using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class H6 : Element
    {
        public string Text { get; set; }
        public bool EnableRichText { get; set; } = true;

        public H6(string text) { Text = text; }

        public override IElement Render() =>
            new Label(Text) { EnableRichText = EnableRichText }
                .With(new StyleClasses("ud-heading", "ud-h6"));
    }
}
