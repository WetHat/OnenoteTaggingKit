using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// A collection of proxy objects for XML elements of the same type,
    /// </summary>
    /// <typeparam name="T">Proxy object type.</typeparam>
    public abstract class PageObjectCollectionBase<T> : IEnumerable<T> where T : PageObjectBase
    {
        /// <summary>
        /// Get the name of the XML elements in this collection.
        /// </summary>
        public XName ElementName { get; }

        /// <summary>
        /// Get the XML namesapace associated with this collection.
        /// </summary>
        public XNamespace Namespace => ElementName.Namespace;

        /// <summary>
        /// The list of proxy objects in this collection.
        /// </summary>
        protected List<T> Items { get; } = new List<T>();

        /// <summary>
        /// Initialize the proxy object collection with elements contained under
        /// an owning element.
        /// </summary>
        /// <param name="name">
        ///     The XML name of the elements proxied by objects in this collection.
        /// </param>
        /// <param name="owner">
        ///     The XML element owning the XML elements of the
        ///     proxy objects in this collection.
        /// </param>
        public PageObjectCollectionBase(XName name,XElement owner) {
            ElementName = name;
            // collect existing
            Items.InsertRange(0, from xe in owner.Elements(name)
                                 select CreateElementProxy(xe));
        }

        /// <summary>
        /// Add a new element at the end of the collection.
        /// </summary>
        /// <remarks>
        /// New elements are not added to an owner by default. It is the responsibility
        /// of the derived classes to do that in a way that is consistent with
        /// the page schema.
        /// </remarks>
        /// <param name="proxy">New proxy object to add.</param>
        protected virtual void Add(T proxy) {
            Items.Add(proxy);
        }

        /// <summary>
        /// Factory method to create a structure object proxy from a
        /// XML element which already exists on a OneNote page document.
        /// </summary>
        /// <param name="e">
        ///     XML element selected from a OneNote page document.</param>
        /// <returns>An instance of a proxy object of type T.</returns>
        protected abstract T CreateElementProxy(XElement e);

        #region IEnumerable<T>
        /// <summary>
        /// Get the enumerator of items in this collection.
        /// </summary>
        /// <returns>Item enumerator</returns>
        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();
        /// <summary>
        /// Get the enumerator of objects in this collection.
        /// </summary>
        /// <returns>Object enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();
        #endregion IEnumerable<T>
    }
}
