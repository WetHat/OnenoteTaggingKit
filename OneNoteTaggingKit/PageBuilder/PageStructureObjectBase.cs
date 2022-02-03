using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Base class for proxy objects of OneNote page  XML elements which have
    /// to appear in a particular order on the page.
    /// </summary>
    /// <remarks>
    /// The order in which instances of sub-classes have to appear on a
    /// OneNote page is defined in the page schema like so:
    /// <code lang="xml">
    /// // Sequence of elements below the page tag
    /// <xsd:element name="TagDef" type="TagDef" minOccurs="0" maxOccurs="unbounded"/>[
    /// <xsd:element name="QuickStyleDef" type="QuickStyleDef" minOccurs="0" maxOccurs="unbounded"/>
    /// <xsd:element name="XPSFile" type="XPSFile" minOccurs="0" maxOccurs="unbounded"/>
    /// <xsd:element name="Meta" type="Meta" minOccurs="0" maxOccurs="unbounded"/>
    /// <xsd:element name="MediaPlaylist" type="MediaPlaylist" minOccurs="0"/>
    /// <xsd:element name="MeetingInfo" type="MeetingInfo" minOccurs="0"/>
    /// <xsd:element name="PageSettings" type="PageSettings" minOccurs="0"/>
    /// <xsd:element name="Title" type="Title" minOccurs="0"/>
    /// </code>
    /// </remarks>
    public class PageStructureObjectBase : PageObjectBase {

        /// <summary>
        /// Proxy for the OneNote page document which owns the element
        /// contained in this proxy.
        /// </summary>
        public OneNotePage Page { get;  }

        /// <summary>
        /// Initialize a proxy object for a XML element selected from a
        /// OneNote page document.
        /// </summary>
        /// <param name="page">Proxy of the page which owns this element.</param>
        /// <param name="element">XML element from a OneNote page document.</param>
        public PageStructureObjectBase(OneNotePage page, XElement element) : base(element) {
            Page = page;
        }
    }
}
