using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Base class for OneNote page objects which must have a name which is
    /// for some derived some unique in a page.
    /// </summary>
    /// <remarks>
    ///     The XML page element must have a `name` attribute.
    /// </remarks>
    public class NamedObjectBase : PageObjectBase
    {
        /// <summary>
        /// Get/set the name of the page element.
        /// </summary>
        public string Name {
            get => GetAttributeValue("name");
            set => SetAttributeValue("name", value);
        }

        /// <summary>
        /// Initialize a named OneNote page object proxy with a
        /// XML element existing on a OneNote page.
        /// </summary>
        /// <remarks>
        ///     The element must have a `name` property.
        /// </remarks>
        /// <param name="element">
        ///     An XML element of an keyed element existing
        ///     on a OneNote page.
        /// </param>
        public NamedObjectBase(XElement element) : base(element) { }

        /// <summary>
        /// Initialize a new keyed OneNote page element
        /// </summary>
        /// <param name="name">The Xml element name</param>
        /// <param name="nameAtt">The name attribute value</param>
        protected NamedObjectBase(XName name,string nameAtt) : base(new XElement(name,
                                                                     new XAttribute("name", key))) {
        }
    }
}
