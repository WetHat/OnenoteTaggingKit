using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy object for OneNote tag `one:Tag` elements.
    /// </summary>
    public class Tag : PageObjectBase
    {
        /// <summary>
        /// Get/set the index of the definition object
        /// </summary>
        public int Index {
            get {
                string value = GetAttributeValue("index");
                return value != null ? int.Parse(value) : default;
            }
            set {
                SetAttributeValue("index", value.ToString());
            }
        }

        /// <summary>
        /// Initialize a new proxy instance from an existing `one:Tag` element
        /// on a OneNote page.
        /// </summary>
        /// <param name="element">OneNote tag XML element.</param>
        public Tag(XElement element) : base(element) {
        }

        /// <summary>
        /// Initialize a new instance of a tag proxy object
        /// </summary>
        /// <param name="owner">Proxy object whose element is tagged.</param>
        /// <param name="index">Tag index referring to a tag definition.</param>
        public Tag(OE owner,int index) : base(new XElement(owner.GetName(nameof(Tag)),
                                                  new XAttribute("index", index),
                                                  new XAttribute("completed", "true"))) {
        }
    }
}
