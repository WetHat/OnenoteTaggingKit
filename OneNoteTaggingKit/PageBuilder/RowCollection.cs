using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// A Collection of table row proxy elements.
    /// </summary>
    public class RowCollection : PageObjectCollectionBase<Table,Row>
    {

        /// <summary>
        /// Create a collection of 'one:Row' proxy objects for the rows in
        /// a OneNote
        /// </summary>
        /// <param name="table">The proxy object or a OneNote table owning the rows.</param>
        public RowCollection(Table table) : base(table.GetName("Row"),table) {

        }

        /// <summary>
        /// Add a row to the table owning this collection.
        /// </summary>
        /// <param name="row">The proxy object of a OneNote table row.</param>
        protected override void Add(Row row) {
            base.Add(row);
            Owner.Element.Add(row.Element);
        }

        /// <summary>
        /// Create a new table row proxy object.
        /// </summary>
        /// <param name="e">The 'one:Row' XML element in a OneNote table.</param>
        /// <returns></returns>
        protected override Row CreateElementProxy(XElement e) {
            return new Row(e);
        }
    }
}
