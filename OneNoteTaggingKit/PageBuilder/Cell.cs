using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy class for table cells
    /// </summary>
    public class Cell : PageObjectBase
    {
        XElement _OEChildren;

        IEnumerable<OE> CellContent {
            get {
                foreach (var oe in _OEChildren.Elements()) {
                    XElement e = oe.Element(GetName("T"));
                    if (e != null) {
                        yield return new OET(oe);
                    } else {
                        e = oe.Element(GetName("Table"));
                        if (e != null) {
                            yield return new OETable(e);
                        }
                    }

                    yield return new OE(e); // generic element
                }
            }
            set {
                _OEChildren.RemoveAll();
                foreach (var oe in value) {
                    _OEChildren.Add(oe.Element);
                }
            }
        }

        /// <summary>
        /// Initialize a cell proxy instance from an XML element
        /// </summary>
        /// <param name="element"></param>
        public Cell (XElement element) : base(element) {
            _OEChildren = element.Element(element.Name.Namespace.GetName("OEChildren"));
        }

        /// <summary>
        /// Initialize a proxy element with a new table cell element.
        /// </summary>
        /// <param name="ns">The XML namespace to create the cell in.</param>
        /// <param name="content">A content object.</param>
        public Cell(XNamespace ns,OE content) : base(new XElement(ns.GetName(nameof(Cell)))) {
            Element.Add(_OEChildren = new XElement(ns.GetName("OEChildren"),content.Element));
        }
        /// <summary>
        /// Initialize a proxy element with a new table cell element.
        /// </summary>
        /// <param name="ns">The XML namespace to create the cell in.</param>
        /// <param name="content">A list of content object.</param>
        public Cell(XNamespace ns, IEnumerable<OE> content) : base(new XElement(ns.GetName(nameof(Cell)))) {
            Element.Add(_OEChildren = new XElement(ns.GetName("OEChildren"), from c in content select c.Element));
        }
    }
}
