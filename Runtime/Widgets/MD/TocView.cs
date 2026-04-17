using System;
using System.Collections.Generic;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets.MD
{
    /// <summary>
    /// Represents a single entry in a TocView navigation sidebar.
    /// Supports tree structure via <see cref="Children"/>, collapsible state,
    /// and selection tracking via <see cref="Id"/>.
    /// </summary>
    public class TocEntry
    {
        /// <summary>Unique identifier for selection tracking. Auto-generated if not set.</summary>
        public string Id { get; set; }

        /// <summary>Display text.</summary>
        public string Text { get; set; }

        /// <summary>Heading level (1–6), used to determine indent depth.</summary>
        public int Level { get; set; } = 1;

        /// <summary>Whether this entry is currently selected.</summary>
        public bool IsSelected { get; set; }

        /// <summary>Callback invoked when this entry is clicked.</summary>
        public Action<TocEntry> OnClick { get; set; }

        /// <summary>Child entries. Empty means this is a leaf node.</summary>
        public List<TocEntry> Children { get; set; } = new List<TocEntry>();

        /// <summary>Whether children are currently visible. Only meaningful when <see cref="HasChildren"/> is true.</summary>
        public bool IsExpanded { get; set; } = true;

        /// <summary>Whether this entry has any child entries.</summary>
        public bool HasChildren => Children != null && Children.Count > 0;

        /// <summary>
        /// Backward-compatible constructor (flat list usage).
        /// <see cref="Id"/> is auto-generated from <paramref name="text"/>.
        /// </summary>
        public TocEntry(string text, int level = 1, Action onClick = null)
        {
            Id = Guid.NewGuid().ToString("N");
            Text = text;
            Level = level;
            OnClick = onClick != null ? _ => onClick() : null;
        }

        /// <summary>
        /// Tree constructor with explicit <paramref name="id"/> for selection tracking.
        /// </summary>
        public TocEntry(string id, string text, int level = 1)
        {
            Id = id;
            Text = text;
            Level = level;
        }

        /// <summary>Convenience method to add a child entry (fluent).</summary>
        public TocEntry AddChild(TocEntry child)
        {
            Children.Add(child);
            return this;
        }

        /// <summary>Convenience method to add a child entry (fluent, shorthand).</summary>
        public TocEntry AddChild(string id, string text, int level = 1)
        {
            Children.Add(new TocEntry(id, text, level));
            return this;
        }
    }

    /// <summary>
    /// A left-side navigation / table-of-contents control that renders a tree of
    /// <see cref="TocEntry"/> items with indent levels, collapsible groups,
    /// hover highlight, and click selection.
    ///
    /// Rendering is handled entirely by UIToolkitTocViewRenderer.
    /// </summary>
    public class TocView : Element
    {
        /// <summary>Top-level entries. Each may have <see cref="TocEntry.Children"/>.</summary>
        public List<TocEntry> Items { get; set; } = new List<TocEntry>();

        /// <summary>The <see cref="TocEntry.Id"/> of the currently selected entry.</summary>
        public string SelectedId { get; set; }

        /// <summary>
        /// Callback invoked when the selected entry changes.
        /// Receives the <see cref="TocEntry.Id"/> of the newly selected entry (null if deselected).
        /// </summary>
        public Action<string> OnSelectionChanged { get; set; }

        /// <summary>
        /// TocView is handled entirely by UIToolkitTocViewRenderer.
        /// </summary>
        public override IElement Render() => null;

        public TocView() { }

        /// <summary>Construct with a flat list of entries.</summary>
        public TocView(IEnumerable<TocEntry> items)
        {
            Items = new List<TocEntry>(items);
        }

        /// <summary>
        /// Programmatically select an entry by <paramref name="id"/>.
        /// Triggers <see cref="OnSelectionChanged"/>.
        /// </summary>
        public void Select(string id)
        {
            SelectedId = id;
            OnSelectionChanged?.Invoke(id);
        }

        /// <summary>
        /// Expand all entries in the tree.
        /// </summary>
        public void ExpandAll() => SetExpandedRecursive(Items, true);

        /// <summary>
        /// Collapse all entries in the tree.
        /// </summary>
        public void CollapseAll() => SetExpandedRecursive(Items, false);

        private static void SetExpandedRecursive(List<TocEntry> entries, bool expanded)
        {
            if (entries == null) return;
            foreach (var e in entries)
            {
                e.IsExpanded = expanded;
                SetExpandedRecursive(e.Children, expanded);
            }
        }
    }
}
