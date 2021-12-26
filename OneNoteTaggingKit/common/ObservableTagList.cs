using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// Contract for read-only, observable lists of tag models.
    /// </summary>
    public interface IObservableTagList : IEnumerable, INotifyCollectionChanged, IReadOnlyList<object>, IDisposable
    {
        /// <summary>
        /// Get all items as a list.
        /// </summary>
        /// <returns>Item list.</returns>
        IList ToTagList();
    }

    /// <summary>
    /// A generic observable list of sorted page tag models.
    /// </summary>
    /// <remarks>
    /// Items in this list are disposed when:
    /// <list type="bullet">
    ///     <item>they are removed from the list</item>
    ///     <item>the list is disposed itself</item>
    /// </list>
    /// </remarks>
    /// <typeparam name="T">
    ///     The tag model type. Either a <see cref="TagModel"/> or a sub-class of it.
    /// </typeparam>
    public class ObservableTagList<T> : ObservableSortedList<TagModelKey, string, T>, IObservableTagList
                                                      where T : TagModel {
        #region IObservableTagList
        object IReadOnlyList<object>.this[int index] => this[index];

        public IList ToTagList() => Values.ToArray();
        IEnumerator<object> IEnumerable<object>.GetEnumerator() => GetEnumerator();
        #endregion IObservableTagList

        /// <summary>
        /// Create a new instance of an observable list of tag view models.
        /// </summary>
        /// <remarks>
        ///     The list is configured to dispose view model on removal
        ///     by default
        /// </remarks>
        public ObservableTagList() {
            DisposeRemovedItems = true;
        }

        #region IDisposable
        /// <summary>
        /// Dispose all items in this list.
        /// </summary>
        public override void Dispose() {
            Clear(); // empty list so that all items get disposed.
            base.Dispose();
        }
        #endregion IDisposable
    }
}
