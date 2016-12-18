////////////////////////////////////////////////////////////
// Author: WetHat
// (C) Copyright 2015, 2016 WetHat Lab, all rights reserved
////////////////////////////////////////////////////////////
using Microsoft.Office.Interop.OneNote;
using System.Collections.Generic;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Observable collections of tags and OneNote pages satisfying a search criterion.
    /// </summary>
    /// <remarks>
    /// Provides a refineable unordered set of tags and pages. The page collection is
    /// built by calling <see cref="FindTaggedPages"/> and can be progressively refined (filtered)
    /// by adding filter tags (<see cref="AddTagToFilter"/>)
    /// </remarks>
    public class FilterablePageCollection : TagsAndPages
    {
        /// <summary>
        /// tag objects used for filtering
        /// </summary>
        /// <remarks>Contains live tags only and hence is a subset of <see cref="_tagfilter"/></remarks>
        private ISet<TagPageSet> _filterTags = new HashSet<TagPageSet>();

        /// <summary>
        /// Names of tags intended for filtering. May contain names of non-existing tags.
        /// </summary>
        private ISet<string> _tagFilter = new HashSet<string>();

        /// <summary>
        /// Set of pages after tag filters have been applied.
        /// </summary>
        private ObservableDictionary<string, TaggedPage> _filteredPages = new ObservableDictionary<string, TaggedPage>();

        internal FilterablePageCollection(OneNoteProxy onenote) : base(onenote)
        {
        }

        /// <summary>
        /// Find pages in OneNote.
        /// </summary>
        /// <remarks>
        /// Calling this method may cause tags in the filter to become stale. It is the responsibility
        /// of the caller to update tag objects it may have associated with the filter.
        /// </remarks>
        /// <param name="query">query string. if null or empty just the tags are provided</param>
        /// <param name="scopeID">OneNote id of the scope to search for pages. This is the element ID of a notebook, section group, or section.
        ///                       If given as null or empty string scope is the entire set of notebooks open in OneNote.
        /// </param>
        /// <param name="includeUnindexedPages">include pages which were not indexed so far</param>
        /// <seealso cref="Filter"/>
        internal void Find(string query, string scopeID, bool includeUnindexedPages = false)
        {
            _filteredPages.Clear();
            _filterTags.Clear();
            if (string.IsNullOrEmpty(query))
            {
                // collect all tags used somewhere on a page
                FindTaggedPages(scopeID, includeUnindexedPages);
            }
            else
            {
                // run a text search
                FindTaggedPages(query, scopeID, includeUnindexedPages);
            }

            // rebuild filter tags and filtered pages
            int filtersApplied = 0;
            foreach (string tagname in _tagFilter)
            {
                TagPageSet t;
                if (Tags.TryGetValue(tagname, out t))
                {
                    _filterTags.Add(t);
                    if (filtersApplied++ == 0)
                    {
                        _filteredPages.UnionWith(t.Pages);
                    }
                    else
                    {
                        _filteredPages.IntersectWith(t.Pages);
                    }
                }
            }
            if (filtersApplied == 0)
            {
                _filteredPages.UnionWith(Pages.Values);
            }
            ApplyFilterToTags();
        }

        /// <summary>
        /// Get the dictionary of pages
        /// </summary>
        internal ObservableDictionary<string, TaggedPage> FilteredPages
        {
            get { return _filteredPages; }
        }

        /// <summary>
        /// Get the set of tag names used for filtering
        /// </summary>
        /// <value>Not all tags returned in the set may be life.</value>
        internal ISet<string> Filter
        {
            get { return _tagFilter; }
        }

        /// <summary>
        /// Undo all tag filters
        /// </summary>
        internal void ClearTagFilter()
        {
            _filterTags.Clear();
            _tagFilter.Clear();
            _filteredPages.UnionWith(Pages.Values);
            foreach (TagPageSet tag in Tags.Values)
            {
                tag.ClearFilter();
            }
        }

        /// <summary>
        /// Filter pages by tag.
        /// </summary>
        /// <remarks>
        ///   Filters pages down to a collection where all pages have this tag and also all tags from preceding
        ///   calls to this method.
        /// </remarks>
        /// <param name="tagName">tag to filter on</param>
        internal void AddTagToFilter(string tagName)
        {
            if (_tagFilter.Add(tagName))
            {
                TagPageSet tag;
                if (Tags.TryGetValue(tagName, out tag))
                {
                    _filterTags.Add(tag);
                    _filteredPages.IntersectWith(tag.FilteredPages);
                    ApplyFilterToTags();
                }
            }
        }

        /// <summary>
        /// Remove tag from the filter
        /// </summary>
        /// <param name="tagName">tag to remove</param>
        internal void RemoveTagFromFilter(string tagName)
        {
            if (_tagFilter.Remove(tagName))
            {
                TagPageSet tag;
                if (Tags.TryGetValue(tagName, out tag))
                {
                    _filterTags.Remove(tag);
                    if (_filterTags.Count == 0)
                    {
                        ClearTagFilter();
                    }
                    else
                    {
                        // recompute filtered pages from scratch
                        _filteredPages.UnionWith(Pages.Values);
                        foreach (TagPageSet tps in _filterTags)
                        {
                            _filteredPages.IntersectWith(tps.Pages);
                        }
                        ApplyFilterToTags();
                    }
                }
            }
        }

        /// <summary>
        /// Apply the current page filter
        /// </summary>
        private void ApplyFilterToTags()
        {
            if (_filterTags.Count == 0)
            {
                foreach (TagPageSet tag in Tags.Values)
                {
                    tag.ClearFilter();
                }
            }
            else
            {
                foreach (TagPageSet tag in Tags.Values)
                {
                    tag.IntersectWith(_filteredPages.Values);
                }
            }
        }
    }
}