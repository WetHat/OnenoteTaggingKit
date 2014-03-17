using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// Observable collections of tags and OneNote pages satisfying a search criterion.
    /// </summary>
    /// <remarks>
    /// Provides a refineable unordered set of tags and pages. The page collection is
    /// built by calling <see cref="Find"/> and can be progressively refined (filtered)
    /// by adding filter tags (<see cref="AddTagToFilter"/>)
    /// </remarks>
    public class FilterablePageCollection : TagCollection
    {
        private ISet<TagPageSet> _filterTags = new HashSet<TagPageSet>();
 
        /// <summary>
        /// Set of pages after tag filters have been applied.
        /// </summary>
        private ObservableDictionary<string, TaggedPage> _filteredPages = new ObservableDictionary<string, TaggedPage>();

        internal FilterablePageCollection(Application onenote, XMLSchema schema) : base (onenote,schema)
        {
        }

        /// <summary>
        /// Find OneNote pages.
        /// </summary>
        /// <param name="query">query string. if null or empty just the tags are provided</param>
        /// <param name="scopeID">OneNote id of the scope to search for pages. This is the element ID of a notebook, section group, or section.
        ///                       If given as null or empty string scope is the entire set of notebooks open in OneNote.
        /// </param>
        internal void Find(string query, string scopeID)
        {
            string strXml;
            if (string.IsNullOrEmpty(query))
            {
                // collect all tags used somewhere on a page
                Find(scopeID);
            }
            else
            {
                // run a text search
                _onenote.FindPages(scopeID, query, out strXml,false,false,_schema);
                _filteredPages.Clear();
                parseOneNoteFindResult(strXml);
            }
            ClearTagFilter();
        }

        /// <summary>
        /// Get the dictionary of pages
        /// </summary>
        internal ObservableDictionary<string, TaggedPage> FilteredPages
        {
            get { return _filteredPages; }
        }

        /// <summary>
        /// Undo all tag filters
        /// </summary>
        internal void ClearTagFilter()
        {
            _filterTags.Clear();
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
        /// <param name="tag">tag to filter on</param>
        internal void AddTagToFilter(TagPageSet tag)
        {
            if (_filterTags.Add(tag))
            {
                // remove pages which are not in this tag's page set
                _filteredPages.IntersectWith(tag.Pages);
                ApplyFilterToTags();
            }
        }

        /// <summary>
        /// Remove tag from the filter
        /// </summary>
        /// <param name="tag">tag to remove</param>
        internal void RemoveTagFromFilter(TagPageSet tag)
        {
            if (_filterTags.Remove(tag))
            {
                if (_filterTags.Count == 0)
                {
                    ClearTagFilter();
                }
                else
                {
                    // recompute filtered pages from scratch
                    _filteredPages.UnionWith(Pages.Values);

                    foreach ( TagPageSet tps in _filterTags)
                    {
                        tps.ClearFilter();
                        _filteredPages.IntersectWith(tps.Pages);
                    }
                    ApplyFilterToTags();
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