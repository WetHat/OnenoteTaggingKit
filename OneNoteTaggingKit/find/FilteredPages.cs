// Author: WetHat | (C) Copyright 2013 - 2023 WetHat Lab, all rights reserved
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    ///     Observable collection of tags and OneNote pages satisfying
    ///     one or more tag conditions and an optional full-text query.
    /// </summary>
    /// <remarks>
    ///     Provides a refineable unordered set of tags and pages.
    ///     The page collection is initialized by calling
    ///     <see cref="FindTaggedPages" /> and can be progressively refined
    ///     (filtered) by adding filter tags ( <see cref="AddTagToFilter" />)
    /// </remarks>
    [ComVisible(false)]
    public class FilteredPages : TagsAndPages {
        /// <summary>
        /// The current text query used to search for pages.
        /// </summary>
        private string _query;

        /// <summary>
        ///     Initialize a new instance of an empty set of tagged OneNote pages.
        /// </summary>
        /// <remarks>
        ///     Use the <see cref="Find"/> method to populate the set.
        /// </remarks>
        /// <param name="onenote">The OneNote proxy object to use</param>
        internal FilteredPages(OneNoteProxy onenote) : base(onenote) {
        }

        /// <summary>
        ///     Find tagged pages in OneNote satisfying a query or tag related
        ///     condition.
        /// </summary>
        /// <remarks>
        ///     Calling this method may cause tags in the filter to become stale.
        ///     It is the responsibility of the caller to update tag objects it
        ///     may have associated with this filter.
        /// </remarks>
        /// <param name="query">Query string. If null or empty just the tags are
        /// used to filter pages.</param>
        /// <param name="scope">The scope to search for pages.</param>
        internal void Find(string query, SearchScope scope) {
            _query = query;

            FindPages(scope, query);

            MatchingPages.Clear();
            FilterTags.IntersectWith(Tags.Values); // remove obsolete tags
            // re-apply the tag filter and remove obsolete tags
            int filtersApplied = 0;
            foreach (TagPageSet tag in FilterTags) {
                if (filtersApplied++ == 0) {
                    MatchingPages.UnionWith(tag.Pages);
                } else {
                    MatchingPages.IntersectWith(tag.Pages);
                }
            }
            if (filtersApplied == 0 && !string.IsNullOrEmpty(query)) {   // as there are no filters we simply show the entire
                // query result
                MatchingPages.UnionWith(base.Pages.Values);
            }
            ApplyFilterToTags();
        }

        /// <summary>
        ///     Get the dictionary of pages matching all criteria.
        /// </summary>
        public ObservableDictionary<string, PageNode> MatchingPages { get; } = new ObservableDictionary<string, PageNode>();

        /// <summary>
        /// Get the set of tags currently used for refinement.
        /// </summary>
        /// <value>Not all tags returned in the set may be life.</value>
        public ISet<TagPageSet> FilterTags { get; } = new HashSet<TagPageSet>();

        /// <summary>
        /// Undo all tag filters
        /// </summary>
        internal void ClearTagFilter()
        {
            if (FilterTags.Count == 0) {
                FilterTags.Clear();
                if (string.IsNullOrEmpty(_query)) {
                    MatchingPages.Clear();
                } else {
                    // Restore the query result
                    MatchingPages.UnionWith(Pages.Values);
                }
            }
            ApplyFilterToTags();
        }

        /// <summary>
        ///     Filter pages by tag.
        /// </summary>
        /// <remarks>
        ///     Filters pages down to a collection where all pages have this tag and also all
        ///     tags from preceding calls to this method.
        /// </remarks>
        /// <param name="tag">Page tag to add to refinement filter.</param>
        internal void AddTagToFilter(TagPageSet tag)
        {
            if (FilterTags.Add(tag)) {
                if (FilterTags.Count == 1 && string.IsNullOrEmpty(_query)) {
                    // first tag initializes the list of matching pages
                    MatchingPages.UnionWith(tag.FilteredPages);
                } else {
                    // incrementally refine the page collection.
                    MatchingPages.IntersectWith(tag.FilteredPages);
                }
                ApplyFilterToTags();
            }
        }

        /// <summary>
        /// Remove tag from the filter.
        /// </summary>
        /// <param name="tag">Page tag to remove from the refinement filter.</param>
        internal void RemoveTagFromFilter(TagPageSet tag)
        {
            if (FilterTags.Remove(tag)) {
                if (string.IsNullOrEmpty(_query)) {
                    MatchingPages.Clear();
                } else {
                    MatchingPages.UnionWith(Pages.Values);
                }
                if (FilterTags.Count > 0) {
                    // rebuild the collection of matching pages
                    int tagsApplied = 0;
                    foreach (TagPageSet tps in FilterTags) {
                        if (tagsApplied++ == 0 && string.IsNullOrEmpty(_query)) {
                            MatchingPages.UnionWith(tps.Pages);
                        } else {
                            MatchingPages.IntersectWith(tps.Pages);
                        }
                    }
                }
                ApplyFilterToTags();
            }
        }

        /// <summary>
        /// Apply the current page filter to all refinement tags.
        /// </summary>
        private void ApplyFilterToTags()
        {
            if (FilterTags.Count == 0) {
                foreach (TagPageSet tag in Tags.Values) {
                    tag.ClearFilter();
                }
            }  else {
                foreach (TagPageSet tag in Tags.Values) {
                    tag.IntersectWith(MatchingPages.Values);
                }
            }
        }
    }
}