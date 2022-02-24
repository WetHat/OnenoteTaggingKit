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


        /// <summary>
        /// Initialize a cell proxy instance from an XML element
        /// </summary>
        /// <param name="element"></param>
        public Cell (XElement element) : base(element) {
            _OEChildren = element.Element(element.Name.Namespace.GetName("OEChildren"));
        }
        public void Add(OE content) {

        }
    }

}
