using System;
using System.Collections.Generic;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    /// <summary>
    /// Represents a single entry in a TocView navigation sidebar.
    /// </summary>
    public class TocEntry
    {
        public string Text { get; set; }

        /// <summary>Heading level (1–6), used to determine indent depth.</summary>
        public int Level { get; set; } = 1;

        public bool IsSelected { get; set; }

        public Action OnClick { get; set; }

        public TocEntry(string text, int level = 1, Action onClick = null)
        {
            Text = text;
            Level = level;
            OnClick = onClick;
        }
    }

    /// <summary>
    /// A left-side navigation / table-of-contents control that renders a list of
    /// <see cref="TocEntry"/> items with indent levels matching H1–H6 headings.
    /// </summary>
    public class TocView : Element
    {
        public List<TocEntry> Items { get; set; } = new List<TocEntry>();

        /// <summary>
        /// TocView is handled entirely by UIToolkitTocViewRenderer and does not compose
        /// child elements declaratively; the renderer builds the visual tree directly.
        /// </summary>
        public override IElement Render() => null;

        public TocView() { }

        public TocView(IEnumerable<TocEntry> items)
        {
            Items = new List<TocEntry>(items);
        }
    }
}
