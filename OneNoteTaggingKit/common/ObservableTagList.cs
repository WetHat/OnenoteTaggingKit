using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// </summary>>
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

        #region IDisposable
        public override void Dispose() {
            base.Dispose();
            // dispose all items in this list
            foreach (var itm in this) {
                itm.Dispose();
            }
            Clear();
        }
        #endregion IDisposable
    }
}
