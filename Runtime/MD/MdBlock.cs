using System.Collections.Generic;

namespace UniDecl.Runtime.MD
{
    public enum MdBlockType
    {
        /// <summary>ATX or setext heading (level 1–6).</summary>
        Heading,
        /// <summary>Regular paragraph.</summary>
        Paragraph,
        /// <summary>Fenced code block (``` or ~~~).</summary>
        CodeBlock,
        /// <summary>Unordered list (-, *, +).</summary>
        UnorderedList,
        /// <summary>Ordered list (1., 2., …).</summary>
        OrderedList,
        /// <summary>Blockquote (&gt;).</summary>
        Blockquote,
        /// <summary>Horizontal rule (---, ***, ___).</summary>
        HorizontalRule,
    }

    /// <summary>A single item inside an ordered or unordered list.</summary>
    public class MdListItem
    {
        public List<MdInline> Inlines { get; set; } = new List<MdInline>();
        public bool IsOrdered { get; set; }
        public int OrderIndex { get; set; }
    }

    /// <summary>A block-level element produced by <see cref="MdParser"/>.</summary>
    public class MdBlock
    {
        /// <summary>Kind of block.</summary>
        public MdBlockType Type { get; set; }

        /// <summary>Heading level 1–6 (only valid for <see cref="MdBlockType.Heading"/>).</summary>
        public int Level { get; set; }

        /// <summary>Raw text content (paragraphs, code blocks).</summary>
        public string RawText { get; set; }

        /// <summary>Language hint (only valid for <see cref="MdBlockType.CodeBlock"/>).</summary>
        public string Language { get; set; }

        /// <summary>Parsed inline elements (headings, paragraphs, list items).</summary>
        public List<MdInline> Inlines { get; set; } = new List<MdInline>();

        /// <summary>List items (only valid for list blocks).</summary>
        public List<MdListItem> Items { get; set; } = new List<MdListItem>();

        /// <summary>Inner blocks (only valid for <see cref="MdBlockType.Blockquote"/>).</summary>
        public List<MdBlock> Blocks { get; set; } = new List<MdBlock>();
    }
}
