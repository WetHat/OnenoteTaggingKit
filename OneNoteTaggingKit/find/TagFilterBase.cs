using System;
using System.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    ///     Abstract base class to define and apply rules to filter
    ///     down OneNote pages based on tags.
    /// </summary>
    public abstract class TagFilterBase : IDisposable, ITagsAndPages
    {
        /// <summary>
        ///     Fet the original, unfiltered source of the collections of tags and
        ///     pages.
        /// </summary>
        public ITagsAndPages Source { get; private set; }

        /// <summary>
        ///     Get the collection of tags currently selected for refinement.
        /// </summary>
        public ObservableDictionary<string, TagPageSet> SelectedTags { get; } = new ObservableDictionary<string, TagPageSet>();

        /// <summary>
        ///     Get the collection of tags available for refinement.
        /// </summary>
        public ObservableDictionary<string, RefinementTag> RefinementTags { get; } = new ObservableDictionary<string, RefinementTag>();

        #region ITagsAndPages
        /// <summary>
        ///     Get the collection of pages resulting from application of the
        ///     tag-based refinement constraint implemented by concrete subclasses.
        /// </summary>
        public ObservableDictionary<string, PageNode> Pages { get; } = new ObservableDictionary<string, PageNode>();

        /// <summary>
        ///     Get the observable collection of tags this filter has access to.
        /// </summary>
        public ObservableDictionary<string, TagPageSet> Tags => Source.Tags;
        #endregion ITagsAndPages
        /// <summary>
        ///     Initialite a new instance of this base class with collections of
        ///     tags and OneNOte pages.
        /// </summary>
        /// <param name="source">Collection of tags and OneNte pages.</param>
        public TagFilterBase(ITagsAndPages source) {
            Source = source;
            Source.Tags.CollectionChanged += Tags_CollectionChanged;
            Source.Pages.CollectionChanged += Pages_CollectionChanged;

            SelectedTags.CollectionChanged += SelectedTags_CollectionChanged;
            // populate the collection of pages and tags from the source
            Pages.Reset(Source.Pages.Values);
        }

        /// <summary>
        ///     Update the collection of refinement tags when the original tag
        ///     source collection changes.
        /// </summary>
        /// <remarks>
        ///     Only basic synchronizationb is performed. It is expected that
        ///     there will be a correlated event for tha pages collection and
        ///     the <see cref="Pages_CollectionChanged"/> event handler calls
        ///     <see cref="RecomputeFilteredPages"/> of the concrete subclass
        ///     to perform specific synchronization.
        /// </remarks>
        /// <param name="sender">The changed tag collection.</param>
        /// <param name="e">Change event details.</param>
        private void Tags_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, TagPageSet> e) {
            switch (e.Action) {
                case NotifyDictionaryChangedAction.Add: {
                        // create decorators for all these tags
                        RefinementTags.UnionWith(from it in e.Items select new RefinementTag(it));
                        break;
                    }
                case NotifyDictionaryChangedAction.Remove:
                    // remove obsolete refinement tags
                    foreach (var tag in e.Items) {
                        RefinementTags.Remove(tag.Key);
                    }
                    // remove obsolete tags from selected tags.
                    try {
                        // disable the event on selected tags expecting that the
                        // Pages event handler does that job.
                        _selTagUpdateEnabled = false;
                        SelectedTags.ExceptWith(e.Items);
                    } finally {
                        _selTagUpdateEnabled = true;
                    }
                    break;
                case NotifyDictionaryChangedAction.Reset:
                    // rebuild the from scratch
                    RefinementTags.Reset(from TagPageSet t in Source.Tags select new RefinementTag(t));
                    // make sure the selection contains only valid tags
                    SelectedTags.IntersectWith(Source.Tags.Values);
                    break;
            }
        }

        /// <summary>
        ///     Update the filtered page set after changes to the collection tag
        ///     and pages collections.
        /// </summary>
        /// <param name="e">Change eventdetails.</param>
        abstract protected void RecomputeFilteredPages(NotifyDictionaryChangedEventArgs<string, TagPageSet> e);

        bool _selTagUpdateEnabled = true;
        private void SelectedTags_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, TagPageSet> e) {
            if (_selTagUpdateEnabled) {
                RecomputeFilteredPages(e); // let derived classes recompute the filtered pages
            }
        }

        private void Pages_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, PageNode> e) {
            // When the collection of pages changes it is necessaryy to reapply
            // the filter rules from scratch
            RecomputeFilteredPages(new NotifyDictionaryChangedEventArgs<string, TagPageSet>());
        }

        #region IDisposable
        /// <summary>
        ///     Remove event handlers from the source collections to allow
        ///     garbage collection of this instance.
        /// </summary>
        public virtual void Dispose() {
            Source.Tags.CollectionChanged -= Tags_CollectionChanged;
            Source.Pages.CollectionChanged -= Pages_CollectionChanged;
        }
        #endregion IDisposable
    }
}
