using System;
using System.Collections.Generic;
using UniDecl.Runtime.Core;

namespace UniDecl.Runtime.Widgets
{
    /// <summary>
    /// Column descriptor for <see cref="MdTable"/>.
    /// </summary>
    public class MdTableColumn
    {
        /// <summary>Column header text.</summary>
        public string Header { get; set; }

        /// <summary>
        /// Optional per-cell element factory.  Receives the zero-based row index and must
        /// return the <see cref="IElement"/> to display in that cell.  When <c>null</c> the
        /// table renderer falls back to <see cref="MdTable.DefaultCellText"/>.
        /// </summary>
        public Func<int, IElement> CellRenderer { get; set; }

        public MdTableColumn() { }

        public MdTableColumn(string header, Func<int, IElement> cellRenderer = null)
        {
            Header = header;
            CellRenderer = cellRenderer;
        }
    }

    /// <summary>
    /// A data table widget with optional per-column custom cell rendering.
    ///
    /// Rendered by <c>UIToolkitMdTableRenderer</c>.
    /// </summary>
    public class MdTable : Element
    {
        /// <summary>Column definitions (order determines display order).</summary>
        public List<MdTableColumn> Columns { get; set; } = new List<MdTableColumn>();

        /// <summary>Number of data rows (not counting the header row).</summary>
        public int RowCount { get; set; }

        /// <summary>
        /// Fallback cell-text provider used when a column has no
        /// <see cref="MdTableColumn.CellRenderer"/>.
        /// Receives <c>(rowIndex, columnIndex)</c> and returns the display string.
        /// </summary>
        public Func<int, int, string> DefaultCellText { get; set; }

        /// <summary>Rendered entirely by <c>UIToolkitMdTableRenderer</c>.</summary>
        public override IElement Render() => null;
    }
}
