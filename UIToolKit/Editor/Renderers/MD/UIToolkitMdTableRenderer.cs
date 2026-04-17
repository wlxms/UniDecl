using UnityEngine.UIElements;
using UniDecl.Runtime.Core;
using UniDecl.Editor.UIToolKit.Style;
using W = UniDecl.Runtime.Widgets.MD;

namespace UniDecl.Editor.UIToolKit.Renderers.MD
{
    /// <summary>
    /// UIToolkit renderer for <see cref="W.MdTable"/>.
    ///
    /// Produces a column-flex container (<c>ud-md-table</c>) with a header row
    /// (<c>ud-md-table-header</c>) followed by one row per data entry
    /// (<c>ud-md-table-row</c>).  Each cell uses the column's
    /// <see cref="W.MdTableColumn.CellRenderer"/> when provided, otherwise falls
    /// back to <see cref="W.MdTable.DefaultCellText"/>.
    /// </summary>
    public class UIToolkitMdTableRenderer : IElementRenderer<W.MdTable, VisualElement>
    {
        public VisualElement Render(W.MdTable element, IElementRenderHost<VisualElement> manager, ElementState state)
        {
            if (element == null) return null;

            var table = new VisualElement();
            table.AddToClassList("ud-md-table");
            table.style.flexDirection = FlexDirection.Column;

            // Header row
            if (element.Columns != null && element.Columns.Count > 0)
            {
                var headerRow = new VisualElement();
                headerRow.AddToClassList("ud-md-table-header");
                headerRow.style.flexDirection = FlexDirection.Row;

                for (int col = 0; col < element.Columns.Count; col++)
                {
                    var headerCell = new Label(element.Columns[col].Header ?? string.Empty);
                    headerCell.AddToClassList("ud-md-table-header-cell");
                    headerRow.Add(headerCell);
                }

                table.Add(headerRow);
            }

            // Data rows
            for (int row = 0; row < element.RowCount; row++)
            {
                var dataRow = new VisualElement();
                dataRow.AddToClassList("ud-md-table-row");
                dataRow.style.flexDirection = FlexDirection.Row;

                if (element.Columns != null)
                {
                    for (int col = 0; col < element.Columns.Count; col++)
                    {
                        var column = element.Columns[col];
                        VisualElement cell;

                        if (column.CellRenderer != null)
                        {
                            var cellElement = column.CellRenderer(row);
                            cell = cellElement != null ? manager.RenderElement(cellElement) : new VisualElement();
                        }
                        else
                        {
                            var text = element.DefaultCellText != null
                                ? element.DefaultCellText(row, col)
                                : string.Empty;
                            cell = new Label(text ?? string.Empty);
                        }

                        if (cell != null)
                        {
                            cell.AddToClassList("ud-md-table-cell");
                            dataRow.Add(cell);
                        }
                    }
                }

                table.Add(dataRow);
            }

            UIToolkitStyleApplier.ApplyElementStyles(element, table);
            return table;
        }
    }
}
