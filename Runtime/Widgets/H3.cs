using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class H3 : Element
    {
        public string Text { get; set; }
        public bool EnableRichText { get; set; } = true;

        public override IElement Render() => null;

        public H3(string text) { Text = text; }
    }
}
