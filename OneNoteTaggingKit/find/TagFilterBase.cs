using System;
using System.Collections.Generic;
using System.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    ///     Abstract base class to define and apply rules to filter
    ///     down OneNote pages based on tags.
    /// </summary>
    /// <remarks>
    ///     Filter classes are designed to be connected to each other to form
    ///     a filter chain
    ///     ~~~bob
    ///                          ┌─ filter ─┐
    ///     ┌──────────────┐ ┌────────┐ ┌────────┐
    ///     │ TagsAndPages │←--Source │←--Source │
    ///     └──────────────┘ │ Next ---→│ Next   │
    ///                      └────────┘ └────────┘
    ///     ~~~
    ///     Eeach node in the filter chain applies its specific rules to
    ///     the page collection handed down via the `Source`s `Pages` and
    ///     stores the result in its own `Pages` property.
    ///
    /// </remarks>
    public abstract class TagFilterBase : IDisposable, ITagsAndPages
    {
        /// <summary>
        ///     Get the source of tags and
        ///     pages from the previous node in the filter chain.
        /// </summary>
        /// <remarks>
        ///     The set of pages
        /// </remarks>
        public ITagsAndPages Source { get; private set; }

        /// <summary>
        ///     Get or set the next filter in the filter chain.
        /// </summary>
        public TagFilterBase Next { get; set; }
        /// <summary>
        ///     Get the collection of tags currently selected for refinement.
        /// </summary>
        public ObservableDictionary<string, TagPageSet> SelectedTags { get; } = new ObservableDictionary<string, TagPageSet>();

        /// <summary>
        ///     Get the collection of tags available for refinement.
        /// </summary>
        /// <remarks>
        ///     The tags may contain pages which are not in the <see cref="Pages"/>
        ///     collection.
        /// </remarks>
        public ObservableDictionary<string, RefinementTagBase> RefinementTags { get; } = new ObservableDictionary<string, RefinementTagBase>();

        #region ITagsAndPages
        /// <summary>
        ///     Get the collection of pages resulting from application of the
        ///     tag-based refinement constraint implemented by concrete subclasses.
        /// </summary>
        public ObservableDictionary<string, PageNode> Pages { get; } = new ObservableDictionary<string, PageNode>();

        /// <summary>
        ///     The collection of tags found on OneNoote pages.
        /// </summary>
        public ObservableDictionary<string, TagPageSet> Tags => Source.Tags;
        #endregion ITagsAndPages

        /// <summary>
        ///     Get the The final collection of pages after application of all filters in the
        ///     filter chain.
        /// </summary>
        public ObservableDictionary<string, PageNode> FilteredPages => Next != null ? Next.FilteredPages : Pages;

        /// <summary>
        ///     Factory method to create a refinement tag which implements the
        ///     approriate filter logic.
        /// </summary>
        /// <param name="tag">A tag with all the pages on them.</param>
        /// <returns>A new refinement tag instance.</returns>
        public abstract RefinementTagBase MakeRefinementTag(TagPageSet tag);

        /// <summary>
        ///     Compute the filter effect for each refinement tag in this filter
        ///     an the current set of filtered pages.
        /// </summary>
        /// <remarks>
        ///     Computes the <see cref="RefinementTagBase.FilteredPageCount"/>
        ///     and <see cref="RefinementTagBase.FilteredPageCountDelta"/>
        ///     properties for each <see cref="RefinementTagBase"/> instance in
        ///     this filter..
        /// </remarks>
        public void RefreshRefinementTags() {
           foreach (var rt in RefinementTags.Values) {
                rt.FilterEffect(FilteredPages.Values);
            }
        }

        bool _autoupdate = false;
        /// <summary>
        ///     Get or set the auto-update flag.
        /// </summary>
        /// <remarks>
        ///     Enables or disables the automatic calculateion of refinement
        ///     filter effects after changes in the filter configuration.
        ///
        ///     Filter effects are represented by the
        ///     <see cref="RefinementTagBase.FilteredPageCountDelta"/>
        ///     and <see cref="RefinementTagBase"/> properties of the
        ///     refinement tags in this filter.
        /// </remarks>
        public bool AutoUodateEnabled {
            get => _autoupdate;
            set {
                if (_autoupdate != value) {
                    _autoupdate = value;
                    if (_autoupdate == true) {
                        RefreshRefinementTags();
                    }
                }
            }
        }

        /// <summary>
        ///     Initialize a new instance of this base class and connect it to
        ///     a source of tags and pages.
        /// </summary>
        /// <param name="source">SOurce of tags and OneNote pages.</param>
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
        ///     there will be a correlated event for the source page collection and
        ///     the <see cref="Pages_CollectionChanged"/> event handler performs
        ///     the necessary synchronization.
        /// </remarks>
        /// <param name="sender">The changed tag collection.</param>
        /// <param name="e">Change event details.</param>
        private void Tags_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, TagPageSet> e) {
            switch (e.Action) {
                case NotifyDictionaryChangedAction.Add: {
                        // create decorators for all these tags
                        RefinementTags.UnionWith(from it in e.Items select MakeRefinementTag(it));
                        break;
                    }
                case NotifyDictionaryChangedAction.Remove:
                    // remove obsolete refinement tags
                    var toRemove = new Stack<RefinementTagBase>();
                    foreach (var tag in e.Items) {
                        RefinementTagBase found;
                        if (RefinementTags.TryGetValue(tag.Key, out found)) {
                            toRemove.Push(found);
                        }
                    }
                    RefinementTags.ExceptWith(toRemove);
                    // remove obsolete tags from the tag filter.
                    try {
                        // disable the event on selected tags expecting that the
                        // pages chenge event handler does recompute the
                        // filtered pages in a subsequent RESET event.

                        _selTagUpdateEnabled = false;
                        SelectedTags.ExceptWith(e.Items);
                    } finally {
                        _selTagUpdateEnabled = true;
                    }
                    break;
                case NotifyDictionaryChangedAction.Reset:
                    // rebuild the from scratch
                    RefinementTags.Reset(from TagPageSet t in Source.Tags select MakeRefinementTag(t));
                    // make sure the selection contains only valid tags
                    try {
                        // disable the event on selected tags expecting that the
                        // pages chenge event handler does recompute the
                        // filtered pages in a subsequent RESET event.

                        _selTagUpdateEnabled = false;
                        SelectedTags.IntersectWith(Source.Tags.Values);
                    } finally {
                        _selTagUpdateEnabled = true;
                    }
                    break;
            }
        }

        /// <summary>
        ///     Apply the tag filter to a given set of pages.
        /// </summary>
        /// <param name="pages">
        ///     Collection of pages.
        /// </param>
        /// <returns>
        ///     Collection of pages which staisfy the filter condition inplemented
        ///     by the specific subclass.
        /// </returns>
        protected virtual IEnumerable<PageNode> FilterPages(IEnumerable<PageNode> pages) {
            IEnumerable<PageNode> filtered = pages;

            foreach (TagPageSet t in SelectedTags.Values) {
                RefinementTagBase rt;
                if (RefinementTags.TryGetValue(t.Key, out rt)) {
                    filtered = rt.FilterPages(filtered);
                }
            }
            return filtered;
        }

        /// <summary>
        ///     Process changes to the collection of filter tags and update the
        ///     collection of filtered pages accordingly.
        /// </summary>
        /// <param name="e">
        ///     Change details for the collection of filter tags.
        /// </param>
        abstract protected void UpdateTagFilter(NotifyDictionaryChangedEventArgs<string, TagPageSet> e);

        bool _selTagUpdateEnabled = true;
        private void SelectedTags_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, TagPageSet> e) {
            if (_selTagUpdateEnabled) {
                UpdateTagFilter(e); // let derived classes recompute the filtered pages
            }
        }

        private void Pages_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, PageNode> e) {
            switch (e.Action) {
                case NotifyDictionaryChangedAction.Add:
                    // apply the filter and add to the current set of pages.
                    Pages.UnionWith(FilterPages(e.Items));
                    break;
                case NotifyDictionaryChangedAction.Remove:
                    // just remove these obsolete pages.
                    Pages.ExceptWith(e.Items);
                    break;
                case NotifyDictionaryChangedAction.Reset:
                    Pages.Reset(FilterPages(Source.Pages.Values));
                    break;
            }
            if (AutoUodateEnabled) {
                RefreshRefinementTags();
            }
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
