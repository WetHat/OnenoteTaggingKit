using System.Collections.Generic;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Collections of OneNote page elements with unique keys.
    /// </summary>
    /// <typeparam name="T">
    ///     Type of the keyed OneNote page element to manage in this collection
    /// </typeparam>
    public abstract class KeyedElementCollection<T>: StructureElementCollection<T> where T : KeyedObjectBase {
        /// <summary>
        /// Initialize an instance of this collection for elements with a
        /// given name found on a OneNote page XML document .
        /// </summary>
        /// <param name="page">The OneNote page Xml document.</param>
        /// <param name="elementName">The XML element</param>
        public KeyedElementCollection(XDocument page, XName elementName) : base (page,elementName) {
        }
    }
}
