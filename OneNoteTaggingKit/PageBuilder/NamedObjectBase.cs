using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Base class for proxy objeccts containing a XML element with
    /// a `name` attribute.
    /// </summary>
    public class NamedObjectBase : PageStructureObjectBase
    {
        /// <summary>
        /// Get/set the name of the element.
        /// </summary>
        public string Name {
            get => GetAttributeValue("name");
            set => SetAttributeValue("name", value);
        }

        /// <summary>
        /// Initialize a proxy object with a
        /// XML element selected from a OneNote page document.
        /// </summary>
        /// <remarks>
        ///     The element must have a `name` attribute.
        /// </remarks>
        /// <param name="page">Proxy of the page which owns this object.</param>
        /// <param name="element">
        ///     An XML element of an keyed element existing
        ///     on a OneNote page.
        /// </param>
        ///
        protected NamedObjectBase(OneNotePage page, XElement element) : base(page,element) { }

        /// <summary>
        /// Initialize a proxy containing a new XML element.
        /// </summary>
        /// <param name="page">Proxy of the page which owns this object.</param>
        /// <param name="element">The XML element of the proxy.</param>
        /// <param name="name">The name attribute value</param>
        protected NamedObjectBase(OneNotePage page, XElement element ,string name)
            : base(page,element) {
            Name = name;
        }
    }
}
