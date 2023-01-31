// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Contract for view models of the
    /// <see cref="WetHatLab.OneNote.TaggingKit.find.FindTaggedPages" /> windows
    /// </summary>
    internal interface IFindTaggedPagesModel {

        /// <summary>
        /// Get the collection of pages with particular tags
        /// </summary>
        ObservableSortedList<HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel> FilteredPages { get; }

        /// <summary>
        /// Get the default search scope
        /// </summary>
        SearchScope DefaultScope { get; }

        /// <summary>
        /// Get the title of the current page, if tacking is enabled
        /// </summary>
        string CurrentPageTitle { get; }

        string WithAllTabLabel { get; }
        string ExceptWithTabLabel { get; }
        string WithAnyTabLabel { get; }
    }

    /// <summary>
    /// View model backing the UI to find tagged pages.
    /// </summary>
    /// <remarks>Search queries are run in the background</remarks>
    [ComVisible(false)]
    public class FindTaggedPagesModel : WindowViewModelBase, IFindTaggedPagesModel {
        #region CurrentPageTitleProperty
        /// <summary>
        /// Backing store for observable property <see cref="CurrentPageTitle"/>
        /// </summary>
        public static readonly DependencyProperty CurrentPageTitleProperty = DependencyProperty.Register(
            nameof(CurrentPageTitle),
            typeof(string),
            typeof(FindTaggedPagesModel));

        /// <summary>
        /// Get/set the title of the tab containing the `Wiath All` filter.
        /// </summary>
        public string CurrentPageTitle {
            get => GetValue(CurrentPageTitleProperty) as string;
            set => SetValue(CurrentPageTitleProperty, value);
        }
        #endregion CurrentPageTitleProperty

        #region WithAllTabLabelProperty
        /// <summary>
        /// Backing store for observable property <see cref="WithAllTabLabel"/>
        /// </summary>
        public static readonly DependencyProperty WithAllTabLabelProperty = DependencyProperty.Register(
            nameof(WithAllTabLabel),
            typeof(string),
            typeof(FindTaggedPagesModel),
            new PropertyMetadata("..."));

        /// <summary>
        /// Get/set the title of the current OneNote page.
        /// </summary>
        public string WithAllTabLabel {
            get => GetValue(WithAllTabLabelProperty) as string;
            set => SetValue(WithAllTabLabelProperty, value);
        }
        #endregion WithAllTabLabelProperty

        #region ExceptWithTabLabelProperty
        /// <summary>
        /// Backing store for observable property <see cref="ExceptWithTabLabel"/>
        /// </summary>
        public static readonly DependencyProperty ExceptWithTabLabelProperty = DependencyProperty.Register(
            nameof(ExceptWithTabLabel),
            typeof(string),
            typeof(FindTaggedPagesModel),
            new PropertyMetadata("..."));

        /// <summary>
        /// Get/set the title of the tab containing the `Except With` filter.
        /// </summary>
        public string ExceptWithTabLabel {
            get => GetValue(ExceptWithTabLabelProperty) as string;
            set => SetValue(ExceptWithTabLabelProperty, value);
        }
        #endregion ExceptWithTabLabelProperty

        #region WithAnyTabLabelProperty
        /// <summary>
        /// Backing store for observable property <see cref="WithAllTabLabel"/>
        /// </summary>
        public static readonly DependencyProperty WithAnyTabLabelProperty = DependencyProperty.Register(
            nameof(WithAnyTabLabel),
            typeof(string),
            typeof(FindTaggedPagesModel),
            new PropertyMetadata("..."));

        /// <summary>
        /// Get/set the title of the current OneNote page.
        /// </summary>
        public string WithAnyTabLabel {
            get => GetValue(WithAnyTabLabelProperty) as string;
            set => SetValue(WithAnyTabLabelProperty, value);
        }
        #endregion WithAnyTabLabelProperty
        /// <summary>
        ///     Get the filter which requires all selected tags to be on pages.
        /// </summary>
        public TagFilterPanelModel WithAllTagsFilterModel { get; private set; }
        /// <summary>
        ///     Get the filter which requires all selected tags to be absent
        ///     on pages.
        /// </summary>
        public TagFilterPanelModel ExceptWithTagsFilterModel { get; private set; }

        /// <summary>
        ///     Get the filter which requires any selected tags to be  on pages.
        /// </summary>
        public TagFilterPanelModel WithAnyTagsFilterModel { get; private set; }


        /// <summary>
        ///     Get the collection of tags and the pages which have those tags.
        /// </summary>
        public TagsAndPages TagsAndPages { get; private set; }
        // the collection of pages matching filter criteria.
        private WithAllTagsFilter _pagesWithAllTags;
        private ExceptWithTagsFilter _pagesExceptWithTags;
        private WithAnyTagsFilter _pagesWithAnyTags;
        private CancellationTokenSource _cancelWorker = new CancellationTokenSource();
        private TextSplitter _highlighter;

        /// <summary>
        ///     Initialize a view model for the <see cref="FindTaggedPages"/>
        ///     dialog.
        /// </summary>
        /// <param name="onenote">
        ///     The OneNote application proxy.
        /// </param>
        internal FindTaggedPagesModel(OneNoteProxy onenote) : base(onenote) {
            TagsAndPages = new TagsAndPages(onenote);

            // With all tags
            _pagesWithAllTags = new WithAllTagsFilter(TagsAndPages);
            _pagesWithAllTags.AutoUodateEnabled = true;
            WithAllTagsFilterModel = new TagFilterPanelModel(onenote, _pagesWithAllTags);
            _pagesWithAllTags.SelectedTags.CollectionChanged += WithAllSelectedTags_CollectionChanged;
            Update_WithAllLabel(_pagesWithAllTags.SelectedTags);

            // Except With tags
            _pagesExceptWithTags = new ExceptWithTagsFilter(_pagesWithAllTags);
            ExceptWithTagsFilterModel = new TagFilterPanelModel(onenote, _pagesExceptWithTags);
            _pagesExceptWithTags.SelectedTags.CollectionChanged += ExceptWithSelectedTags_CollectionChanged;
            Update_ExceptWithLabel(_pagesExceptWithTags.SelectedTags);

            // with any tags
            _pagesWithAnyTags = new WithAnyTagsFilter(_pagesExceptWithTags);
            WithAnyTagsFilterModel = new TagFilterPanelModel(onenote, _pagesWithAnyTags);
            _pagesWithAnyTags.SelectedTags.CollectionChanged += WithAnySelectedTags_CollectionChanged;
            Update_WithAnyLabel(_pagesWithAnyTags.SelectedTags);

            // track changes in filter result
            _pagesWithAnyTags.FilteredPages.CollectionChanged += HandlePageCollectionChanges;

            CurrentPageTitle = Properties.Resources.TagSearch_CheckBox_Tracking_Text;
            // load the search history
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SearchHistory)) {
                string[] searches = Properties.Settings.Default.SearchHistory.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < searches.Length && i < Properties.Settings.Default.SearchHistory_Size; i++) {
                    SearchHistory.Add(searches[i].Trim());
                }
            }
        }
        void Update_WithAllLabel(ObservableDictionary<string, TagPageSet> selected) {
            WithAllTabLabel = string.Format(Properties.Resources.TagSearch_Filter_Label_AllTags, selected.Count);
        }
        private void WithAllSelectedTags_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, TagPageSet> e) {
            Dispatcher.Invoke(() => Update_WithAllLabel(sender as ObservableDictionary<string, TagPageSet>));
        }
        void Update_ExceptWithLabel(ObservableDictionary<string, TagPageSet> selected) {
            ExceptWithTabLabel = string.Format(Properties.Resources.TagSearch_Filter_Label_NoneTags, selected.Count);
        }
        private void ExceptWithSelectedTags_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, TagPageSet> e) {
            Dispatcher.Invoke(() => Update_ExceptWithLabel(sender as ObservableDictionary<string, TagPageSet>));
        }

        void Update_WithAnyLabel(ObservableDictionary<string, TagPageSet> selected) {
            WithAnyTabLabel = string.Format(Properties.Resources.TagSearch_Filter_Label_AnyTags, selected.Count);
        }
        private void WithAnySelectedTags_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, TagPageSet> e) {
            Dispatcher.Invoke(() => Update_WithAnyLabel(sender as ObservableDictionary<string, TagPageSet>));
        }
        /// <summary>
        ///     Get scope used to search for OneNOte pages.
        /// </summary>
        public SearchScope CurrentScope { get; private set; }

        /// <summary>
        /// FindTaggedPages pages matching a search criterion in the background.
        /// </summary>
        /// <param name="query">query. If null or empty just all page tags are collected</param>
        /// <param name="scope">Range of the search</param>
        internal async Task FindPagesAsync(string query, SearchScope scope) {
            CurrentScope = scope;
            if (!string.IsNullOrEmpty(query)) {
                query = query.Trim().Replace(',', ' ');
                SearchHistory.Remove(query);
                SearchHistory.Insert(0, query); // move query to the front
                // update settings
                StringBuilder history = new StringBuilder();
                for (int i = 0; i < SearchHistory.Count() && i < Properties.Settings.Default.SearchHistory_Size; i++) {
                    if (history.Length > 0) {
                        history.Append(',');
                    }
                    history.Append(SearchHistory[i]);
                }
                Properties.Settings.Default.SearchHistory = history.ToString();

                // construct the query pattern
                string[] words = query.Split(new char[] { ',', ' ', ':', ';' }, StringSplitOptions.RemoveEmptyEntries);

                _highlighter = new TextSplitter(from w in words select w.Replace("'", "").Replace("\"", ""));
            } else {
                _highlighter = new TextSplitter();
            }

            await Task.Run(() => TagsAndPages.FindPages(scope,query), _cancelWorker.Token);
        }

        // Collection of previous searches
        private readonly ObservableCollection<string> _searchHistory = new ObservableCollection<string>();

        /// <summary>
        /// Get the list of previous searches.
        /// </summary>
        public ObservableCollection<string> SearchHistory {
            get {
                return _searchHistory;
            }
        }

        #region IFindTaggedPagesModel

        /// <summary>
        /// Get the collection of filtered pages.
        /// </summary>
        public ObservableSortedList<HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel> FilteredPages { get; } = new ObservableSortedList<HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel>();

        /// <summary>
        /// Get the default scope
        /// </summary>
        public SearchScope DefaultScope => (SearchScope)Properties.Settings.Default.DefaultScope;

        #endregion IFindTaggedPagesModel

        #region tag tracking
        private Timer _tracker;

        string _currentPageID = string.Empty;
        internal void BeginTracking() {
            if (_tracker == null) {
                _tracker = new Timer(TrackCurrentPage, null, 1000, 1000);
            }
        }

        internal void EndTracking() {
            if (_tracker != null) {
                _currentPageID = string.Empty;
                var waitHandle = new ManualResetEvent(false);
                _tracker.Dispose(waitHandle);
                waitHandle.WaitOne();
                _tracker = null;
            }
            CurrentPageTitle = Properties.Resources.TagSearch_CheckBox_Tracking_Text;
        }

        private void TrackCurrentPage(object state) {
            var currentPage =  OneNoteApp.CurrentPageID;
            if (_currentPageID != currentPage) {
                _currentPageID = currentPage;
                WithAllTagsFilterModel.ContextTagSource.LoadPageTags(TagContext.CurrentNote,false);

                var mdls = (from mdl in WithAllTagsFilterModel.RefinementTagModels.Values
                            where mdl.IsFullMatch
                            select mdl).ToList();
                PageNode pg = WithAllTagsFilterModel.ContextTagSource.Pages.Values.FirstOrDefault();
                if (pg != null) {
                    Dispatcher.Invoke(() => {
                        CurrentPageTitle = pg.Name;
                        ExceptWithTagsFilterModel.Filter.SelectedTags.Clear();
                        WithAnyTagsFilterModel.Filter.SelectedTags.Clear();
                        WithAllTagsFilterModel.ResetFilter(mdls);
                    });
                }
            }
        }

        #endregion tag tracking

        /// <summary>
        /// Reflect changes to the filteren pages on the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandlePageCollectionChanges(object sender, NotifyDictionaryChangedEventArgs<string, PageNode> e) {
            Action a = null;
            var origin = sender as ObservableDictionary<string, PageNode>;
            switch (e.Action) {
                case NotifyDictionaryChangedAction.Add:
                    if (origin.Count < TagsAndPages.Pages.Count || !string.IsNullOrWhiteSpace(TagsAndPages.Query)) {
                        IEnumerable<PageNode> toAdd;
                        if (FilteredPages.Count > 0) {
                            toAdd = e.Items;
                        } else {
                            // nothing there yet display all filtered pages
                            toAdd = origin.Values.ToArray();
                        }

                        a = () => FilteredPages.AddAll(from i in toAdd select new HitHighlightedPageLinkModel(i, _highlighter));
                    } else {
                        // too many pages to display
                        a = () => FilteredPages.Clear();
                    }
                    break;

                case NotifyDictionaryChangedAction.Remove:
                    if (FilteredPages.Count > 0) {
                        a = () => FilteredPages.RemoveAll(from i in e.Items select i.Key);
                    } else {
                        // nothing there yet display all filtered pages
                        IEnumerable<PageNode> pages = origin.Values.ToArray();
                        a = () => FilteredPages.AddAll(from i in pages select new HitHighlightedPageLinkModel(i, _highlighter));
                    }
                    break;

                case NotifyDictionaryChangedAction.Reset:
                    // we need to rebuild the entire model in case page properties
                    // have changed
                    if (origin.Count < TagsAndPages.Pages.Count || !string.IsNullOrWhiteSpace(TagsAndPages.Query)) {
                        PageNode[] pages = origin.Values.ToArray();
                        a = () => {
                            FilteredPages.Clear();
                            FilteredPages.AddAll(from i in pages select new HitHighlightedPageLinkModel(i, _highlighter));
                        };

                     } else {
                        // avoid displaying excessive amount of pages
                        a = () => FilteredPages.Clear();
                    }
                    break;
            }
            Dispatcher.Invoke(a);
        }

        internal void NavigateTo(string pageID) {
            OneNoteApp.NavigateTo(pageID);
        }

        /// <summary>
        /// Dispose the view model.
        /// </summary>
        public override void Dispose() {
            base.Dispose();
            if (_cancelWorker != null) {
                _cancelWorker.Cancel();
                _cancelWorker.Dispose();
                _cancelWorker = null;
            }
            if (_pagesWithAllTags != null) {
                _pagesWithAllTags.Dispose();
                _pagesWithAllTags = null;
            }
            if (_pagesExceptWithTags != null) {
                _pagesExceptWithTags.Dispose();
                _pagesExceptWithTags = null;
            }
            if (_pagesWithAnyTags != null) {
                _pagesWithAnyTags.Dispose();
                _pagesWithAnyTags = null;
            }
            EndTracking();
        }
    }
}