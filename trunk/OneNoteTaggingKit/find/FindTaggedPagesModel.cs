using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        /// Get a list of scopes available for finding tagged pages.
        /// </summary>
        IList<TagSearchScope> Scopes { get; }
        /// <summary>
        /// Get or set the scope to use
        /// </summary>
        TagSearchScope SelectedScope { get; set; }
        /// <summary>
        /// Get the collection of pages with particular tags
        /// </summary>
        ObservableSortedList <HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel> Pages { get; }
        ObservableSortedList <TagModelKey, string, TagSelectorModel> Tags { get; }

        int PageCount { get; }
        int TagCount { get; }
    }

    /// <summary>
    /// Enumeration of scopes to search for tagged pages
    /// </summary>
    public enum SearchScope
    {
        /// <summary>
        /// OneNote section
        /// </summary>
        Section      = 0,
        /// <summary>
        /// OneNote section group
        /// </summary>
        SectionGroup = 1,
        /// <summary>
        /// OneNote notebook
        /// </summary>
        Notebook     = 2,
        /// <summary>
        /// All notebooks open in OneNote
        /// </summary>
        AllNotebooks = 3,
    }

    /// <summary>
    /// Search Scope UI facade
    /// </summary>
    public class TagSearchScope
    {
        /// <summary>
        /// Get or set the search scope
        /// </summary>
       public SearchScope Scope {get; set;}
        /// <summary>
        /// get or set the display label.
        /// </summary>
       public string ScopeLabel { get; set; }
    }

    /// <summary>
    /// View model backing the UI to find tagged pages.
    /// </summary>
    /// <remarks>Search queries are run in the background</remarks>
    public class FindTaggedPagesModel : DependencyObject, ITagSearchModel, IDisposable, INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs PAGE_COUNT = new PropertyChangedEventArgs("PageCount");
        private static readonly PropertyChangedEventArgs TAG_COUNT = new PropertyChangedEventArgs("TagCount");

        private Microsoft.Office.Interop.OneNote.Application _onenote;

        private Microsoft.Office.Interop.OneNote.Window _currentWindow;

        private IList<TagSearchScope> _scopes;

        private TagSearchScope _selectedScope;

        // the collection of tags found on OneNote pages
        private FilterablePageCollection _searchResult ;

        // pages in the search result exposed to the UI
        private ObservableSortedList<HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel> _pages = new ObservableSortedList<HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel>();
        private ObservableSortedList<TagModelKey, string, TagSelectorModel> _tags = new ObservableSortedList<TagModelKey, string, TagSelectorModel>();
        
        private CancellationTokenSource _cancelWorker = new CancellationTokenSource();
        private BlockingCollection<Action> _actions;

        // Collection of previous searches
        private ObservableCollection<string> _searchHistory = new ObservableCollection<string>();

        private Regex _queryPattern;

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

        internal FindTaggedPagesModel(Microsoft.Office.Interop.OneNote.Application onenote, Microsoft.Office.Interop.OneNote.Window currentWindow, XMLSchema schema)
        {
            _onenote = onenote;
            _currentWindow = currentWindow;
            _scopes = new TagSearchScope[4];
            _scopes[0] = new TagSearchScope {
                                              Scope = SearchScope.Section,
                                              ScopeLabel = Properties.Resources.TagSearch_Scope_Section_Label
                                            };
            _scopes[1] = new TagSearchScope {
                                                Scope = SearchScope.SectionGroup,
                                                ScopeLabel = Properties.Resources.TagSearch_Scope_SectionGroup_Label
                                            };
            _scopes[2] = new TagSearchScope {
                                                Scope = SearchScope.Notebook,
                                                ScopeLabel = Properties.Resources.TagSearch_Scope_Notebook_Label
                                            };
            _scopes[3] = new TagSearchScope {
                                                Scope = SearchScope.AllNotebooks,
                                                ScopeLabel = Properties.Resources.TagSearch_Scope_AllNotebooks_Label
                                            };

            _selectedScope = _scopes[Properties.Settings.Default.DefaultScope];
            _searchResult = new FilterablePageCollection(_onenote,schema);
            _searchResult.Tags.CollectionChanged += HandleTagCollectionChanges;
            _searchResult.Pages.CollectionChanged += HandlePageCollectionChanges;

            // load the search history
            if (!string.IsNullOrEmpty(Properties.Settings.Default.SearchHistory))
            {
                string[] searches = Properties.Settings.Default.SearchHistory.Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < searches.Length && i < Properties.Settings.Default.SearchHistory_Size; i++)
                {
                    _searchHistory.Add(searches[i].Trim());
                }
            }

            _actions = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
            TaskFactory tf = new TaskFactory(_cancelWorker.Token,TaskCreationOptions.LongRunning,TaskContinuationOptions.None,TaskScheduler.Default);
            tf.StartNew(processActions);
        }

        private void fireNotifyPropertyChanged(PropertyChangedEventArgs propArgs)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, propArgs);
            }
        }
        /// <summary>
        /// Find pages matching a search criterion in the background.
        /// </summary>
        /// <param name="query">query. If null or empty just all page tags are collected</param>
        /// <param name="continuationAction">a UI action to execute (in the UI thread) after completion of the search</param>
        internal void FindPagesAsync(string query, Action continuationAction)
        {
            string scopeID = String.Empty;
            switch (_selectedScope.Scope)
            {
                case SearchScope.Notebook:
                    scopeID = _currentWindow.CurrentNotebookId;
                    break;
                case SearchScope.SectionGroup:
                    scopeID = _currentWindow.CurrentSectionGroupId;
                    break;
                case SearchScope.Section:
                    scopeID = _currentWindow.CurrentSectionId;
                    break;
            }

            if (!string.IsNullOrEmpty(query))
            {
                query = query.Trim().Replace(',', ' ');
                _searchHistory.Remove(query);
                _searchHistory.Insert(0, query);
                // update settings
                StringBuilder history = new StringBuilder();
                for (int i = 0; i < _searchHistory.Count() && i < Properties.Settings.Default.SearchHistory_Size; i++)
                {
                    if (history.Length > 0)
                    {
                        history.Append(',');
                    }
                    history.Append(_searchHistory[i]);
                }
                Properties.Settings.Default.SearchHistory = history.ToString();

                // construct the query pattern
                string[] words = query.Split(new char[] { ',', ' ', ':', ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < words.Length; i++)
                {
                    words[i] = words[i].Replace("'", "").Replace("\"", "");
                }

                string pattern = string.Join("|", words);
                _queryPattern = new Regex( string.Join("|", words),RegexOptions.IgnoreCase);
            }
            else
            {
                _queryPattern = null;
            }

            _actions.Add(() => _searchResult.Find(query, scopeID));
            _actions.Add(() => Dispatcher.Invoke(continuationAction));
        }

        internal void ClearTagFilterAsync(Action continuationAction)
        {
            _actions.Add(() => _searchResult.ClearTagFilter());
            _actions.Add(() => Dispatcher.Invoke(continuationAction));
        }

        internal void AddTagToFilterAsync(TagPageSet tag)
        {
            _actions.Add(() => _searchResult.ApplyTagFilter(tag));
        }

        internal void RemoveTagFromFilterAsync(TagPageSet tag)
        {
            _actions.Add(() => _searchResult.UnapplyTagFilter(tag));
        }

        #region ITagSearchModel
        /// <summary>
        /// Get the list of scopes available for collecting tagged pages.
        /// </summary>
        public IList<TagSearchScope> Scopes
        {
            get
            {
                return _scopes;
            }
        }

        /// <summary>
        ///  get or set the scope currently used for finding tags
        /// </summary>
        public TagSearchScope SelectedScope
        {
            get
            {
                return _selectedScope;
            }
            set
            {
                if (_selectedScope.Scope != value.Scope)
                {
                    _selectedScope = value;
                    Properties.Settings.Default.DefaultScope = (int)_selectedScope.Scope;
                }
            }
        }

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

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            TagSelectorModel mdl = sender as TagSelectorModel;
            if (mdl != null && args.PropertyName.Equals("IsChecked"))
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
                    a = () => _tags.AddAll(from i in e.Items select new TagSelectorModel(i, OnModelPropertyChanged));;
                    break;
                case NotifyDictionaryChangedAction.Remove:
                    a = () =>_tags.RemoveAll(from i in e.Items select i.Key);
                    break;
                case NotifyDictionaryChangedAction.Reset:
                    a = () => _tags.Clear();
                    break;
            }
           if (a != null)
           {
               Dispatcher.Invoke(new Action(() => { a() ; fireNotifyPropertyChanged(TAG_COUNT);}));
           }
        }

        private void HandlePageCollectionChanges(object sender, NotifyDictionaryChangedEventArgs<string, TaggedPage> e)
        {
            Action a = null;
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                    a = () => _pages.AddAll(from i in e.Items select new HitHighlightedPageLinkModel(i,_queryPattern));
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

        /// <summary>
        /// get the collection of pages having specific tag
        /// </summary>
        public ObservableSortedList <HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel> Pages
        {
            get { return _pages; }
        }
       
        /// <summary>
        /// get the collection of tags 
        /// </summary>
        public ObservableSortedList <TagModelKey, string, TagSelectorModel> Tags
        {
            get { return _tags; }
        }

        public int PageCount
        {
            get { return Pages.Count; }
        }
        public int TagCount
        {
            get { return _tags.Count; }
        }
        #endregion ITagSearchModel

        internal void NavigateTo(string pageID)
        {
            _onenote.NavigateTo(pageID);
        }
        #region IDisposable
        /// <summary>
        /// Dispose the view model.
        /// </summary>
        public void Dispose()
        {
            _cancelWorker.Cancel();
        }
        #endregion IDisposable

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion INotifyPropertyChanged
    }
}
