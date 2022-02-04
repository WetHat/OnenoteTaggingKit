using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// The collection of Meta objects for Meta XML elements on
    /// a OneNote page. Meta objects with special semantics are accessible via
    /// the properties:
    /// <list type="bullet">
    ///     <item><see cref="PageTags"/></item>
    ///     <item><see cref="SearchScope"/>/></item>
    /// </list>
    /// </summary>
    public class MetaCollection : PageStructureObjectCollection<Meta> {

        /// <summary>
        /// The Meta key of page tags.
        /// </summary>
        /// <remarks>For backwards compatibility this key should never change.</remarks>
        public const string PageTagsMetaKey = "TaggingKit.PageTags";
        /// <summary>
        /// The Meta key for scopes of saved searches.
        /// </summary>
        ///  <remarks>For backwards compatibility this key should never change.</remarks>
        public const string SearchScopeMetaKey = "TaggingKit.SearchScope";

        Meta _pageTags;
        /// <summary>
        /// Get the comma separated list of page tags on this page.
        /// </summary>
        public string PageTags {
            get {
                return _pageTags != null ? _pageTags.Value : String.Empty;
            }
            set {
                if (_pageTags != null) {
                    if (!_pageTags.Value.Equals(value)) {
                        _pageTags.Value = value;
                        IsModified = true;
                    }
                } else if (!string.IsNullOrWhiteSpace(value)) {
                    _pageTags = new Meta(Page, PageTagsMetaKey, value);
                    Add(_pageTags);
                }
            }
        }

        Meta _searchScope;
        /// <summary>
        /// Get the ID of the hierarchy element the saved search on this page
        /// was created for.
        /// </summary>
        public string SearchScope {
            get {
                return _searchScope != null ? _searchScope.Value : String.Empty;
            }
            set {
                if (_searchScope != null) {
                    _searchScope.Value = value;
                    IsModified = true;
                } else if (!string.IsNullOrWhiteSpace(value)) {
                    _searchScope = new Meta(Page, SearchScopeMetaKey, value);
                    Add(_searchScope);
                }
            }
        }

        /// <summary>
        /// Factory method to Create a meta element for a Meta element which
        /// exists on a OneNote page.
        /// </summary>
        /// <param name="e">XML element representing the Meta element</param>
        /// <returns>
        ///     A new instance of a <see cref="Meta"/> object.
        /// </returns>
        protected override Meta CreateElementProxy(XElement e) {
            Meta m = new Meta(Page,e);
            // watch out for well known Meta XML elements.
            switch (m.Name) {
                case PageTagsMetaKey:
                    _pageTags = m;
                    break;
                case SearchScopeMetaKey:
                    _searchScope = m;
                    break;
            }
            return m;
        }
        /// <summary>
        /// Intitialize the collection Meta element proxies from an OneNote page.
        /// </summary>
        /// <param name="page">The OneNote page proxy object.</param>
        public MetaCollection(OneNotePage page) : base(page, page.GetName(nameof(Meta))) {
        }
    }
}
