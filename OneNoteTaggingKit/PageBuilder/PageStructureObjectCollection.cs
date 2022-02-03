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
    public abstract class PageStructureObjectCollection<T> : PageObjectCollectionBase<T> where T : PageStructureObjectBase
    {
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
        /// Initialize an instance of this collection with elements
        /// found on a OneNote page XML document .
        /// </summary>
        /// <remarks>
        ///     The type of element to collect is inferred from
        ///     the given schema position.
        /// </remarks>
        /// <param name="page">The OneNote page Xml document.</param>
        /// <param name="name">The position of elements on the page.</param>
        protected PageStructureObjectCollection(OneNotePage page, XName name) : base(name,page.Root) {
            Page = page;
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
            Page.Add(proxy); // add to page too.
            IsModified = true;
        }
    }
}
