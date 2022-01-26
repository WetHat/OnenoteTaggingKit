using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Abstract base class for collections of new or existing OneNote top-level
    /// structure object proxies which have to appear in a given sequence on the
    /// OneNote page.
    /// </summary>
    public abstract class StructureElementCollection<T> where T : PageObjectBase
    {
        /// <summary>
        /// The OneNote page document this collection relates to.
        /// </summary>
        protected XDocument Page { get;}

        /// <summary>
        /// Get the name of the XML elements in this collection.
        /// </summary>
        protected XName ElementName { get; }

        /// <summary>
        /// The list of elements in this collection
        /// </summary>
        protected List<T> Elements { get; } = new List<T>();
        /// <summary>
        /// The sequence of structure elements in which elements have to
        /// appear on a OneNote page.
        /// </summary>
        static readonly string[] PositionKeys = new string[] { "TagDef", "QuickStyleDef", "XPSFile", "Meta", "MediaPlaylist", "MeetingInfo", "PageSettings", "Title", "Outline" };

        /// <summary>
        /// Determine if the collection, which was originally loaded from the page,
        /// was modified
        /// </summary>
        public bool IsModified { get; protected set; }
        /// <summary>
        /// Initialize an instance of this collection for elements with a
        /// given name found on a OneNote page XML document .
        /// </summary>
        /// <param name="page">The OneNote page Xml document.</param>
        /// <param name="elementName">The XML element</param>
        public StructureElementCollection(XDocument page, XName elementName) {
            Page = page;
            ElementName = elementName;
            foreach (var m in page.Root.Elements(ElementName)) {
                Elements.InsertRange(0, from xe in page.Root.Elements(ElementName)
                                        select CreateElement(xe));
            }
        }
        /// <summary>
        /// Factory method to create a structure object proxies from an
        /// XML element which already exists on a OneNote page
        /// </summary>
        /// <param name="e">
        ///     XML element with the name defined in
        ///     <see cref="ElementName"/></param>
        /// <returns>An instance of a structure object of type T.</returns>
        protected abstract T CreateElement(XElement e);

        /// <summary>
        /// Add a new element at the end of the collection.
        /// </summary>
        /// <remarks>
        /// New elements are automativally added to the pages XmlDocument.
        /// </remarks>
        /// <param name="element">New element to add.</param>
        public virtual void Add(T element) {
            if (Elements.Count > 0) {
                // use the page document of the last element in the list
                // to determine the inserteion point.
                Elements[Elements.Count - 1].Element.AddAfterSelf(element.Element);
                Elements.Add(element);
                IsModified = true;
            } else {
                // must determine a reference element where to add the element
                int startindex = Array.IndexOf<string>(PositionKeys, ElementName.LocalName);
                if (startindex >= 0) {
                    for (int i = startindex + 1; i < PositionKeys.Length; i++) {
                        XElement successor = Page.Root.Element(element.GetName(PositionKeys[i]));
                        if (successor != null) {
                            successor.AddBeforeSelf(element.Element);
                            Elements.Add(element);
                            return;
                        }
                    }
                }
                // add to the end of the page
                Page.Root.Add(element.Element);
                Elements.Add(element);
                IsModified = true;
            }
        }
    }
}
