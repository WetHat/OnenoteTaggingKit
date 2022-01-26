using System.Collections.Generic;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Base class for OneNote page element proxy objects.
    /// </summary>
    /// <remarks>
    ///     Instances of classes derived from this base class can be managed
    ///     in hash tables.
    /// </remarks>
    public class PageObjectBase
    {
        /// <summary>
        /// Get or set the raw XML element.
        /// </summary>
        public XElement Element { get; protected set; }

        /// <summary>
        /// Check decorators for equality.
        /// </summary>
        /// <param name="obj">decorator to check for equality</param>
        /// <returns>true, if both decorators decorate the same XML element; false otherwise</returns>
        public override bool Equals(object obj) {
            PageObjectBase other = obj as PageObjectBase;

            return other != null && Element == other.Element;
        }

        /// <summary>
        /// Get the hash code or this decorator.
        /// </summary>
        /// <returns>hash code</returns>
        public override int GetHashCode() {
            return Element.GetHashCode();
        }

        /// <summary>
        /// Get a fully qualified name
        /// </summary>
        /// <param name="name">local name</param>
        /// <returns>fully qualified name</returns>
        public XName GetName(string name) {
            return Element.Name.Namespace.GetName(name);
        }

        /// <summary>
        /// Remove element from its parent.
        /// </summary>
        public virtual void Remove() {
            Element.Remove();
        }

        /// <summary>
        /// Create a new instance of a OneNote page element
        /// </summary>
        /// <param name="name">Element name</param>
        /// <param name="children">zero or more child elements</param>
        protected PageObjectBase(XName name, params PageObjectBase[] children) {
            Element = new XElement(name);
            Element.Add(Unwrap(children));
        }

        /// <summary>
        /// Create and decorate an XML element in a OneNote page document.
        /// </summary>
        /// <param name="name">Element name</param>
        /// <param name="children">collection of child elements</param>
        protected PageObjectBase(XName name, IEnumerable<PageObjectBase> children) {
            Element = new XElement(name);
            Element.Add(Unwrap(children));
        }

        /// <summary>
        /// Decorate an existing XML element.
        /// </summary>
        /// <param name="element">XML element to decorate</param>
        protected PageObjectBase(XElement element) {
            Element = element;
        }

        /// <summary>
        /// Create an decorator instance
        /// </summary>
        /// <remarks>
        ///   Use the <see cref="Element"/> method to set the XML element to decorate.
        /// </remarks>
        protected PageObjectBase() {
        }

        /// <summary>
        /// Unwrap decorated elements
        /// </summary>
        /// <param name="elements">collection of decorated elements</param>
        /// <returns>unwrapped XML elements</returns>
        protected static IEnumerable<XElement> Unwrap(IEnumerable<PageObjectBase> elements) {
            foreach (var element in elements) {
                yield return element.Element;
            }
        }

        /// <summary>
        /// Get the value of an attribute
        /// </summary>
        /// <param name="name">attribute name</param>
        /// <returns>attribute value or null if the attribute is not set</returns>
        protected string GetAttributeValue(string name) {
            XAttribute att = Element.Attribute(name);
            return att != null ? att.Value : null;
        }

        /// <summary>
        /// Set an attribute value
        /// </summary>
        /// <remarks>
        ///    If the attribute does not exist, it is created.
        ///    If the value is null, the attribute is deleted.
        /// </remarks>
        /// <param name="name">attribute name</param>
        /// <param name="value">attribute value</param>
        protected void SetAttributeValue(string name, string value) {
            Element.SetAttributeValue(name, value);
        }
    }
}
