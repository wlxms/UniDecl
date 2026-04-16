namespace UniDecl.Runtime.MD
{
    public enum MdInlineType
    {
        /// <summary>Plain text segment.</summary>
        Text,
        /// <summary>Bold text (**text** or __text__).</summary>
        Bold,
        /// <summary>Italic text (*text* or _text_).</summary>
        Italic,
        /// <summary>Bold and italic text (***text*** or ___text___).</summary>
        BoldItalic,
        /// <summary>Inline code (`code`).</summary>
        Code,
        /// <summary>Hyperlink ([text](url)).</summary>
        Link,
        /// <summary>Image (![alt](url)).</summary>
        Image,
        /// <summary>Hard line break.</summary>
        LineBreak,
    }

    /// <summary>An inline-level element produced by <see cref="MdParser.ParseInlines"/>.</summary>
    public class MdInline
    {
        public MdInlineType Type { get; set; }

        /// <summary>Display text (all types except <see cref="MdInlineType.LineBreak"/>).</summary>
        public string Text { get; set; }

        /// <summary>Target URL (only for <see cref="MdInlineType.Link"/> and <see cref="MdInlineType.Image"/>).</summary>
        public string Url { get; set; }

        // ---- factory helpers ----

        public static MdInline PlainText(string text) =>
            new MdInline { Type = MdInlineType.Text, Text = text };

        public static MdInline Bold(string text) =>
            new MdInline { Type = MdInlineType.Bold, Text = text };

        public static MdInline Italic(string text) =>
            new MdInline { Type = MdInlineType.Italic, Text = text };

        public static MdInline BoldItalic(string text) =>
            new MdInline { Type = MdInlineType.BoldItalic, Text = text };

        public static MdInline Code(string text) =>
            new MdInline { Type = MdInlineType.Code, Text = text };

        public static MdInline Link(string text, string url) =>
            new MdInline { Type = MdInlineType.Link, Text = text, Url = url };

        public static MdInline Image(string alt, string url) =>
            new MdInline { Type = MdInlineType.Image, Text = alt, Url = url };

        public static MdInline LineBreak() =>
            new MdInline { Type = MdInlineType.LineBreak };
    }
}
