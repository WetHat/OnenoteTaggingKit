using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// Sequence position of page structure elements
    public enum PagePosition
    {
        /// <summary>
        /// Position of tag definition elements on a OneNote page.
        /// </summary>
        TagDef = 0,
        /// <summary>
        /// Position of Meta elements on a OneNote page.
        /// </summary>
        Meta = 3
    }

    /// <summary>
    /// Abstract base class for collections of new or existing OneNote top-level
    /// structure object proxies which have to appear in a given sequence on the
    /// OneNote page.
    /// </summary>
    public abstract class PageStructureObjectCollection<T> where T : PageObjectBase
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
        /// The list of proxy objects in this collection.
        /// </summary>
        protected List<T> Items { get; } = new List<T>();
        /// <summary>
        /// The sequence of structure elements in which elements have to
        /// appear on a OneNote page.
        /// </summary>
        /// <remarks>
        /// The schema for stucture elements on a OneNote page is defined as:
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
        /// <param name="position">The position of elements on the page.</param>
        protected PageStructureObjectCollection(XDocument page, PagePosition position) {
            Page = page;
            // lookup the element name
            var localname = PositionKeys[(int)position];
            ElementName = page.Root.Name.Namespace.GetName(localname);
            foreach (var m in page.Root.Elements(ElementName)) {
                Items.InsertRange(0, from xe in page.Root.Elements(ElementName)
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
        /// <param name="proxy">New proxy object to add.</param>
        public virtual void Add(T proxy) {
            if (Items.Count > 0) {
                // use the page document of the last element in the list
                // to determine the inserteion point.
                Items[Items.Count - 1].Element.AddAfterSelf(proxy.Element);
                Items.Add(proxy);
                IsModified = true;
            } else {
                // must determine a reference element where to add the element
                int startindex = Array.IndexOf<string>(PositionKeys, ElementName.LocalName);
                if (startindex >= 0) {
                    for (int i = startindex + 1; i < PositionKeys.Length; i++) {
                        XElement successor = Page.Root.Element(proxy.GetName(PositionKeys[i]));
                        if (successor != null) {
                            successor.AddBeforeSelf(proxy.Element);
                            Items.Add(proxy);
                            return;
                        }
                    }
                }
                // add to the end of the page
                Page.Root.Add(proxy.Element);
                Items.Add(proxy);
                IsModified = true;
            }
        }
    }
}
