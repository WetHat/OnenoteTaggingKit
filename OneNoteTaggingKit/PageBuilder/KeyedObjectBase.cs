using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Base class for OneNote page objects which must have a key which is
    /// unique in a page.
    /// </summary>
    /// <remarks>
    ///     For a CML page element to have a key it must have a `name` attribute.
    /// </remarks>
    public class KeyedObjectBase : PageObjectBase
    {
        /// <summary>
        /// Get the unique key of the page element.
        /// </summary>
        public string Key {
            get => GetAttributeValue("name");
        }

        /// <summary>
        /// Initialize an keyed OneNote page element existing ,
        /// </summary>
        /// <remarks>
        ///     Keyed element must have a `name` property containing a key
        ///     which is unique to the page.
        /// </remarks>
        /// <param name="element">
        ///     An XML element of an keyed element existing
        ///     on a OneNote page.
        /// </param>
        public KeyedObjectBase(XElement element) : base(element) { }

        /// <summary>
        /// Initialize a new keyed OneNote page element
        /// </summary>
        /// <param name="name">The Xml element name</param>
        /// <param name="key"></param>
        protected KeyedObjectBase(XName name,string key) : base(new XElement(name,
                                                                     new XAttribute("name", key))) {
        }
    }
}
