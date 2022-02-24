using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy class for OneNote tables
    /// </summary>
    public class Table : PageObjectBase {

        static XElement DefineColumns(XNamespace ns, int columns) {
            XElement cols = new XElement(ns.GetName("Columns"));

            for (int i = 0; i < columns; i++) {
                cols.Add(new XElement(ns.GetName("Column")),
                            new XAttribute("index", i));
            }
            return cols;
        }

        /// <summary>
        /// Get the colelction of rows in this table.
        /// </summary>
        public RowCollection Rows { get;}

        /// <summary>
        /// Get or set the table border visibility.
        /// </summary>
        public bool BordersVisible {
            get {
                return "true".Equals(GetAttributeValue("bordersVisible"));
            }
            set {
                SetAttributeValue("bordersVisible", value.ToString().ToLower());
            }
        }

        /// <summary>
        /// Get or set the presence of a header row in the table.
        /// </summary>
        public bool HasHeaderRow {
            get {
                return "true".Equals(GetAttributeValue("hasHeaderRow"));
            }
            set {
                SetAttributeValue("hasHeaderRow", value.ToString().ToLower());
            }
        }


        /// <summary>
        /// Initialize proxy with a table element found on a OneNote page document.
        /// </summary>
        /// <param name="element">
        ///     A `one:Table` XML element
        ///     on a OneNote page document
        /// </param>
        public Table(XElement element) : base(element) {
            Rows = new RowCollection(this);
        }

        /// <summary>
        /// Initialize a table proxy with a new, empty OneNote table element.
        /// </summary>
        /// <param name="ns">The namespace the create the table in.</param>
        /// <param name="columns">Number of table columns.</param>
        public Table(XNamespace ns, int columns)
            : base(new XElement(ns.GetName(nameof(Table)),
                       DefineColumns(ns,columns))) {
            Rows = new RowCollection(this);
        }
    }
}
