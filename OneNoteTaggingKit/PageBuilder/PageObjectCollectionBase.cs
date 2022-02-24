using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// A collection of proxy objects for XML elements of the same type,
    /// </summary>
    /// <typeparam name="Towner">Proxy object type of the collection owner.</typeparam>
    /// <typeparam name="Titem">Proxy object type of the items in the collection.</typeparam>
    public abstract class PageObjectCollectionBase<Towner,Titem> : IEnumerable<Titem> where Towner : PageObjectBase
                                                                                      where Titem : PageObjectBase
    {
        /// <summary>
        /// Get the name of the XML elements in this collection.
        /// </summary>
        public XName ElementName { get; }

        /// <summary>
        /// Get the element procy owning this colleciton.
        /// </summary>
        public Towner Owner { get; }
        /// <summary>
        /// Get the XML namespace this collection is defined in.
        /// </summary>
        public XNamespace Namespace { get => ElementName.Namespace; }
        /// <summary>
        /// Get an XML name using the namespave associated with this collection.
        /// </summary>
        /// <param name="name">Local name.</param>
        public XName GetName(string name) => ElementName.Namespace.GetName(name);

        /// <summary>
        /// The list of proxy objects in this collection.
        /// </summary>
        protected List<Titem> Items { get; } = new List<Titem>();

        /// <summary>
        /// Initialize the proxy object collection with elements contained under
        /// an owning element on a OneNote page XML document.
        /// </summary>
        /// <param name="name">
        ///     The XML name of the elements proxied by objects in this collection.
        /// </param>
        /// <param name="owner">
        ///     The element proxy owning the objects in this colelction..
        /// </param>
        /// <param name="selector">
        ///     Lambda function to select the XML elements to
        ///     populate the collection with. If not provided all elements with
        ///     the given name added to the collection.
        /// </param>
        protected PageObjectCollectionBase(XName name, Towner owner, Func<XElement,IEnumerable<XElement>> selector = null) {
            ElementName = name;
            Owner = owner;
            // collect existing
            if (selector == null) {
                Items.InsertRange(0, from xe in owner.Element.Elements(name)
                                     select CreateElementProxy(xe));
            }
            else {
                Items.InsertRange(0, from XElement xe in selector(owner.Element)
                                     select CreateElementProxy(xe));
            }
        }

        /// <summary>
        /// Add a new element at the end of the collection.
        /// </summary>
        /// <remarks>
        /// New elements are **not** added to an owner by default. It is the responsibility
        /// of the derived classes to do that in a way that is consistent with
        /// the page schema.
        /// </remarks>
        /// <param name="proxy">New proxy object to add.</param>
        protected virtual void Add(Titem proxy) {
            Items.Add(proxy);
        }

        /// <summary>
        /// Factory method to create a structure object proxy from a
        /// XML element which already exists on a OneNote page document.
        /// </summary>
        /// <param name="e">
        ///     XML element selected from a OneNote page document.</param>
        /// <returns>An instance of a proxy object of type T.</returns>
        protected abstract Titem CreateElementProxy(XElement e);

        #region IEnumerable<Titem>
        /// <summary>
        /// Get the enumerator of items in this collection.
        /// </summary>
        /// <returns>Item enumerator</returns>
        public IEnumerator<Titem> GetEnumerator() => Items.GetEnumerator();
        /// <summary>
        /// Get the enumerator of objects in this collection.
        /// </summary>
        /// <returns>Object enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
        #endregion IEnumerable<Titem>
    }
}
