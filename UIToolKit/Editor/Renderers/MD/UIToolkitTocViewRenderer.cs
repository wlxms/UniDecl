using UnityEngine;
using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets.MD;

namespace UniDecl.Editor.UIToolKit.Renderers.MD
{
    public class UIToolkitTocViewRenderer : IElementRenderer<W.TocView, VisualElement>
    {
        // Unicode chevrons for expand/collapse indicator
        private const string ChevronRight = "\u25B6";   // ▶
        private const string ChevronDown  = "\u25BC";   // ▼

        public VisualElement Render(W.TocView element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var container = new VisualElement();
            container.AddToClassList("ud-toc-view");

            RenderEntries(container, element.Items, element, 0);

            UIToolkitStyleApplier.ApplyElementStyles(element, container);
            return container;
        }

        // =================================================================
        // Recursive tree rendering
        // =================================================================

        private void RenderEntries(VisualElement parent, System.Collections.Generic.List<W.TocEntry> entries, W.TocView toc, int depth)
        {
            if (entries == null) return;

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null) continue;

                var row = CreateEntryRow(entry, toc, depth);
                parent.Add(row);

                // Render children if expanded
                if (entry.HasChildren && entry.IsExpanded)
                {
                    var childrenContainer = new VisualElement();
                    childrenContainer.AddToClassList("ud-toc-children");

                    RenderEntries(childrenContainer, entry.Children, toc, depth + 1);
                    parent.Add(childrenContainer);
                }
            }
        }

        private VisualElement CreateEntryRow(W.TocEntry entry, W.TocView toc, int depth)
        {
            var row = new VisualElement();
            row.AddToClassList("ud-toc-item");

            var level = Mathf.Clamp(entry.Level, 1, 6);
            row.AddToClassList($"ud-toc-item--l{level}");

            if (entry.HasChildren)
                row.AddToClassList("ud-toc-item--has-children");

            if (entry.IsSelected || entry.Id == toc.SelectedId)
                row.AddToClassList("ud-toc-item--selected");

            // Indent: combine heading level indent + tree depth indent
            var indentPx = (level - 1) * 12f + depth * 16f;
            row.style.paddingLeft = indentPx;

            // Chevron indicator (only for entries with children)
            if (entry.HasChildren)
            {
                var chevron = new Label(entry.IsExpanded ? ChevronDown : ChevronRight);
                chevron.AddToClassList("ud-toc-chevron");
                chevron.RegisterCallback<ClickEvent>(_ =>
                {
                    entry.IsExpanded = !entry.IsExpanded;
                    // Trigger re-render by rebuilding the tree
                    toc.Rebuild();
                });
                row.Add(chevron);
            }

            // Text label
            var label = new Label(entry.Text);
            label.AddToClassList("ud-toc-item-label");
            row.Add(label);

            // Click to select
            row.RegisterCallback<ClickEvent>(_ =>
            {
                // Toggle selection
                string newSelectedId = (entry.Id == toc.SelectedId) ? null : entry.Id;
                toc.SelectedId = newSelectedId;

                // Fire callbacks
                entry.OnClick?.Invoke(entry);
                toc.OnSelectionChanged?.Invoke(newSelectedId);

                // Re-render to update visual state
                toc.Rebuild();
            });

            // Hover highlight (CSS :hover handles visual; we also add the clickable cursor)
            row.AddToClassList("ud-toc-item--clickable");

            return row;
        }
    }
}
