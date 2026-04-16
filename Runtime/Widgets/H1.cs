using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    public class H1 : Element
    {
        public string Text { get; set; }
        public bool EnableRichText { get; set; } = true;

        public H1(string text) { Text = text; }

        /// <summary>
        /// Penetrates to a <see cref="Label"/> so the existing Label renderer handles
        /// the visual output. The <see cref="StyleClasses"/> component carries the
        /// heading-specific CSS classes that theme stylesheets target.
        /// </summary>
        public override IElement Render() =>
            new Label(Text) { EnableRichText = EnableRichText }
                .With(new StyleClasses("ud-heading", "ud-h1"));
    }
}
