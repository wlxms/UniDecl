using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class Label : Element
    {
        public string Text { get; set; }
        public bool EnableRichText { get; set; } = true;
        public bool ParseEscapeSequences { get; set; } = true;

        public override IElement Render() => null;

        public Label(string text) { Text = text; }
    }
}