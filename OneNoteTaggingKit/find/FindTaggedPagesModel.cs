// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
        /// Get the collection of tags
        /// </summary>
        RefinementTagsSource PageTagsSource { get; }

        /// <summary>
        /// Get the collection of pages with particular tags
        /// </summary>
        ObservableSortedList<HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel> FilteredPages { get; }

        /// <summary>
        /// View models of tags currently selected for refinement.
        /// </summary>
        ObservableTagList<SelectedTagModel> SelectedRefinementTags { get; }
        /// <summary>
        /// Get the default search scope
        /// </summary>
        SearchScope DefaultScope { get; }

        /// <summary>
        /// Get the title of the current page, if tacking is enabled
        /// </summary>
        string CurrentPageTitle { get; }
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
        /// Get/set the title of the current OneNote page.
        /// </summary>
        public string CurrentPageTitle {
            get => GetValue(CurrentPageTitleProperty) as string;
            set => SetValue(CurrentPageTitleProperty, value);
        }
        #endregion CurrentPageTitleProperty
        #region CurrentPageTagsProperty
        /// <summary>
        /// Backing store for observable property <see cref="CurrentPageTags"/>
        /// </summary>
        public static readonly DependencyProperty CurrentPageTagsProperty = DependencyProperty.Register(
            nameof(CurrentPageTags),
            typeof(IEnumerable<string>),
            typeof(FindTaggedPagesModel),
            new FrameworkPropertyMetadata(new string[] { }));

        /// <summary>
        /// Get/set the list of tags found on the current OneNote page.
        /// </summary>
        public IEnumerable<string> CurrentPageTags {
            get => GetValue(CurrentPageTagsProperty) as IEnumerable<string>;
            set => SetValue(CurrentPageTagsProperty, value);
        }
        #endregion CurrentPageTagsProperty

        // the collection of pages matching filter criteria.
        private TagsAndPages _tagsAndPages;
        private WithAllTagsFilter _pagesWithAllTags;
        private CancellationTokenSource _cancelWorker = new CancellationTokenSource();
        private TextSplitter _highlighter;

        internal FindTaggedPagesModel(OneNoteProxy onenote) : base(onenote) {
            _tagsAndPages = new TagsAndPages(onenote);
            _pagesWithAllTags = new WithAllTagsFilter(_tagsAndPages);
            _pagesWithAllTags.AutoUodateEnabled = true;
            PageTagsSource = new RefinementTagsSource(_pagesWithAllTags);
            // track changes to the tag source so that we can update the selected tags accordingly
            PageTagsSource.CollectionChanged += PageTagsSource_CollectionChanged;
            // track changes in filter result
            _pagesWithAllTags.FilteredPages.CollectionChanged += HandlePageCollectionChanges;

            CurrentPageTitle = Properties.Resources.TagSearch_CheckBox_Tracking_Text;
            // load the search history
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SearchHistory)) {
                string[] searches = Properties.Settings.Default.SearchHistory.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < searches.Length && i < Properties.Settings.Default.SearchHistory_Size; i++) {
                    SearchHistory.Add(searches[i].Trim());
                }
            }
        }

        private void PageTagsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Remove:
                    // deselect dead refinement tags
                    SelectedRefinementTags.RemoveAll(from RefinementTagModel mdl in e.OldItems select mdl.Key);
                    break;
            }
        }

        /// <summary>
        /// Collection of tags used in a OneNote hierarchy context (section, section group, notebook)
        /// </summary>
        public TagsAndPages ContextTags { get { return new TagsAndPages(OneNoteApp); } }

        /// <summary>
        ///  Get/set the OneNote element id of the current search scope.
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

            await Task.Run(() => _tagsAndPages.FindPages(scope,query), _cancelWorker.Token);
        }

        /// <summary>
        /// Add a single tag to the refinement filter.
        /// </summary>
        /// <param name="tag">View model of the tag.</param>
        internal void AddTagToFilter(RefinementTagModel tag) {
            if (!SelectedRefinementTags.ContainsKey(tag.Key)) {
                SelectedRefinementTags.AddAll(new SelectedTagModel[] {
                    new SelectedTagModel() {
                        SelectableTag = tag,
                        TagIndicator = "",
                        TagIndicatorColor = Brushes.Red
                    } });
            }
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
        /// get the collection of all tags found on OneNote pages.
        /// </summary>
        public RefinementTagsSource PageTagsSource { get; }

        /// <summary>
        /// Get the collection of filtered pages.
        /// </summary>
        public ObservableSortedList<HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel> FilteredPages { get; } = new ObservableSortedList<HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel>();

        /// <summary>
        /// Get the default scope
        /// </summary>
        public SearchScope DefaultScope => (SearchScope)Properties.Settings.Default.DefaultScope;

        /// <summary>
        /// Get the observable list of tags selected for refinement.
        /// </summary>
        public ObservableTagList<SelectedTagModel> SelectedRefinementTags { get; } = new ObservableTagList<SelectedTagModel>();

        #endregion IFindTaggedPagesModel

        #region tag tracking
        private Timer _tracker;
        private string _currentPageID = string.Empty;

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
            if (!_currentPageID.Equals(OneNoteApp.CurrentPageID)) {
                _currentPageID = OneNoteApp.CurrentPageID;
                TagsAndPages tap = new TagsAndPages(OneNoteApp);
                tap.LoadPageTags(TagContext.CurrentNote);
                PageNode tp = tap.Pages.Values.FirstOrDefault();
                if (tp != null) {
                    Dispatcher.Invoke(() => {
                        CurrentPageTags = (from t in tp.Tags select t.BaseName).ToArray();
                        CurrentPageTitle = tp.Name;
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
            switch (e.Action) {
                case NotifyDictionaryChangedAction.Add:
                    IEnumerable<PageNode> toAdd;
                    if (FilteredPages.Count > 0) {
                        toAdd = e.Items;
                    } else {
                        // nothing there yet display all filtered pages
                        var origin = sender as ObservableDictionary<string, PageNode>;
                        toAdd = origin.Values.ToArray();
                    }
                    a = () => FilteredPages.AddAll(from i in toAdd select new HitHighlightedPageLinkModel(i, _highlighter));
                    break;

                case NotifyDictionaryChangedAction.Remove:
                    if (FilteredPages.Count > 0) {
                        a = () => FilteredPages.RemoveAll(from i in e.Items select i.Key);
                    } else {
                        // nothing there yet display all filtered pages
                        var origin = sender as ObservableDictionary<string, PageNode>;
                        IEnumerable<PageNode> pages = origin.Values.ToArray();
                        a = () => FilteredPages.AddAll(from i in pages select new HitHighlightedPageLinkModel(i, _highlighter));
                    }
                    break;

                case NotifyDictionaryChangedAction.Reset:
                    // we need to rebuild the entire model in case page properties
                    // have changed
                    if (_pagesWithAllTags.FilteredPages.Count < _tagsAndPages.Pages.Count || !string.IsNullOrWhiteSpace(_tagsAndPages.Query)) {
                        var origin = sender as ObservableDictionary<string, PageNode>;
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
            _cancelWorker.Cancel();
            if (_pagesWithAllTags != null) {
                _pagesWithAllTags.Dispose();
                _pagesWithAllTags = null;
            }
            EndTracking();
        }
    }
}