﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using WetHatLab.OneNote.TaggingKit.common.ui;
using System.Windows.Threading;
using System.Runtime.InteropServices;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// Adapter class to facilitate raising events in a given tread context.
    /// </summary>
    public class EventAdapter
    {
        NotifyCollectionChangedEventHandler _originalHandler;
        Dispatcher _dispatcher;
        /// <summary>
        /// Initialize a new event adapter object.
        /// </summary>
        /// <param name="d">
        ///     Dispatcher to use for raising events.
        /// </param>
        /// <param name="handler">
        ///     Event handler to call in the given dispatcher's context.
        /// </param>
        public EventAdapter(Dispatcher d, NotifyCollectionChangedEventHandler handler) {
            _dispatcher = d;
            _originalHandler = handler;
        }

        /// <summary>
        /// Event handler adapter which calls the original event in the specified
        /// thread context.
        /// </summary>
        /// <param name="sender">Object which raised the event.</param>
        /// <param name="args">Event details.</param>
        public void Handler(object sender, NotifyCollectionChangedEventArgs args) {
            _dispatcher.Invoke(() => _originalHandler(sender, args));
        }
    }

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
    ///     Items in this list are disposed when:
    ///     <list type="bullet">
    ///         <item>they are removed from the list</item>
    ///         <item>the list is disposed itself</item>
    ///     </list>
    ///
    ///     Instances of  this observable list have affinity to the thread they
    ///     were created by. Collection change events are raised in the context
    ///     of that thread.
    /// </remarks>
    /// <typeparam name="T">
    ///     The tag model type. Either a <see cref="TagModel"/> or a sub-class of it.
    /// </typeparam>
    [ComVisible(false)]
    public class ObservableTagList<T> : ObservableSortedList<string, string, T>, IObservableTagList
                                                      where T : TagModel {
        #region IObservableTagList
        object IReadOnlyList<object>.this[int index] => this[index];
        /// <summary>
        /// Get all tags as object list.
        /// </summary>
        /// <returns>Generic object list.</returns>
        public IList ToTagList() => Values.ToArray();
        IEnumerator<object> IEnumerable<object>.GetEnumerator() => GetEnumerator();
        #endregion IObservableTagList

        /// <summary>
        /// The dispatcher in whose contect this observable list instance was
        /// created in.
        /// </summary>
        protected Dispatcher OriginalDispatcher { get; }

        /// <summary>
        /// Event raised when the list of tags has changed,
        /// </summary>
        /// <remarks>
        ///     The events are raised in the context of the thread in which this
        ///     observable list instance was generated.
        /// </remarks>
        public override event NotifyCollectionChangedEventHandler CollectionChanged {
            add {
                var adapter = new EventAdapter(OriginalDispatcher, value);
                base.CollectionChanged += adapter.Handler;
            }
            remove {
                throw new NotImplementedException("Event handlers cannot be removed!");
            }
        }

        /// <summary>
        /// Create a new instance of an observable list of tag view models.
        /// </summary>
        /// <remarks>
        ///     The list is configured to dispose view model on removal
        ///     by default
        /// </remarks>
        public ObservableTagList() {
            DisposeRemovedItems = true;
            OriginalDispatcher = Dispatcher.CurrentDispatcher;
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
