// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// An observable, sorted collection of items having sortable keys.
    /// </summary>
    /// <remarks>
    /// Instances of this class provide change notification through
    /// <see cref="INotifyCollectionChanged" /> and can take part in data binding to UI
    /// controls. This class is optimized for batch updates (item collections). Single
    /// items cannot be added.
    /// </remarks>
    /// <typeparam name="TValue">item type providing sortable keys</typeparam>
    /// <typeparam name="TKey">unique key type</typeparam>
    /// <typeparam name="TSort">sort key type. Sort keys are not required to be unique</typeparam>
    public class ObservableSortedList<TSort, TKey, TValue> : INotifyCollectionChanged, IEnumerable<TValue>
        where TValue : ISortableKeyedItem<TSort, TKey>
        where TKey : IEquatable<TKey>, IComparable<TKey>
        where TSort : IComparable<TSort>
    {
        /// <summary>
        /// Event to notify about changes to this collection.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private const int INITIAL_CAPACITY = 200;
        private List<KeyValuePair<TSort, TValue>> _sortedList = new List<KeyValuePair<TSort, TValue>>(INITIAL_CAPACITY);
        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>(INITIAL_CAPACITY);

        #region Comparers
        private class SimpleComparer<TSort, TValue> : IComparer<KeyValuePair<TSort, TValue>>
                                                      where TSort : IComparable<TSort>
        {
            #region IComparer<KeyValuePair<TSort, TValue>>

            /// <summary>
            /// Compare instances of <see cref="KeyValuePair{TSort, TValue}"/> using
            /// the `Key` property.
            /// </summary>
            /// <param name="x">First object</param>
            /// <param name="y">Second object</param>
            /// <returns>Comparison result -1,0,1 if x $lt; y; x == y; x &gt; y</returns>
            public int Compare(KeyValuePair<TSort, TValue> x, KeyValuePair<TSort, TValue> y) {
                return x.Key.CompareTo(y.Key);
            }

            #endregion IComparer<KeyValuePair<TSort, TValue>>
        };

        private class Comparer<TSort, TValue> : IComparer<KeyValuePair<TSort, TValue>>
                                                where TSort : IComparable<TSort>
                                                where TValue : ISortableKeyedItem<TSort, TKey>
        {
            #region IComparer<KeyValuePair<TSort, TValue>>

            /// <summary>
            /// Compare instances of <see cref="KeyValuePair{TSort, TValue}"/> using
            /// the `Key` property and the key of the value.
            /// </summary>
            /// <param name="x">First object</param>
            /// <param name="y">Second object</param>
            /// <returns>Comparison result -1,0,1 if x $lt; y; x == y; x &gt; y</returns>
            public int Compare(KeyValuePair<TSort, TValue> x, KeyValuePair<TSort, TValue> y)
            {
                var result = x.Key.CompareTo(y.Key);
                return result == 0 ? x.Value.Key.CompareTo(y.Value.Key) : result;
            }

            #endregion IComparer<KeyValuePair<TSort, TValue>>
        };
        #endregion Comparers
        /// <summary>
        /// The default comparer which sorts the data by name.
        /// </summary>
        public static readonly IComparer<KeyValuePair<TSort, TValue>> DefaultComparer = new Comparer<TSort, TValue>();
        static readonly IComparer<KeyValuePair<TSort, TValue>> sSimpleComparer = new SimpleComparer<TSort, TValue>();
        static readonly IComparer<KeyValuePair<int, TValue>> sIndexComparer = new SimpleComparer<int,TValue>();

        /// <summary>
        /// Get the number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return _sortedList.Count; }
        }

        IComparer<KeyValuePair<TSort, TValue>> _comparer = DefaultComparer;
        /// <summary>
        /// Set a comparer to determine the sort order of the list.
        /// </summary>
        /// <remarks>
        /// Setting a comparer triggers a sort of the data in the list.
        /// The default comparer is available in the static property
        /// <see cref="DefaultComparer"/>.
        /// </remarks>
        public IComparer<KeyValuePair<TSort, TValue>> ItemComparer {
            private get { return _comparer; }
            set {
                if (_comparer != value) {
                    // save the current content
                    List < TValue> saved = new List<TValue>(from it in _sortedList select it.Value);
                    Clear();
                    _comparer = value;
                    // re-add the saved content with a new comparer
                    AddAll(saved);
                }
            }
        }

        /// <summary>
        /// Get all items in the collection.
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get { return from e in _sortedList select e.Value; }
        }

        /// <summary>
        /// Determine if the list contains an item with a given key
        /// </summary>
        /// <param name="key">key to check</param>
        /// <returns>true if the given item is contained in the list; false otherwise</returns>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Determine if the list contains at least one item with a given sort key.
        /// </summary>
        /// <remarks>Sort keys are not unique. The list may contain more than
        /// one item with the same sort key.</remarks>
        /// <param name="sortkey">the sorting key to check</param>
        /// <returns>true if the given item is contained in the list; false otherwise</returns>
        public bool ContainsSortKey(TSort sortkey) {
           return _sortedList.BinarySearch(new KeyValuePair<TSort, TValue>(sortkey,default(TValue)), sSimpleComparer) >=0 ;
        }

        /// <summary>
        /// Try to retrieve a value from the list with a given key
        /// </summary>
        /// <param name="key">key of the item</param>
        /// <param name="value">found vale or null</param>
        /// <returns>true if a value was found for the key provided</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        private int IndexOfKey(TKey key)
        {
            int index = -1;
            TValue found;
            if (_dictionary.TryGetValue(key, out found)) {
                // lookup the index in the sorted list
                index = _sortedList.BinarySearch(new KeyValuePair<TSort, TValue>(found.SortKey, found), ItemComparer);
            }

            return index;
        }

        /// <summary>
        /// Clear all items from the collection.
        /// </summary>
        /// <remarks>Notifies all listeners about the change</remarks>
        public void Clear()
        {
            _sortedList.Clear();
            _dictionary.Clear();
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            if (CollectionChanged != null)
            {
                CollectionChanged(this, args);
            }
        }

        /// <summary>
        /// Remove items from the collection in batches.
        /// </summary>
        /// <remarks>
        /// Groups the given items into contiguous ranges of batches and removes each batch
        /// at once, firing one change notification per batch.
        /// </remarks>
        /// <param name="keys">items to remove</param>
        internal void RemoveAll(IEnumerable<TKey> keys)
        {
            List<KeyValuePair<int, TValue>> toDelete = new List<KeyValuePair<int, TValue>>();
            foreach (TKey key in keys)
            {
                int index = IndexOfKey(key);
                if (index >= 0)
                {
                    toDelete.Add(new KeyValuePair<int, TValue>(index, _dictionary[key]));
                }
            }
            toDelete.Sort(sIndexComparer);

            while (toDelete.Count > 0)
            {
                LinkedList<KeyValuePair<int, TValue>> batch = new LinkedList<KeyValuePair<int, TValue>>();
                int lastElementIndex = toDelete.Count - 1;
                batch.AddFirst(toDelete[lastElementIndex]);
                toDelete.RemoveAt(lastElementIndex);
                int n = 1;
                // keep adding elements with contiguous indices
                lastElementIndex = toDelete.Count - 1;
                while (lastElementIndex >= 0 && toDelete[lastElementIndex].Key == (batch.First.Value.Key - 1))
                {
                    batch.AddFirst(toDelete[lastElementIndex]);
                    toDelete.RemoveAt(lastElementIndex);
                    lastElementIndex = toDelete.Count - 1;
                    n++;
                }

                int startindex = batch.First.Value.Key;

                List<TValue> olditems = new List<TValue>(n);
                foreach (var dead in batch)
                {
                    olditems.Add(dead.Value);
                    bool removed = _dictionary.Remove(dead.Value.Key);
#if DEBUG
                    Debug.Assert(removed, string.Format("Failed to remove item with key {0}", dead.Value.Key));
#endif
                }

                // remove this batch from the list
                _sortedList.RemoveRange(startindex, n);

                // fire the event to inform listeners
                if (CollectionChanged != null)
                {
                    // fire event for this batch
                    NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove,
                                                                                                 olditems,
                                                                                                 startindex);
                    CollectionChanged(this, args);
                }
            }
        }

        /// <summary>
        /// Add items to the sorted collection in batches.
        /// </summary>
        /// <remarks>
        /// Groups the given items into contiguous ranges of batches and adds each batch at
        /// once, firing one change notification per batch.
        /// </remarks>
        /// <param name="items">items to add</param>
        internal void AddAll(IEnumerable<TValue> items)
        {
            List<KeyValuePair<int, TValue>> toAdd = new List<KeyValuePair<int, TValue>>();
            foreach (TValue item in items)
            {
                if (!_dictionary.ContainsKey(item.Key))
                {
                    // lookup insertion point
                    int insertionPoint = _sortedList.BinarySearch(new KeyValuePair<TSort, TValue>(item.SortKey, item), ItemComparer);

                    if (insertionPoint < 0)
                    {
                        _dictionary.Add(item.Key, item);
                        toAdd.Add(new KeyValuePair<int, TValue>(~insertionPoint, item));
                    }
                    else
                    {
                        TraceLogger.Log(TraceCategory.Error(), "List is inconsistency! Attempting to recover");
                        TraceLogger.Flush();
                        _dictionary.Add(item.Key, item);
                    }
                }
            }
            toAdd.Sort(sIndexComparer);

            // process the sorted list of items to add in reverse order so that we do not
            // have to correct indices

            while (toAdd.Count > 0)
            {
                List<KeyValuePair<TSort, TValue>> batch = new List<KeyValuePair<TSort, TValue>>();

                int lastItemIndex = toAdd.Count - 1;
                KeyValuePair<int, TValue> itemToAdd = toAdd[lastItemIndex];

                // add the first item to the batch
                int insertionPoint = itemToAdd.Key;
                batch.Add(new KeyValuePair<TSort, TValue>(itemToAdd.Value.SortKey, itemToAdd.Value));

                toAdd.RemoveAt(lastItemIndex);
                lastItemIndex = toAdd.Count - 1;

                while (lastItemIndex >= 0 && toAdd[lastItemIndex].Key == insertionPoint)
                {
                    itemToAdd = toAdd[lastItemIndex];
                    batch.Add(new KeyValuePair<TSort, TValue>(itemToAdd.Value.SortKey, itemToAdd.Value));

                    toAdd.RemoveAt(lastItemIndex);
                    lastItemIndex = toAdd.Count - 1;
                }

                batch.Sort(ItemComparer);

                _sortedList.InsertRange(insertionPoint, batch);

                if (CollectionChanged != null)
                {
                    NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                                                                                          (from b in batch select b.Value).ToList(),
                                                                                          insertionPoint);

                    CollectionChanged(this, args);
                }
            }
        }

        #region IEnumerable<TValue>

        /// <summary>
        /// Get an enumerator for items in the list
        /// </summary>
        /// <returns>item enumerator</returns>
        public IEnumerator<TValue> GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        /// <summary>
        /// Get a generic enumerator of items in the list
        /// </summary>
        /// <returns>item enumerator</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Values.GetEnumerator();
        }

        #endregion IEnumerable<TValue>
    }
}