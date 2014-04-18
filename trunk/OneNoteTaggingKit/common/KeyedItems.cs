using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// Contract for classes which want to provide a unique key suitable for hashing 
    /// </summary>
    /// <typeparam name="T">Type of key</typeparam>
    public interface IKeyedItem<T> where T : IEquatable<T>
    {
        /// <summary>
        /// Get a unique key
        /// </summary>
        T Key { get; }
    }

    /// <summary>
    /// Contract for classes which want to provide keys suitable for hashing and sorting
    /// </summary>
    /// <typeparam name="TKey">unique key</typeparam>
    /// <typeparam name="TSort">Type of the sortable key</typeparam>
    public interface ISortableKeyedItem<TSort,TKey> : IKeyedItem<TKey> where TSort : IComparable<TSort>
                                                                       where TKey  : IEquatable<TKey>
    {
        /// <summary>
        /// Get the key suitable for sorting
        /// </summary>
        TSort SortKey { get;  }
    }
}
