using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// The meta information on a OneNote page.
    /// </summary>
    public class Meta : KeyedObjectBase {

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
        /// <param name="meta"></param>
        public Meta (XElement meta) :base (meta) {
            Value = GetAttributeValue("content");
        }

        /// <summary>
        /// Initialize a new instance of a Meta proxy object for a given
        /// key/value pair.
        /// </summary>
        /// <param name="ns">The Xml NameSpace to use.</param>
        /// <param name="key">The unique key of the Meta element</param>
        /// <param name="value">The value associated with the key.</param>
        public Meta(XNamespace ns,string key, string value)
            : base (ns.GetName("Meta"),key) {
            Value = value;
        }
    }
}
