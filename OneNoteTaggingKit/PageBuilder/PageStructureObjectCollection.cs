﻿using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    ///     Abstract base class for collections of new or existing OneNote top-level
    ///     structure object proxies which have to appear in a given sequence on the
    ///     OneNote page.
    /// </summary>
    [ComVisible(false)]
    public abstract class PageStructureObjectCollection<T> : PageObjectCollectionBase<OneNotePage,T> where T : PageStructureObjectBase
    {
        /// <summary>
        ///     Get the schema position of the objects managed by this collection.
        /// </summary>
        public PageSchemaPosition SchemaPosition { get; private set; }
        /// <summary>
        /// The OneNote page proxy object this collection relates to.
        /// </summary>
        protected OneNotePage Page { get; }

        /// <summary>
        /// Determine if the collection, which was originally loaded from the page,
        /// is now modified.
        /// </summary>
        /// <remarks>T
        ///     his information is used to decide whether the
        ///     page needs to be saved.
        /// </remarks>
        public bool IsModified { get; protected set; }

        /// <summary>
        ///     Initialize an instance of this collection with elements
        ///     found on a OneNote page XML document .
        /// </summary>
        /// <remarks>
        ///     The type of element to collect is inferred from
        ///     the given schema position.
        /// </remarks>
        /// <param name="page">
        ///     The OneNote page document proxy.
        /// </param>
        /// <param name="name">
        ///     The XML name of the items in this colelction.
        /// </param>
        /// <param name="position">
        ///     The page position of elements of this type according to the page schema.
        /// </param>
        protected PageStructureObjectCollection(XName name, OneNotePage page, PageSchemaPosition position)
            : base(name,page) {
            Page = page;
            SchemaPosition = position;
        }

        /// <summary>
        /// Add a new element at the end of the collection.
        /// </summary>
        /// <remarks>
        /// New elements are automatically added to the page's XmlDocument.
        /// </remarks>
        /// <param name="proxy">New proxy object to add.</param>
        protected override void Add(T proxy) {
            base.Add(proxy);
            Page.Add(proxy,SchemaPosition); // add to page too.
            IsModified = true;
        }
    }
}
