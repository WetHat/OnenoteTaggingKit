using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    ///     Proxy object for a indented group of OneNote content elements.
    /// </summary>
    public class OEChildren : PageObjectBase
    {
        /// <summary>
        ///     Initialize a nested, indented content proxy obkect with a collection
        ///     of page objects.
        /// </summary>
        /// <param name="ns">The namespace to use to create the group XML element `one:OEChildred`</param>
        /// <param name="elements">Page objects to add as nested, indented content.</param>
        public OEChildren(XNamespace ns, IEnumerable<OE> elements)
           : base(new XElement(ns.GetName("OEChildren"), from oe in elements select oe.Element)) {
        }
    }
}
