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

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// A observable, sorted collection of items having sortable keys.
    /// </summary>
    /// <remarks>
    /// Instances of this class provide change notification through <see cref="INotifyCollectionChanged"/>. This
    /// class is optimized for batch updates (item collections). Single items cannot be added. Batch updates are
    /// usefull to optimize UI updates by allowing update of larger chunks of data, rather than individual items
    /// </remarks>
    /// <typeparam name="Tvalue">item type providing sortable keys</typeparam>
    class ObservableSortedList<Tvalue> : INotifyCollectionChanged, IDisposable where Tvalue : IKeyedItem
    {
        /// <summary>
        /// Event to notify about changes to this collection.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private SortedList<string,Tvalue> _sortedList = new SortedList<string,Tvalue>();

        /// <summary>
        /// Get the number of items in the collection.
        /// </summary>
        internal int Count
        {
            get { return _sortedList.Count;  }
        }

        /// <summary>
        /// Get all items in the collection.
        /// </summary>
        internal IList<Tvalue> Values
        {
            get { return _sortedList.Values; }
        }

        /// <summary>
        /// Clear all items from the collection.
        /// </summary>
        /// <remarks>
        /// Notifies all listeners about the change
        /// </remarks>
        internal void Clear()
        {
            _sortedList.Clear();
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            CollectionChanged(this, args);
        }

        /// <summary>
        /// Inform listeners to change notifications about removal of a contiguous range of items.
        /// </summary>
        /// <remarks>
        /// This method in addition also removes the items from the sorted collection. Hence it expects
        /// all given items to be still present in the sorted collection
        /// </remarks>
        /// <param name="batch">items to remove</param>
        /// <param name="startindex">start index of contiguous range of items</param>
        /// <returns>true, if batch was non empty</returns>
        private bool processRemoveBatch(LinkedList<Tvalue> batch, int startindex)
        {
            if (batch.Count > 0)
            {
                // remove this batch from the sorted list
                foreach (Tvalue dead in batch)
                {
                    bool removed =_sortedList.Remove(dead.Key);
#if DEBUG
                    Debug.Assert(removed, string.Format("Item with key '{0}' could not be removed!",dead.Key));
#endif
                }

                // fire event for this batch
                NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                                                                                             batch.ToArray(),
                                                                                             startindex);
                if (CollectionChanged != null)
                {
                    CollectionChanged(this, args);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove items from the collection in batches. 
        /// </summary>
        /// <remarks>
        /// Groups the given items into contiguous ranges of batches and removes
        /// each batch at once, firing one change notification per batch.
        /// </remarks>
        /// <param name="items">items to remove</param>
        internal void RemoveAll(IEnumerable<Tvalue> items)
        {
            SortedList<int, Tvalue> toDelete = new SortedList<int, Tvalue>();
            foreach (Tvalue item in items)
            {
                int index = _sortedList.IndexOfKey(item.Key);
                if (index >= 0)
                {
                    toDelete.Add(index, item);
                }
            }

            // fire event in batches
            int n = 0;
            int batchStartIndex = -1;
            int batchLastIndex = -2;
            LinkedList<Tvalue> batch = new LinkedList<Tvalue>();
            foreach (KeyValuePair<int,Tvalue> item in toDelete)
            {
                if (item.Key > batchLastIndex +1)
                {   // finish current batch
                    if (processRemoveBatch(batch,batchStartIndex - n))
                    {
                        n += batch.Count;
                        batch.Clear();
                    }

                    // ... and start new batch with this item
                    batchStartIndex = item.Key;
                    batchLastIndex = batchStartIndex - 1;
                }
#if DEBUG
                Debug.Assert(item.Key == batchLastIndex + 1);
#endif
                batchLastIndex = item.Key;
                batch.AddLast(item.Value);
            }

            // fire event for last batch
            processRemoveBatch(batch, batchStartIndex - n);
        }

        /// <summary>
        /// Inform listeners to change notifications about addition of a contiguous range of items.
        /// </summary>
        /// <remarks>
        /// It is assumed that all items in the provided batch are already present in the sorted collection.
        /// </remarks>
        /// <param name="batch">collection of items to add</param>
        /// <param name="startindex">start index of the contiguous range of items</param>
        /// <returns>true, if items were added</returns>
        private bool processAddBatch(LinkedList<Tvalue> batch, int startindex)
        {
#if DEBUG
            foreach (Tvalue itm in batch)
            {
                Debug.Assert(_sortedList.ContainsKey(itm.Key),string.Format("Item '{0}' not found in the collection!",itm.Key));
            }
#endif
            if (batch.Count > 0)
            {
                // fire event for this batch
                NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                                                                                             batch.ToArray(),
                                                                                             startindex);
                if (CollectionChanged != null)
                {
                    CollectionChanged(this, args);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add items to the sorted collection in batches.
        /// </summary>
        /// <remarks>
        /// Groups the given items into contiguous ranges of batches and adds
        /// each batch at once, firing one change notification per batch.
        /// </remarks>
        /// <param name="items">items to add</param>
        internal void AddAll(IEnumerable<Tvalue> items)
        {
            LinkedList<Tvalue> addedItems = new LinkedList<Tvalue>();
            foreach (Tvalue item in items)
            {
                if (!_sortedList.ContainsKey(item.Key))
                {
                    addedItems.AddLast(item);
                    _sortedList.Add(item.Key, item);
                }
            }

            // build a sorted list of added items
            SortedList<int, Tvalue> sortedAdds = new SortedList<int, Tvalue>();
            foreach (Tvalue a in addedItems)
            {
                int index = _sortedList.IndexOfKey(a.Key);
#if DEBUG
                Debug.Assert(index >= 0, string.Format("previously added item not found: {0}",a.Key));
#endif
                sortedAdds.Add(index, a);
            }

            // fire event in batches
            int batchStartIndex = -1;
            int batchLastIndex = -2;
            addedItems.Clear();

            foreach (KeyValuePair<int, Tvalue> item in sortedAdds)
            {
                if (item.Key > batchLastIndex + 1)
                {
                    // process current batch
                    if (processAddBatch(addedItems, batchStartIndex))
                    {
                        addedItems.Clear();
                    }
                    // ... and start a new batch with this item
                    batchStartIndex = item.Key;
                    batchLastIndex = batchStartIndex-1;
                }
#if DEBUG
                Debug.Assert(item.Key == batchLastIndex + 1);
#endif
                batchLastIndex = item.Key;
                addedItems.AddLast(item.Value);
            }

            // fire event for last batch
            processAddBatch(addedItems, batchStartIndex);
        }
        #region IDisposable
        public void Dispose()
        {
            CollectionChanged = null;
        }
        #endregion

    }
    /// <summary>
    /// Observable collections of tags and OneNote pages satisfying a search criterion.
    /// </summary>
    /// <remarks>
    /// Provides a refineable set of tags and pages. The page collection is
    /// built by calling <see cref="Find"/> and can be progressively refined (filtered)
    /// by adding filter tags (<see cref="ApplyTagFilter"/>)
    /// </remarks>
    public class FilterablePageCollection : IDisposable
    {
        private Application _onenote;

        private Dictionary<string, TagPageSet> _tags = new Dictionary<string, TagPageSet>();
        private HashSet<TaggedPage> _searchResult;

        private ObservableSortedList<TagPageSet> _filteredTags  = new ObservableSortedList<TagPageSet>();
        private ObservableSortedList<TaggedPage> _filteredPages = new ObservableSortedList<TaggedPage>();

        private HashSet<TagPageSet> _filterTags = new HashSet<TagPageSet>();

        internal FilterablePageCollection(Application onenote)
        {
            _onenote = onenote;
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
        /// <param name="scopeID">id if the scope to search for pages. This is the element ID of a notebook, section group, or section.
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
                _onenote.FindMeta(scopeID, OneNotePageProxy.META_NAME, out strXml);
                _searchResult = null;
            }
            else
            {
                // run a text search
                _onenote.FindPages(scopeID, query, out strXml);
                _searchResult = new HashSet<TaggedPage>();
            }

            // process result
            XDocument result = XDocument.Parse(strXml);
            XNamespace one = result.Root.GetNamespaceOfPrefix("one");
            
            foreach (XElement page in result.Descendants(one.GetName("Page")))
            {
                TaggedPage tp = new TaggedPage(page,query);
                foreach (string tag in tp.Tags)
                {
                    TagPageSet t;
                    if (!_tags.TryGetValue(tag, out t))
                    {
                        t = new TagPageSet(tag);
                        _tags.Add(tag, t);
                    }
                    t.AddPage(tp);   
                }
                if (_searchResult != null)
                {
                    _searchResult.Add(tp);
                }
            }

            _filteredTags.AddAll(_tags.Values);
            if (_searchResult != null)
            {
                // announce the search result
                _filteredPages.AddAll(_searchResult);
            }
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

        #region
        public void Dispose()
        {
            _filteredPages.Dispose();
            _filteredTags.Dispose();
        }
        #endregion

    }
}
