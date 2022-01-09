// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Observable collections of tags and OneNote pages satisfying a
    /// search criterion and having common tags.
    /// </summary>
    /// <remarks>
    /// Provides a refineable unordered set of tags and pages. The page collection is built
    /// by calling <see cref="FindTaggedPages" /> and can be progressively refined
    /// (filtered) by adding filter tags ( <see cref="AddTagToFilter" />)
    /// </remarks>
    [ComVisible(false)]
    public class FilteredPages : TagsAndPages
    {
        /// <summary>
        /// Page tag objects currently used for refinement.
        /// </summary>
        private ISet<TagPageSet> _filterTags = new HashSet<TagPageSet>();

        /// <summary>
        /// The current text query used to search for pages.
        /// </summary>
        private string _query;

        internal FilteredPages(OneNoteProxy onenote) : base(onenote)
        {
        }

        /// <summary>
        /// Find pages in OneNote.
        /// </summary>
        /// <remarks>
        /// Calling this method may cause tags in the filter to become stale. It is the
        /// responsibility of the caller to update tag objects it may have associated with
        /// the filter.
        /// </remarks>
        /// <param name="query">  query string. if null or empty just the tags are provided</param>
        /// <param name="scopeID">
        /// OneNote id of the scope to search for pages. This is the element ID of a
        /// notebook, section group, or section. If given as null or empty string scope is
        /// the entire set of notebooks open in OneNote.
        /// </param>
        /// <seealso cref="Filter" />
        internal void Find(string query, string scopeID)
        {
            _query = query;
            if (string.IsNullOrEmpty(query))
            {
                // collect all tags used somewhere on a page
                FindTaggedPages(scopeID);
            }
            else
            {
                // run a text search
                FindTaggedPages(query, scopeID);
            }

            MatchingPages.Clear();
            _filterTags.IntersectWith(Tags.Values); // remove obsolete tags
            // re-apply the tag filter and remove obsolete tags
            int filtersApplied = 0;
            foreach (TagPageSet tag in _filterTags)
            {
                if (filtersApplied++ == 0) {
                    MatchingPages.UnionWith(tag.Pages);
                } else {
                    MatchingPages.IntersectWith(tag.Pages);
                }
            }
            if (filtersApplied == 0 && !string.IsNullOrEmpty(query))
            {   // as there are no filters we simply show the entire
                // query result
                MatchingPages.UnionWith(base.Pages.Values);
            }
            ApplyFilterToTags();
        }

        /// <summary>
        /// Get the dictionary of pages.
        /// </summary>
        public ObservableDictionary<string, TaggedPage> MatchingPages { get; } = new ObservableDictionary<string, TaggedPage>();

        /// <summary>
        /// Get the set of tag names used for filtering
        /// </summary>
        /// <value>Not all tags returned in the set may be life.</value>
        public ISet<TagPageSet> Filter => _filterTags;

        /// <summary>
        /// Undo all tag filters
        /// </summary>
        internal void ClearTagFilter()
        {
            _filterTags.Clear();
            MatchingPages.Clear();
            if (!string.IsNullOrEmpty(_query))
            {
                MatchingPages.UnionWith(base.Pages.Values);
            }
            foreach (TagPageSet tag in Tags.Values)
            {
                tag.ClearFilter();
            }
        }

        /// <summary>
        /// Filter pages by tag.
        /// </summary>
        /// <remarks>
        /// Filters pages down to a collection where all pages have this tag and also all
        /// tags from preceding calls to this method.
        /// </remarks>
        /// <param name="tag">Page tag to add to refinement filter.</param>
        internal void AddTagToFilter(TagPageSet tag)
        {
            if (_filterTags.Add(tag))
            {
                if (MatchingPages.Count == 0) {
                    MatchingPages.UnionWith(tag.FilteredPages);
                } else {
                    MatchingPages.IntersectWith(tag.FilteredPages);
                }
                ApplyFilterToTags();
            }
        }

        /// <summary>
        /// Remove tag from the filter.
        /// </summary>
        /// <param name="tag">Page tag to remove from refinement filter.</param>
        internal void RemoveTagFromFilter(TagPageSet tag)
        {
            if (_filterTags.Remove(tag)) {
                if (_filterTags.Count == 0) {
                    ClearTagFilter();
                } else {
                    // recompute filtered pages from scratch
                    MatchingPages.Clear();
                    int tagsApplied = 0;
                    foreach (TagPageSet tps in _filterTags) {
                        if (tagsApplied++ == 0) {
                            MatchingPages.UnionWith(tps.Pages);
                        } else {
                            MatchingPages.IntersectWith(tps.Pages);
                        }
                    }
                    ApplyFilterToTags();
                }
            }
        }

        /// <summary>
        /// Apply the current page filter to all refinement tags.
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
                    tag.IntersectWith(MatchingPages.Values);
                }
            }
        }
    }
}