using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace WetHatLab.OneNote.TaggingKit.common
{
    public enum NotifyDictionaryChangedAction
    {
        Add,
        Remove,
        Reset
    }
    public class NotifyDictionaryChangedEventArgs<TKey,TValue> : EventArgs where TValue : IKeyedItem<TKey>
                                                                           where TKey   : IEquatable<TKey>
    {
        public NotifyDictionaryChangedAction Action { get; private set; }
        public IEnumerable<TValue> Items { get; private set;}

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

    public delegate void NotifyDictionaryChangedEventHandler<TKey, TValue>(object sender, NotifyDictionaryChangedEventArgs<TKey, TValue> e)
        where TValue : IKeyedItem<TKey>
        where TKey : IEquatable<TKey>;

    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TValue : IKeyedItem<TKey>
        where TKey : IEquatable<TKey>
    {
        private IDictionary<TKey, TValue> _dictionary = new Dictionary<TKey,TValue>();

        public event NotifyDictionaryChangedEventHandler<TKey,TValue> CollectionChanged;

        private void fireChangedEvent(NotifyDictionaryChangedEventArgs<TKey,TValue> e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }

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

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public ICollection<TValue> Values
        {
            get
            {
                return _dictionary.Values;
            }
        }

        public ICollection<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

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

        public void Clear()
        {
            _dictionary.Clear();
            fireChangedEvent(new NotifyDictionaryChangedEventArgs<TKey, TValue>());
        }

        public int Count
        {
            get { return _dictionary.Count; }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public void Add(TKey key, TValue value)
        {
            Add(value);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

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

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool removed = _dictionary.Remove(item);
            if (removed)
            {
                fireChangedEvent(new NotifyDictionaryChangedEventArgs<TKey,TValue>(item.Value,NotifyDictionaryChangedAction.Remove));
            }
            return removed;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }
        #endregion IDictionary<TKey,TValue>
    }
}
