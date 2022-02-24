using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy class for OneNote table rows.
    /// </summary>
    public class Row : PageObjectBase
    {
        /// <summary>
        /// Get the collection of cells in this row.
        /// </summary>
        public CellCollection Cells { get; }
        /// <summary>
        /// Initialize a table row proxy instance from an existing `one:Row`element
        /// in a OneNote table.
        /// </summary>
        /// <param name="element">OneNote table row XML element.</param>
        public Row(XElement element) : base(element) {
            Cells = new CellCollection(this);
        }

        /// <summary>
        /// Initialize the proxy with a new table row element
        /// </summary>
        /// <param name="ns">The XML namespace to create the row in</param>
        /// <param name="cells">Cells to add.</param>
        public Row(XNamespace ns, params Cell[] cells) : base(new XElement(ns.GetName(nameof(Row)))) {
            Cells = new CellCollection(this);
            foreach (var c in cells) {
                Cells.AddCell(c);
            }
        }
    }
}
