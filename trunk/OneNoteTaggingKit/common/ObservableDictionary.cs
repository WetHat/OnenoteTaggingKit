using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace WetHatLab.OneNote.TaggingKit.common
{

    /// <summary>
    /// Classification of changes to a <see cref="ObservableDictionary&lt;TKey,TValue&gt;"/> instance.
    /// </summary>
    public enum NotifyDictionaryChangedAction
    {
        /// <summary>
        /// items were added to the dictionary
        /// </summary>
        Add,
        /// <summary>
        /// items were removed from the dictionary
        /// </summary>
        Remove,
        /// <summary>
        /// dictionary was reset.
        /// </summary>
        Reset
    }

    /// <summary>
    /// Event details describing to details of a changes to instances of <see cref="ObservableDictionary&lt;TKey,TValue&gt;"/>
    /// </summary>
    /// <typeparam name="TKey">dictionary key type</typeparam>
    /// <typeparam name="TValue">dictionary value type</typeparam>
    public class NotifyDictionaryChangedEventArgs<TKey,TValue> : EventArgs where TValue : IKeyedItem<TKey>
                                                                           where TKey   : IEquatable<TKey>
    {
        /// <summary>
        /// Get the nature of the change to the <see cref="ObservableDictionary&lt;TKey,TValue&gt;"/> instance.
        /// </summary>
        public NotifyDictionaryChangedAction Action { get; private set; }
        
        /// <summary>
        /// Get the items involved in the change to the <see cref="ObservableDictionary&lt;TKey,TValue&gt;"/> instance
        /// </summary>
        public IEnumerable<TValue> Items { get; private set;}

        /// <summary>
        /// Create an instance describing a <see cref="NotifyDictionaryChangedAction.Reset"/> action.
        /// </summary>
        internal NotifyDictionaryChangedEventArgs()
        {
            Action = NotifyDictionaryChangedAction.Reset;
        }
        internal NotifyDictionaryChangedEventArgs(TValue item, NotifyDictionaryChangedAction action)
        {
            Items = new TValue[] { item };
            Action = action;
        }
        internal NotifyDictionaryChangedEventArgs(IEnumerable<TValue> items, NotifyDictionaryChangedAction action)
        {
            Items = items;
            Action = action;
        }
    }

    /// <summary>
    /// delegate to handle change events in instances of <see cref="ObservableDictionary&lt;TKey,TValue&gt;"/>
    /// </summary>
    /// <typeparam name="TKey">dictionary key type</typeparam>
    /// <typeparam name="TValue">dictionary value type</typeparam>
    /// <param name="sender">dictionary which sent and event to post changes</param>
    /// <param name="e">event details</param>
    public delegate void NotifyDictionaryChangedEventHandler<TKey, TValue>(object sender, NotifyDictionaryChangedEventArgs<TKey, TValue> e)
        where TValue : IKeyedItem<TKey>
        where TKey : IEquatable<TKey>;

    /// <summary>
    /// A dictionary which informs subscribed listerners about changes to its contents.
    /// </summary>
    /// <typeparam name="TKey">type of the key used in the dictionary</typeparam>
    /// <typeparam name="TValue">type of objects stored in the dictionary</typeparam>
    /// <remarks>This class is <b>not</b> thread save.
    /// </remarks>
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TValue : IKeyedItem<TKey>
        where TKey : IEquatable<TKey>
    {
        private IDictionary<TKey, TValue> _dictionary = new Dictionary<TKey,TValue>();

        /// <summary>
        /// Event to inform subscribers about changes to the dictionary.
        /// </summary>
        public event NotifyDictionaryChangedEventHandler<TKey,TValue> CollectionChanged;

        private void fireChangedEvent(NotifyDictionaryChangedEventArgs<TKey,TValue> e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }

        /// <summary>
        /// Perform set union of the given items with the items already in the dictionary
        /// </summary>
        /// <param name="items">items to unite with the items in this dictionary</param>
        public void UnionWith(IEnumerable<TValue> items)
        {
            LinkedList<TValue> added = new LinkedList<TValue>();
            foreach (TValue item in items)
            {
                if (!_dictionary.ContainsKey(item.Key))
                {
                    added.AddLast(item);
                    _dictionary.Add(item.Key, item);
                }
            }
            if (added.First != null)
            {
                fireChangedEvent(new NotifyDictionaryChangedEventArgs<TKey,TValue>(added,NotifyDictionaryChangedAction.Add));
            }
        }

        /// <summary>
        /// Perform a set subtract. The given items are removed from the set of items in the dictionary.
        /// </summary>
        /// <param name="items">items to remove</param>
        public void ExceptWith(IEnumerable<TValue> items)
        {
            LinkedList<TValue> removed = new LinkedList<TValue>();
            foreach (TValue item in items)
            {
                if (_dictionary.ContainsKey(item.Key))
                {
                    removed.AddLast(item);
                }
            }

            if (removed.First != null)
            {
                foreach (TValue item in removed)
                {
                    _dictionary.Remove(item.Key);
                }
                fireChangedEvent(new NotifyDictionaryChangedEventArgs<TKey, TValue>(removed, NotifyDictionaryChangedAction.Remove));
            }
        }

        /// <summary>
        /// Perform a set intersect. Only items present in both the dictionary <b>and</b> the argument remain
        /// in the dictionary.
        /// </summary>
        /// <param name="items">items to intersect with</param>
        public void IntersectWith(IEnumerable<TValue> items)
        {
            LinkedList<TValue> removed = new LinkedList<TValue>();

            HashSet<TValue> itemSet = new HashSet<TValue>(items);

            foreach (TValue item in _dictionary.Values)
            {
                if (!itemSet.Contains(item))
                {
                    removed.AddLast(item);
                }
            }

            if (removed.First != null)
            {
                foreach (TValue item in removed)
                {
                    _dictionary.Remove(item.Key);
                }
                fireChangedEvent(new NotifyDictionaryChangedEventArgs<TKey,TValue>(removed,NotifyDictionaryChangedAction.Remove));
            }
        }

        /// <summary>
        /// Add an item to the dictionary.
        /// </summary>
        /// <param name="value">item to add</param>
        /// <returns>true, if the item was actually added;
        /// false, if the item was already present in the dictionary</returns>
        private bool Add(TValue value)
        {
            TValue found;
            bool added = false;
            if (!_dictionary.TryGetValue(value.Key, out found))
            {
                _dictionary.Add(value.Key, value);
                added = true;
                fireChangedEvent(new NotifyDictionaryChangedEventArgs<TKey, TValue>(value, NotifyDictionaryChangedAction.Add));
            }
            return added;
        }

        #region IDictionary<TKey,TValue>

        /// <summary>
        /// Check of the dictionary contains an item with a given key
        /// </summary>
        /// <param name="key">item key</param>
        /// <returns>true if an item with the given key is contained in the dictionary; false otherwise</returns>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Get the collection of items contained in the dictionary.
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                return _dictionary.Values;
            }
        }

        /// <summary>
        /// Get the collection of keys of the items in the dictionary
        /// </summary>
        public ICollection<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        /// <summary>
        /// Remove an item with a given key.
        /// </summary>
        /// <param name="key">key of item to remove</param>
        /// <returns>true, if the item was found and removed; false otherwise</returns>
        public bool Remove(TKey key)
        {
            TValue found;

            bool removed;
            if (removed = _dictionary.TryGetValue(key, out found))
            {
                _dictionary.Remove(key);
                fireChangedEvent(new NotifyDictionaryChangedEventArgs<TKey, TValue>(found, NotifyDictionaryChangedAction.Remove));
            }
            return removed;
        }

        /// <summary>
        /// Remove all items from the dictionary.
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
            fireChangedEvent(new NotifyDictionaryChangedEventArgs<TKey, TValue>());
        }

        /// <summary>
        /// Get the number of items in the dictionary.
        /// </summary>
        public int Count
        {
            get { return _dictionary.Count; }
        }

        /// <summary>
        /// get an enuerator for the entries in the dictionary.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <summary>
        /// Add an item to the dictionary, if not already present.
        /// </summary>
        /// <param name="key">item key</param>
        /// <param name="value">item to add</param>
        public void Add(TKey key, TValue value)
        {
            Add(value);
        }

        /// <summary>
        /// Try to find an item in the dictionary.
        /// </summary>
        /// <param name="key">key of item to find</param>
        /// <param name="value">item found, or null if no item with the given key is contained in the dictionary</param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// Get or add an item to the dictionary
        /// </summary>
        /// <param name="key">item key</param>
        /// <returns>item</returns>
        public TValue this[TKey key]
        {
            get
            {
                return _dictionary[key];
            }
            set
            {
                Add(value);
            }
        }

        /// <summary>
        /// Add a new entry to the dictionary
        /// </summary>
        /// <param name="item">dictionary entry</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item);
        }

        /// <summary>
        /// Determine if the dictionary contains a prticular entry
        /// </summary>
        /// <param name="item">entry to check</param>
        /// <returns>true, if given item is contained in the dictionary; false otherwise</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        /// <summary>
        /// Copy all items in the dictionary to an array
        /// </summary>
        /// <param name="array">array to copy the items from the dictioary into</param>
        /// <param name="arrayIndex">start index in the array for the copy</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Determine if this instance of the dictionary is mutable.
        /// </summary>
        /// <remarks>Observable dictionaries are always mutable</remarks>
        /// <value>true always</value>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Remove an entry from the dictionary
        /// </summary>
        /// <param name="item">item to remove</param>
        /// <returns>true, if the item was found and successfully removed; false otherwise</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool removed = _dictionary.Remove(item);
            if (removed)
            {
                fireChangedEvent(new NotifyDictionaryChangedEventArgs<TKey,TValue>(item.Value,NotifyDictionaryChangedAction.Remove));
            }
            return removed;
        }

        /// <summary>
        /// Get a generic enumerator for the items in the dictionary.
        /// </summary>
        /// <returns>enumerator</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
        #endregion IDictionary<TKey,TValue>
    }
}
