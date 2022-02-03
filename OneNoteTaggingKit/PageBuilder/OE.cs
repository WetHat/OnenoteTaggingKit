using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy object for OneNote page content elements.
    /// </summary>
    public class OE : PageObjectBase {
        /// <summary>
        /// Get the collection of tag proxies for this content element.
        /// </summary>
        public TagCollection Tags { get; }

        /// <summary>
        /// Get the unique OneNote id of that element.
        /// </summary>
        /// <remarks>for new Elements that ID is `null`.</remarks>
        public string ElementId => GetAttributeValue("objectID");

        /// <summary>
        /// Get/set the style index.
        /// </summary>
        /// <remarks>There is supposed to be a <see cref="QuickStyleDef"/> style
        /// definition with thar index.</remarks>
        public int QuickStyleIndex {
            get {
                string value = GetAttributeValue("quickStyleIndex");
                return value != null ? int.Parse(value) : default;
            }
            set {
                SetAttributeValue("quickStyleIndex", value.ToString());
            }
        }

        /// <summary>
        /// Initialize a content element proxy object from an 'one:OE' XML
        /// element which is part of a OneNote page XML document.
        /// </summary>
        /// <param name="element">XML element on a OneNote page.</param>
        public OE(XElement element) : base(element) {
            Tags = new TagCollection(this);
        }
    }
}
