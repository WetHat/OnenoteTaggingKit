using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Collection of table cell proxies defining a table row.
    /// </summary>
    public class CellCollection : PageObjectCollectionBase<Row,Cell> {
        /// <summary>
        /// Initialize the proxy object collection of table cell elements
        /// contained under a table row element in a table on
        /// a OneNote page XML document.
        /// </summary>
        /// <param name="row">
        ///     The table row ent owning the table cells.
        /// </param>
        public CellCollection(Row row) :base(row.GetName(nameof(Cell)),row) {
        }
        /// <summary>
        /// Add a cell to the collection of cells in a table row.
        /// </summary>
        /// <param name="cell">Cell proxy to add to the collection.</param>
        protected override void Add(Cell cell) {
            base.Add(cell);
            Owner.Element.Add(cell.Element);
        }

        /// <summary>
        /// Add a cell to this row.
        /// </summary>
        /// <param name="cell">Table cell proxy to add.</param>
        public void AddCell(Cell cell) {
            Add(cell);
        }

        /// <summary>
        /// Create a new proxy object for a cell in a OneNote tyble.
        /// </summary>
        /// <param name="e">a `one:cell` XML element in a OneNote table cell.</param>
        /// <returns>New Cell proxy object.</returns>
        protected override Cell CreateElementProxy(XElement e) {
            return new Cell(e);
        }

        /// <summary>
        /// Get the number of cells in this colelction.
        /// </summary>
        public int CellCount { get => Items.Count; }

        /// <summary>
        /// Get a cell proxy from this collection.
        /// </summary>
        /// <param name="index">Cell index.</param>
        /// <returns></returns>
        public Cell this[int index] => Items[index];
    }
}
