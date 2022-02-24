using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy class for a OneNote elements with and embedded table.
    /// </summary>
    public class OETable : OE
    {
        /// <summary>
        /// Get the proxy object for the embedded OneNote table element..
        /// </summary>
        public Table Table { get; }

        /// <summary>
        /// Initialize a proxy element with a OneNote table found on a OneNote
        /// page document.
        /// </summary>
        /// <param name="element">
        ///     A `one:OE` element on a OneNote page document which has an
        ///     embedded table.
        /// </param>
        public OETable(XElement element) : base(element) {
            XElement te = element.Element(element.Name.Namespace.GetName("Table"));
            if (te != null) {
                Table = new Table(te);
            }
        }

        /// <summary>
        /// Initialie an element proxy object with a new embedded OneNote table.
        /// </summary>
        /// <param name="table">The table to embedd.</param>
        public OETable(Table table) : base(table.Namespace,table.Element) {
        }
    }
}
