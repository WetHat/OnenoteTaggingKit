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

    public class ObservableDictionary<TKey,TValue> where TValue : IKeyedItem<TKey>
                                                   where TKey   : IEquatable<TKey>
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

        public ICollection<TValue> Values
        {
            get
            {
                return _dictionary.Values;
            }
        }

        public bool Add(TValue value)
        {
            TValue found;
            bool added;
            if (!(added = _dictionary.TryGetValue(value.Key,out found)))
            {
                fireChangedEvent(new NotifyDictionaryChangedEventArgs<TKey,TValue>(value,NotifyDictionaryChangedAction.Add));
            }
            return added;
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

        public bool Contains(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Contains(TValue value)
        {
            return _dictionary.ContainsKey(value.Key);
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
    }
}
