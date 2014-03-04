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

namespace WetHatLab.OneNote.TaggingKit.collections
{
    /// <summary>
    /// Observable collections of tags and OneNote pages satisfying a search criterion.
    /// </summary>
    /// <remarks>
    /// Provides a refineable unordered set of tags and pages. The page collection is
    /// built by calling <see cref="Find"/> and can be progressively refined (filtered)
    /// by adding filter tags (<see cref="ApplyTagFilter"/>)
    /// </remarks>
    public class FilterablePageCollection : IDisposable
    {
        private Application _onenote;

        XMLSchema _schema;

        private IDictionary<string, TagPageSet> _tags = new Dictionary<string, TagPageSet>();

        /// <summary>
        /// Set of Pages returned from a full text search.
        /// </summary>
        /// <remarks>May be null if no full text query was used to retrieve tags</remarks>
        private HashSet<TaggedPage> _searchResult;

        private ObservableSortedList<TagPageSet> _filteredTags  = new ObservableSortedList<TagPageSet>();
        private ObservableSortedList<TaggedPage> _filteredPages = new ObservableSortedList<TaggedPage>();

        private HashSet<TagPageSet> _filterTags = new HashSet<TagPageSet>();

        internal FilterablePageCollection(Application onenote, XMLSchema schema)
        {
            _onenote = onenote;
            _schema = schema;
        }

        internal event NotifyCollectionChangedEventHandler TagCollectionChanged
        {
            add { _filteredTags.CollectionChanged += value; }
            remove { _filteredTags.CollectionChanged -= value;  }
        }

        internal event NotifyCollectionChangedEventHandler PageCollectionChanged
        {
            add { _filteredPages.CollectionChanged += value; }
            remove { _filteredPages.CollectionChanged -= value; }
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
            _tags.Clear();
            _filterTags.Clear();
            _filteredTags.Clear();
            _filteredPages.Clear();
            
            string strXml;
            if (string.IsNullOrEmpty(query))
            {
                // collect all tags used somewhere on a page
                _onenote.FindMeta(scopeID, OneNotePageProxy.META_NAME, out strXml,false,_schema);
                _searchResult = null;
            }
            else
            {
                // run a text search
                _onenote.FindPages(scopeID, query, out strXml,false,false,_schema);
                _searchResult = new HashSet<TaggedPage>();
            }

            _tags = buildTagAndPagesList(query, strXml,_tags,_searchResult);

            _filteredTags.AddAll(_tags.Values);
            if (_searchResult != null)
            {
                // announce the search result
                _filteredPages.AddAll(_searchResult);
            }
            else
            {  // attempt to automatically update the tag suggestion list, if we have collected all used tags
                HashSet<string> knownTags = new HashSet<String>(OneNotePageProxy.ParseTags(Properties.Settings.Default.KnownTags));
                int countBefore = knownTags.Count;

                // add tags from search result
                foreach (string t in _tags.Keys)
                {
                    knownTags.Add(t);
                }

                if (countBefore != knownTags.Count)
                { // updated tag suggestions
                    string[] sortedTags = knownTags.ToArray();
                    Array.Sort(sortedTags);
                    Properties.Settings.Default.KnownTags = string.Join(",", sortedTags);
                }
            }
        }

        internal static IDictionary<string, TagPageSet> buildTagAndPagesList(string query, string strXml, IDictionary<string, TagPageSet> tags, HashSet<TaggedPage> searchResult)
        {
            // process result
            try
            {
                XDocument result = XDocument.Parse(strXml);
                XNamespace one = result.Root.GetNamespaceOfPrefix("one");

                foreach (XElement page in result.Descendants(one.GetName("Page")))
                {
                    TaggedPage tp = new TaggedPage(page, query);
                    foreach (string tag in tp.Tags)
                    {
                        TagPageSet t;
                        if (!tags.TryGetValue(tag, out t))
                        {
                            t = new TagPageSet(tag);
                            tags.Add(tag, t);
                        }
                        t.AddPage(tp);
                    }
                    if (searchResult != null)
                    {
                        searchResult.Add(tp);
                    }
                }
            }
            catch (Exception)
            {
                // unable to parse tags
            }
           return tags;
        }

        /// <summary>
        /// get dictionary of tags.
        /// </summary>
        internal IDictionary<string, TagPageSet> Tags
        {
            get { return _tags;}
        }

        /// <summary>
        /// Undo all tag filters
        /// </summary>
        internal void ClearTagFilter()
        {
            if (_filterTags.Count > 0)
            {
                _filterTags.Clear();
                if (_searchResult != null)
                {
                    // reset filtered pages to search result
                    _filteredPages.AddAll(_searchResult);
                }
                else
                {
                    _filteredPages.Clear();
                }
                ApplyFilterToTags();
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
        internal void ApplyTagFilter(TagPageSet tag)
        {
            if (_searchResult == null && _filterTags.Count == 0)
            {
#if DEBUG
                Debug.Assert(_filteredPages.Count == 0, "Collection of filtered pages expected to be empty");
#endif
                _filterTags.Add(tag);
                _filteredPages.AddAll(tag.Pages);
                ApplyFilterToTags();
            }
            else if (_filterTags.Add(tag))
            {
                // remove pages which are not in this tag's page set
                ISet<TaggedPage> pagesInTag = tag.Pages;
                _filteredPages.RemoveAll(from tp in _filteredPages.Values where !pagesInTag.Contains(tp) select tp);
                ApplyFilterToTags();
            }
        }

        /// <summary>
        /// Remove tag from the filter
        /// </summary>
        /// <param name="tag">tag to remove</param>
        internal void UnapplyTagFilter(TagPageSet tag)
        {
            if (_filterTags.Remove(tag))
            {
                if (_filterTags.Count == 0)
                {
                    if (_searchResult == null)
                    {
                        _filteredPages.Clear();
                    }
                    else
                    {
                        // reset filtered pages to search result
                        _filteredPages.AddAll(_searchResult);
                    }
                }
                else
                {
                    // recompute filtered pages locally
                    HashSet<TaggedPage> filteredPages = _searchResult != null ? new HashSet<TaggedPage>(_searchResult) : null;
                    foreach ( TagPageSet tps in _filterTags)
                    {
                        tps.IntersectWith(null);
                        if (filteredPages != null)
                        {
                            filteredPages.IntersectWith(tps.Pages);
                        }
                        else
                        {
                            filteredPages= new HashSet<TaggedPage>(tps.Pages);
                        }
                    }

                    // Since we removed a filter the just need to add missing pages
                    _filteredPages.AddAll(filteredPages);
                }

                ApplyFilterToTags();
            }
        }

        /// <summary>
        /// Apply the current page filter add tags with a page count > 0 and remove tags with page count == 0
        /// form the set of tags
        /// </summary>
        private void ApplyFilterToTags()
        {      
            var pages = _searchResult == null && _filterTags.Count == 0 ? null:_filteredPages.Values;

            LinkedList<TagPageSet> toAdd = new LinkedList<TagPageSet>();
            LinkedList<TagPageSet> toRemove = new LinkedList<TagPageSet>();
            foreach (TagPageSet tag in _tags.Values)
            {
                tag.IntersectWith(pages);
                if (tag.Pages.Count > 0)
                {
                    toAdd.AddLast(tag);
                }
                else
                {
                    toRemove.AddLast(tag);
                }
            }

            _filteredTags.RemoveAll(toRemove);
            _filteredTags.AddAll(toAdd);
        }

        #region IDisposable
        public void Dispose()
        {
            _filteredPages.Dispose();
            _filteredTags.Dispose();
        }
        #endregion IDisposable

    }
}
