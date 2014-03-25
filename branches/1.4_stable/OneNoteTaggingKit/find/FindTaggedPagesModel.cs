using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace WetHatLab.OneNote.TaggingKit.find
{
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
    /// UI Facade providing hit higlighted page titles for a <see cref="TaggedPage"/> objects.
    /// </summary>
    public class HitHighlightedPage
    {
        private TaggedPage _page;

        private TextBlock _hithighlightedTitel;
        private TextBlock _hithighlightedTooltip;
        internal HitHighlightedPage(TaggedPage page)
        {
            _page = page;

            _hithighlightedTitel = new TextBlock();
            _hithighlightedTitel.TextTrimming = System.Windows.TextTrimming.CharacterEllipsis;

            // build the highlightes inline Text
            if (_page.TitleHits != null)
            {
                int afterLastHighlight = 0;
                foreach (Match m in _page.TitleHits)
                {
                    // reate a plain run between the last highlight anf this highlight
                    if (m.Index > afterLastHighlight)
                    {
                        _hithighlightedTitel.Inlines.Add(new Run(Title.Substring(afterLastHighlight, m.Index - afterLastHighlight)));
                    }
                    // add a highlighted Run
                    Run r = new Run(Title.Substring(m.Index,m.Length));
                    r.Background=Brushes.Yellow;
                    _hithighlightedTitel.Inlines.Add(r);
                    afterLastHighlight = m.Index + m.Length;
                }
                // add remaining plain text
                if (afterLastHighlight < Title.Length)
                {
                    _hithighlightedTitel.Inlines.Add(new Run(Title.Substring(afterLastHighlight, Title.Length - afterLastHighlight)));
                }
            }
            else
            {
                _hithighlightedTitel.Inlines.Add(new Run(Title));
            }

            _hithighlightedTooltip = new TextBlock();
            _hithighlightedTooltip.TextTrimming = _hithighlightedTitel.TextTrimming;

            foreach (Run r in _hithighlightedTitel.Inlines)
            {
                Run newR = new Run(r.Text);
                newR.Background = r.Background;
                _hithighlightedTooltip.Inlines.Add(newR);
            }
        }

        /// <summary>
        /// Get the page's unique ID
        /// </summary>
        public String ID
        {
            get
            {
                return _page.ID;
            }
        }

        /// <summary>
        /// Get the page's plain (unhighlighted) title.
        /// </summary>
        public string Title
        {
            get
            {
                return _page.Title;
            }
        }

        /// <summary>
        /// Get the page's hit highlighted title.
        /// </summary>
        public TextBlock HitHighlightedTitle
        {
            get
            {
                return _hithighlightedTitel;
            }
        }

        /// <summary>
        /// Get the page's hit highlighted tooltip
        /// </summary>
        public TextBlock HitHighlightedTooltip
        {
            get
            {
                return _hithighlightedTooltip;
            }
        }

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
    public class FindTaggedPagesModel : System.Windows.DependencyObject, ITagSearchModel, IDisposable
    {
        public static readonly System.Windows.DependencyProperty PageCountProperty = System.Windows.DependencyProperty.Register("PageCount", typeof(int), typeof(FindTaggedPagesModel), new System.Windows.PropertyMetadata(0));

        public static readonly System.Windows.DependencyProperty TagCountProperty = System.Windows.DependencyProperty.Register("TagCount", typeof(int), typeof(FindTaggedPagesModel), new System.Windows.PropertyMetadata(0));
        
        private Application _onenote;

        private Window _currentWindow;

        private Dispatcher _dispatcher;

        private IList<TagSearchScope> _scopes;

        private TagSearchScope _selectedScope;

        // the collection of tags found on pages
        private FilterablePageCollection _searchResult ;

        // pages in the search result exposed to the UI
        private ObservableCollection<HitHighlightedPage> _pages = new ObservableCollection<HitHighlightedPage>();

        private CancellationTokenSource _cancelWorker = new CancellationTokenSource();
        private BlockingCollection<Action> _actions;

        // Collection of previous searches
        private ObservableCollection<string> _searchHistory = new ObservableCollection<string>();
            
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
                    return; // that's it
                }
            }
        }

        internal FindTaggedPagesModel(Application onenote, Window currentWindow, XMLSchema schema)
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
            _searchResult.TagCollectionChanged += ForwardTagCollectionChanges;
            _searchResult.PageCollectionChanged += HandlePageCollectionChanges;

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
            TaskFactory tf = new TaskFactory();
            tf.StartNew(processActions, _cancelWorker.Token);
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
                query = query.Trim().Replace(',',' ');
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
            }

            _actions.Add(() => _searchResult.Find(query, scopeID));
            _actions.Add(() => Dispatcher.Invoke(continuationAction));
        }

        internal void ClearTagFilterAsync(Action continuationAction)
        {
            _actions.Add(() => _searchResult.ClearTagFilter());
            _actions.Add(() => Dispatcher.Invoke(continuationAction));
        }

        internal void AddTagToFilterAsync(TagPageSet tag, Action continuationAction)
        {
            _actions.Add(() => _searchResult.ApplyTagFilter(tag));
            _actions.Add(() => Dispatcher.Invoke(continuationAction));
        }

        internal void RemoveTagFromFilterAsync(TagPageSet tag, Action continuationAction)
        {
            _actions.Add(() => _searchResult.UnapplyTagFilter(tag));
            _actions.Add(() => Dispatcher.Invoke(continuationAction));
        }

        #region ITagSearchModel
        /// <summary>
        /// Get the list of scopes available for collecing tagged pages.
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

        private void HandlePageCollectionChanges(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            _pages.Insert(i + e.NewStartingIndex, new HitHighlightedPage((TaggedPage)e.NewItems[i]));
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            Debug.Assert(_pages[e.OldStartingIndex].Title.Equals(((TaggedPage)e.OldItems[i]).Title),
                                        string.Format("Removing wrong page at {0}! Want to remove {1}. Actually removed {2}",
                                                      e.OldStartingIndex,
                                                      _pages[e.OldStartingIndex].Title,
                                                      ((TaggedPage)e.OldItems[i]).Title));
                            _pages.RemoveAt(e.OldStartingIndex);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        _pages.Clear();
                        break;
                }
                ObservableSortedList<TaggedPage> pages = sender as ObservableSortedList<TaggedPage>;
                SetValue(PageCountProperty,pages.Count);
            }));
        }

        private void ForwardTagCollectionChanges(object sender, NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => {
                                                 if (TagCollectionChanged != null)
                                                 {
                                                     TagCollectionChanged(sender, e);
                                                 }
                                                 ObservableSortedList<TagPageSet> tags = sender as ObservableSortedList<TagPageSet>;
                                                 SetValue(TagCountProperty, tags.Count);
                                               } ));
        }

        /// <summary>
        /// Fired when the collection of tags available for filtering changes
        /// </summary>
        public event NotifyCollectionChangedEventHandler TagCollectionChanged;

        /// <summary>
        /// get the collection of pages having specific tag
        /// </summary>
        public ObservableCollection<HitHighlightedPage> Pages
        {
            get { return _pages; }
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
            _searchResult.Dispose();
        }
        #endregion IDisposable
    }
}
