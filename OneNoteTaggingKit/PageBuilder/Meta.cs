using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy for a `one:Meta`meta element on a OneNote page document.
    /// </summary>
    public class Meta : NamedObjectBase {

        /// <summary>
        /// Get/set the value of the Meta element.
        /// </summary>
        public string Value {
            get => GetAttributeValue("content");
            set => SetAttributeValue("content", value);
        }

        /// <summary>
        /// Initialize a Meta proxy object from an existing Meta XML element on a
        /// OneNote page.</summary>
        /// <param name="page">Proxy of the page which owns this object.</param>
        /// <param name="element"></param>
        public Meta (OneNotePage page, XElement element) :base (page, element) {
        }

        /// <summary>
        /// Initialize a proxy with a new 'one:Meta' XML element.
        /// </summary>
        /// <param name="page">Proxy of the page which owns this object.</param>
        /// <param name="name">The `name` attribute value of the Meta element</param>
        /// <param name="value">The `content` attibute value of the meta element.</param>
        public Meta(OneNotePage page, string name, string value)
            : base (page,
                    new XElement(page.GetName(nameof(Meta)),
                        new XAttribute("content",value)),
                    name) {
        }
    }
}
