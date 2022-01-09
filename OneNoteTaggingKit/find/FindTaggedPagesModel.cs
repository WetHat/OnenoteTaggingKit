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

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Contract for view models of the
    /// <see cref="WetHatLab.OneNote.TaggingKit.find.FindTaggedPages" /> windows
    /// </summary>
    internal interface IFindTaggedPagesModel
    {
        /// <summary>
        /// Get the collection of tags
        /// </summary>
        RefinementTagsSource TagSource { get; }

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
        private FilteredPages _filteredTagsAndPages;
        private CancellationTokenSource _cancelWorker = new CancellationTokenSource();
        private TextSplitter _highlighter;

        internal FindTaggedPagesModel(OneNoteProxy onenote) : base(onenote) {
            _filteredTagsAndPages = new FilteredPages(onenote);
            TagSource = new RefinementTagsSource(_filteredTagsAndPages);
            // track changes in filter result
            _filteredTagsAndPages.MatchingPages.CollectionChanged += HandlePageCollectionChanges;

            CurrentPageTitle = Properties.Resources.TagSearch_CheckBox_Tracking_Text;
            // load the search history
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SearchHistory)) {
                string[] searches = Properties.Settings.Default.SearchHistory.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < searches.Length && i < Properties.Settings.Default.SearchHistory_Size; i++) {
                    SearchHistory.Add(searches[i].Trim());
                }
            }
        }

        /// <summary>
        /// Collection of tags used in a OneNote hierarchy context (section, section group, notebook)
        /// </summary>
        public TagsAndPages ContextTags { get { return new TagsAndPages(OneNoteApp); } }

        /// <summary>
        ///  Get/set the OneNote element id of the current search scope.
        /// </summary>
        public string CurrentScopeID { get; set; }

        /// <summary>
        /// FindTaggedPages pages matching a search criterion in the background.
        /// </summary>
        /// <param name="query">query. If null or empty just all page tags are collected</param>
        /// <param name="scope">Range of the search</param>
        internal async Task FindPagesAsync(string query, SearchScope scope) {
            switch (scope) {
                case SearchScope.Notebook:
                    CurrentScopeID = OneNoteApp.CurrentNotebookID;
                    break;

                case SearchScope.SectionGroup:
                    CurrentScopeID = OneNoteApp.CurrentSectionGroupID;
                    break;

                case SearchScope.Section:
                    CurrentScopeID = OneNoteApp.CurrentSectionID;
                    break;

                default:
                    CurrentScopeID = String.Empty;
                    break;
            }

            if (!string.IsNullOrEmpty(query)) {
                query = query.Trim().Replace(',', ' ');
                SearchHistory.Remove(query);
                SearchHistory.Insert(0, query);
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

            await Task.Run(() => _filteredTagsAndPages.Find(query, CurrentScopeID), _cancelWorker.Token);
        }

        internal Task ClearTagFilterAsync() {
            return Task.Run(() => _filteredTagsAndPages.ClearTagFilter(), _cancelWorker.Token);
        }

        internal Task AddTagToFilterAsync(TagPageSet tag) {
            return Task.Run(() => _filteredTagsAndPages.AddTagToFilter(tag), _cancelWorker.Token);
        }

        internal Task RemoveTagFromFilterAsync(TagPageSet tag) {
            return Task.Run(() => _filteredTagsAndPages.RemoveTagFromFilter(tag), _cancelWorker.Token);
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
        /// get the collection of all OneNote page tags.
        /// </summary>
        public RefinementTagsSource TagSource { get; }

        /// <summary>
        /// Get the collection of filtered pages.
        /// </summary>
        public ObservableSortedList<HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel> FilteredPages
            { get; } = new ObservableSortedList<HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel>();

        /// <summary>
        /// Get the default scope
        /// </summary>
        public SearchScope DefaultScope => (SearchScope)Properties.Settings.Default.DefaultScope;

        #endregion

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
                TaggedPage tp = tap.Pages.Values.FirstOrDefault();
                if (tp != null) {
                    Dispatcher.Invoke(() =>
                    {
                        CurrentPageTags = (from t in tp.Tags select t.TagName).ToArray();
                        CurrentPageTitle = tp.Title;
                    });
                }
            }
        }

        #endregion tag tracking

        private void HandlePageCollectionChanges(object sender, NotifyDictionaryChangedEventArgs<string, TaggedPage> e) {
            Action a = null;
            switch (e.Action) {
                case NotifyDictionaryChangedAction.Add:
                    a = () => FilteredPages.AddAll(from i in e.Items select new HitHighlightedPageLinkModel(i, _highlighter, OneNoteApp));
                    break;

                case NotifyDictionaryChangedAction.Remove:
                    a = () => FilteredPages.RemoveAll(from i in e.Items select i.Key);
                    break;

                case NotifyDictionaryChangedAction.Reset:
                    a = () => FilteredPages.Clear();
                    break;
            }
            Dispatcher.Invoke(a);
        }

        /// <summary>
        /// Select all tags with fully matching highligting pattern for
        /// search refinenment.
        /// </summary>
        internal void SelectAllFullyHighlightedTags() {
            foreach (RefinementTagModel t in from ht in TagSource.Values where ht.IsFullMatch select ht) {
                t.IsSelected = true;
            }
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
            EndTracking();
        }
    }
}