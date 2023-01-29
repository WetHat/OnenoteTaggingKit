using System.Xml.Linq;
using System.Linq;
namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy object for OneNote page content elements.
    /// </summary>
    public class OE : PageObjectBase {
        /// <summary>
        /// Get the collection of tag proxies for this content element.
        /// </summary>
        public TagCollection Tags { get; protected set; }

        /// <summary>
        ///     Set indented content for this page object.
        /// </summary>
        public OEChildren Children { 
            set {
                Element.Add(value.Element);
            }
        }

        int _bullet = -1;
        /// <summary>
        /// Set the List item bullet type.
        /// </summary>
        public int Bullet {
            set {
                if (_bullet != value) {
                    XElement bullet = Element.Descendants(GetName("Bullet")).FirstOrDefault();
                    if (bullet == null) {
                        XElement list = new XElement(GetName("List"),
                                            new XElement(GetName("Bullet"),
                                                new XAttribute("bullet", value)));
                        if (Tags.LastTag == null) {
                            Element.AddFirst(list);
                        } else {
                            Tags.LastTag.Element.AddAfterSelf(list);
                        }
                    } else {
                        bullet.SetAttributeValue("bullet", value);
                    }
                }
            }
        }
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
        /// Set the Style to use for this page content element.
        /// </summary>
        public QuickStyleDef QuickStyle {
            set {
                QuickStyleIndex = value.Index;
            }
        }

        /// <summary>
        /// Initialize a new instance of a `one:OE` proxy with data from
        /// a generic page element proxy.
        /// </summary>
        /// <param name="source">The proxy sourcing the data.</param>
        protected OE(OE source) : base(source.Element) {
            Tags = source.Tags;
        }

        /// <summary>
        /// Initialize a content element proxy object from an 'one:OE' XML
        /// element which is part of a OneNote page XML document.
        /// </summary>
        /// <param name="element">XML element on a OneNote page.</param>
        public OE(XElement element) : base(element) {
            Tags = new TagCollection(this);
        }

        /// <summary>
        /// Intialize a new element proxy with additional XML page content.
        /// </summary>
        /// <param name="ns">The XML namespave to use.</param>
        /// <param name="content">Zero or more content elements</param>
        protected OE(XNamespace ns, params XElement[] content) : this(new XElement(ns.GetName("OE"))) {
            Element.Add(content);
        }
    }
}
