using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Contract for view models of the <see cref="WetHatLab.OneNote.TaggingKit.find.FindTaggedPages"/> windows
    /// </summary>
    internal interface ITagSearchModel
    {
        /// <summary>
        /// Get the collection of pages with particular tags
        /// </summary>
        ObservableSortedList <HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel> Pages { get; }
        
        /// <summary>
        /// Get the collection of tags
        /// </summary>
        TagSource Tags { get; }
        /// <summary>
        /// Get the number of pages in the search result
        /// </summary>
        int PageCount { get; }
        /// <summary>
        /// Get the number of tags
        /// </summary>
        int TagCount { get; }
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
    public class FindTaggedPagesModel : WindowViewModelBase, ITagSearchModel
    {
        internal static readonly PropertyChangedEventArgs PAGE_COUNT = new PropertyChangedEventArgs("PageCount");
        internal static readonly PropertyChangedEventArgs TAG_COUNT = new PropertyChangedEventArgs("TagCount");
        internal static readonly PropertyChangedEventArgs CURRENT_PAGE_TITLE = new PropertyChangedEventArgs("CurrentPageTitle");
        internal static readonly PropertyChangedEventArgs CURRENT_TAGS = new PropertyChangedEventArgs("CurrentTags");
        
        // the collection of tags found on OneNote pages
        private FilterablePageCollection _searchResult ;

        // pages in the search result exposed to the UI
        private ObservableSortedList<HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel> _pages = new ObservableSortedList<HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel>();
        private CancellationTokenSource _cancelWorker = new CancellationTokenSource();
        private BlockingCollection<Action> _actions;

        private TextSplitter _highlighter;

        /// <summary>
        /// Process request asynchronously
        /// </summary>
        private void processActions()
        {
            while (true)
            {
                Action a = _actions.Take();
                try
                {
                    a();
                }
                catch (Exception e)
                {
                    Debug.Assert(false, e.ToString());
                    return; // that's it! We have to give up.
                }
            }
        }

        internal FindTaggedPagesModel(Microsoft.Office.Interop.OneNote.Application onenote, XMLSchema schema) : base (onenote,schema)
        {
            _searchResult = new FilterablePageCollection(OneNoteApp, OneNotePageSchema);
            _searchResult.Tags.CollectionChanged          += HandleTagCollectionChanges;
            _searchResult.FilteredPages.CollectionChanged += HandlePageCollectionChanges;

            CurrentPageTitle = Properties.Resources.TagSearch_CheckBox_Tracking_Text;
            // load the search history
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SearchHistory))
            {
                string[] searches = Properties.Settings.Default.SearchHistory.Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < searches.Length && i < Properties.Settings.Default.SearchHistory_Size; i++)
                {
                    SearchHistory.Add(searches[i].Trim());
                }
            }

            _actions = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
            TaskFactory tf = new TaskFactory(_cancelWorker.Token,TaskCreationOptions.LongRunning,TaskContinuationOptions.None,TaskScheduler.Default);
            tf.StartNew(processActions);
        }

        /// <summary>
        /// Collection of tags used in a OneNote hierarchy context (section, section group, notebook)
        /// </summary>
        public TagsAndPages ContextTags { get { return new TagsAndPages(OneNoteApp, OneNotePageSchema); } }

        /// <summary>
        /// FindTaggedPages pages matching a search criterion in the background.
        /// </summary>
        /// <param name="query">query. If null or empty just all page tags are collected</param>
        /// <param name="scope">Range of the search</param>
        /// <param name="continuationAction">a UI action to execute (in the UI thread) after completion of the search</param>
        internal void FindPagesAsync(string query, SearchScope scope, Action continuationAction)
        {
            string scopeID = String.Empty;
            switch (scope)
            {
                case SearchScope.Notebook:
                    scopeID = CurrentNotebookID;
                    break;
                case SearchScope.SectionGroup:
                    scopeID = CurrentSectionGroupID;
                    break;
                case SearchScope.Section:
                    scopeID = CurrentSectionID;
                    break;
            }

            if (!string.IsNullOrEmpty(query))
            {
                query = query.Trim().Replace(',', ' ');
                SearchHistory.Remove(query);
                SearchHistory.Insert(0, query);
                // update settings
                StringBuilder history = new StringBuilder();
                for (int i = 0; i < SearchHistory.Count() && i < Properties.Settings.Default.SearchHistory_Size; i++)
                {
                    if (history.Length > 0)
                    {
                        history.Append(',');
                    }
                    history.Append(SearchHistory[i]);
                }
                Properties.Settings.Default.SearchHistory = history.ToString();

                // construct the query pattern
                string[] words = query.Split(new char[] { ',', ' ', ':', ';' }, StringSplitOptions.RemoveEmptyEntries);

                _highlighter = new TextSplitter(from w in words select w.Replace("'", "").Replace("\"", ""));
            }
            else
            {
                _highlighter = new TextSplitter();
            }

            _actions.Add(() => _searchResult.Find(query, scopeID, includeUnindexedPages:false));


            _actions.Add(() => Dispatcher.Invoke(UpdateFilterSelectionAction));
            _actions.Add(() => Dispatcher.Invoke(continuationAction));
        }

        private void UpdateFilterSelectionAction()
        {
            foreach (string filterTag in _searchResult.Filter)
            {
                TagSelectorModel mdl;
                if (Tags.TryGetValue(filterTag, out mdl))
                {
                    mdl.IsChecked = true;
                }
            }
        }

        internal void ClearTagFilterAsync(Action continuationAction)
        {
            _actions.Add(() => _searchResult.ClearTagFilter());
            _actions.Add(() => Dispatcher.Invoke(continuationAction));
        }

        internal void AddTagToFilterAsync(TagPageSet tag)
        {
            _actions.Add(() => _searchResult.AddTagToFilter(tag.TagName));
        }

        internal void RemoveTagFromFilterAsync(TagPageSet tag)
        {
            _actions.Add(() => _searchResult.RemoveTagFromFilter(tag.TagName));
        }

        /// <summary>
        ///  Get the default scope
        /// </summary>
        public SearchScope DefaultScope
        {
            get
            {
                return (SearchScope)Properties.Settings.Default.DefaultScope;
            }
        }

        #region ITagSearchModel

        // Collection of previous searches
        private ObservableCollection<string> _searchHistory = new ObservableCollection<string>();
        /// <summary>
        /// Get the list of previous searches.
        /// </summary>
        public ObservableCollection<string> SearchHistory
        {
            get
            {
                return _searchHistory;
            }
        }

        /// <summary>
        /// get the collection of pages having specific tag
        /// </summary>
        public ObservableSortedList <HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel> Pages
        {
            get { return _pages; }
        }

        private TagSource _tags = new TagSource();

        /// <summary>
        /// get the collection of tags 
        /// </summary>
        public TagSource Tags
        {
            get { return _tags; }
        }

        /// <summary>
        /// Get the nuber of OneNote pages in the search result.
        /// </summary>
        public int PageCount
        {
            get { return Pages.Count; }
        }
        /// <summary>
        /// Get the number of tags used on pages in the search result.
        /// </summary>
        public int TagCount
        {
            get { return Tags.Count; }
        }

        string _currentPageTitle;

        /// <summary>
        /// 
        /// </summary>
        public string CurrentPageTitle
        {
            get { return _currentPageTitle; }
            set
            {
                _currentPageTitle = value;
                fireNotifyPropertyChanged(CURRENT_PAGE_TITLE);
            }
        }

        #endregion ITagSearchModel

        #region tag tracking
        IEnumerable<string> _currentTags;
        /// <summary>
        /// get the set of tags associated with the current page
        /// </summary>
        internal IEnumerable<string> CurrentTags
        {
            get
            {
                return _currentTags;
            }
            private set
            {
                _currentTags = value;
                fireNotifyPropertyChanged(CURRENT_TAGS);
            }
        }
        Timer _tracker;
        string _currentPageID = string.Empty;
        internal void BeginTracking()
        {
            if (_tracker == null)
            {
                _tracker = new Timer(TrackCurrentPage, null, 1000, 1000);
            }
        }

        internal void EndTracking()
        {
            if (_tracker != null)
            {
                _currentPageID = string.Empty;
                var waitHandle = new ManualResetEvent(false);
                _tracker.Dispose(waitHandle);
                waitHandle.WaitOne();
                _tracker = null;
            }
            CurrentPageTitle = Properties.Resources.TagSearch_CheckBox_Tracking_Text;
        }

        private void TrackCurrentPage(object state)
        {
            if (!_currentPageID.Equals(CurrentPageID))
            {
                _currentPageID = CurrentPageID;
                TagsAndPages tap = new TagsAndPages(OneNoteApp, OneNotePageSchema);
                tap.GetPagesFromHierarchy(TagContext.CurrentNote);
                TaggedPage tp = tap.Pages.Values.FirstOrDefault();
                if (tp != null)
                {
                    Dispatcher.Invoke(() => {
                                                CurrentTags = from t in tp.Tags select t.TagName;
                                                CurrentPageTitle = tp.Title;
                                            });
                }
            }
        }
        #endregion tag tracking

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            TagSelectorModel mdl = sender as TagSelectorModel;
            if (mdl != null && args == TagSelectorModel.IS_CHECKED)
            {
                if (mdl.IsChecked)
                {
                    AddTagToFilterAsync(mdl.Tag);
                }
                else
                {
                    RemoveTagFromFilterAsync(mdl.Tag);
                }
            }
        }
        private void HandleTagCollectionChanges(object sender, NotifyDictionaryChangedEventArgs<string, TagPageSet> e)
        {
            Action a = null;

            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                    a = () => Tags.AddAll(from i in e.Items select new TagSelectorModel(i, OnModelPropertyChanged)); ;
                    break;
                case NotifyDictionaryChangedAction.Remove:
                    a = () => Tags.RemoveAll(from i in e.Items select i.Key);
                    break;
                case NotifyDictionaryChangedAction.Reset:
                    a = () => Tags.Clear();
                    break;
            }
            if (a != null)
            {
                Dispatcher.Invoke(new Action(() => { a(); fireNotifyPropertyChanged(TAG_COUNT); }));
            }
        }

        private void HandlePageCollectionChanges(object sender, NotifyDictionaryChangedEventArgs<string, TaggedPage> e)
        {
            Action a = null;
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                    a = () => _pages.AddAll(from i in e.Items select new HitHighlightedPageLinkModel(i, _highlighter, OneNoteApp));
                    break;
                case NotifyDictionaryChangedAction.Remove:
                    a = () => _pages.RemoveAll(from i in e.Items select i.Key);
                    break;
                case NotifyDictionaryChangedAction.Reset:
                    a = () => _pages.Clear();
                    break;
            }
            if (a != null)
            {
                Dispatcher.Invoke(new Action(() => { a(); fireNotifyPropertyChanged(PAGE_COUNT); }));
            }
        }

        internal void NavigateTo(string pageID)
        {
            OneNoteApp.NavigateTo(pageID);
        }
        /// <summary>
        /// Dispose the view model.
        /// </summary>
        public override void Dispose()
        {
            _cancelWorker.Cancel();
            EndTracking();
            base.Dispose();
        }
    }

    /// <summary>
    /// A collection of tags represented by data context objects implementing the
    /// <see cref="IHighlightableTagDataContext"/> contract. 
    /// </summary>
    public class TagSource : ObservableSortedList<TagModelKey, string, TagSelectorModel>, ITagSource
    {
        #region ITagSource
        /// <summary>
        /// Get the sequence of tags in this collection.
        /// </summary>
        public IEnumerable<IHighlightableTagDataContext> TagDataContextCollection
        {
            get { return Values; }
        }
        #endregion
    }
}
