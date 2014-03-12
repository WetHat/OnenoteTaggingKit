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
    /// Contract for class which want to provide a unique key suitable for hashing and sorting
    /// </summary>
    /// <typeparam name="T">Type of the sortable unique key</typeparam>
    public interface ISortableKeyedItem<T> : IKeyedItem<T> where T : IComparable<T>, IEquatable<T>
    {
    }
}
