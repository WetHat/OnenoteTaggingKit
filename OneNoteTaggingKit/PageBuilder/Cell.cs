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
    }
}
