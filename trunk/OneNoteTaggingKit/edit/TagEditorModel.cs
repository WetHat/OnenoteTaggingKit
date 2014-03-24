using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// Contract used by the tag editor view model
    /// </summary>
    /// <seealso cref="WetHatLab.OneNote.TaggingKit.edit.TagEditor"/>
    internal interface ITagEditorModel
    {
        /// <summary>
        /// Get the collection of tags on current OneNote page.
        /// </summary>
        ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> PageTags { get; }

        /// <summary>
        /// Get the collection of all knows tags.
        /// </summary>
        ObservableCollection<string> KnownTags { get; }

        /// <summary>
        /// Get the title of the currently loaded page
        /// </summary>
        string PageTitle { get;  }

        /// <summary>
        /// Get a marker to indicate unsaved changes
        /// </summary>
        string ModificationMarker { get; }
    }

    /// <summary>
    /// View Model to support the tag editor dialog.
    /// </summary>
    /// <remarks>Maintains a data models for:
    /// <list type="bullet">
    ///   <item>Tags on the current page</item>
    ///   <item>similar pages (based on the tags they share with the current page)</item>
    /// </list>
    /// </remarks>
    public class TagEditorModel : DependencyObject, ITagEditorModel, INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs PAGE_TITLE = new PropertyChangedEventArgs("PageTitle");
        private static readonly PropertyChangedEventArgs MODIFICATION_MARKER = new PropertyChangedEventArgs("ModificationMarker");

        private Microsoft.Office.Interop.OneNote.Application _OneNote;
        private XMLSchema _schema;

        private ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> _pageTags = new ObservableSortedList<TagModelKey, string, SimpleTagButtonModel>();
        
        private ObservableCollection<string> _knownTags = new ObservableCollection<string>();

        private AggregatedPageCollection _pageAggregation;

        /// <summary>
        /// The actual page 
        /// </summary>
        private OneNotePageProxy _currentActualPage;
        /// <summary>
        /// page from the database creted by 'find'
        /// </summary>
        private TaggedPage _currentPage;

        /// <summary>
        /// True if the tags on the current page were changed.
        /// </summary>
        private bool _tagsChanged;

        /// <summary>
        /// Cancellation token souce for all workers
        /// </summary>
        private CancellationTokenSource _cancelTagDatabaseLoader = new CancellationTokenSource();
        private Task<IList<String>> _tagDatabaseLoader;

        private CancellationTokenSource _cancelPageUpdater = new CancellationTokenSource();
        private Task _pageUpdater;

        internal TagEditorModel(Microsoft.Office.Interop.OneNote.Application onenote,XMLSchema schema)
        {
            _OneNote = onenote;
            _schema = schema;
            _pageAggregation = new AggregatedPageCollection(_OneNote, _schema);
            _pageAggregation.AggregationTags.CollectionChanged += OnPageTagsChanged;
        }

        /// <summary>
        /// Determine if the currently loaded page is the same as the current OneNote page.
        /// </summary>
        internal bool InSync
        {
            get
            {
                return _currentPage != null && _OneNote.Windows.CurrentWindow.CurrentPageId.Equals(_currentPage.ID);
            }
        }

        /// <summary>
        /// Determine if the currently loaded page has unsaved changes to tags.
        /// </summary>
        internal bool HasUnsavedChanges
        {
            get { return _tagsChanged; }
        }

        internal async Task UpdatePageAsync(bool force)
        {
            string id = _OneNote.Windows.CurrentWindow.CurrentPageId;
            
            // terminate any running workers
            if (_pageUpdater != null && _pageUpdater.Status == TaskStatus.Running)
            {
                _cancelPageUpdater.Cancel();
            }

            _pageUpdater = Task.Run(() => UpdatePageAction(id, force), _cancelPageUpdater.Token);
            await _pageUpdater;

            firePropertyChangedEvent(PAGE_TITLE);
        }

        private void UpdatePageAction(string pageID, bool force)
        {
            if (!InSync || force)
            {
                _currentActualPage = null;
                _currentPage = null;
                _tagsChanged = false;
                firePropertyChangedEvent(MODIFICATION_MARKER);
                lock (_pageAggregation)
                {
                    // lookup page from database
                   if (!_pageAggregation.Pages.TryGetValue(pageID, out _currentPage) || force)
                   {
                        // load the actual page.
                        _currentActualPage = new OneNotePageProxy(_OneNote, pageID, _schema);
                        // update the database
                        if (_currentPage != null)
                        { // make sure tags will be identical to the ones on the page
                            _currentPage.Tags.Clear();
                        }
                        _currentPage = _pageAggregation.TagPage(_currentActualPage.PageTags, pageID, _currentActualPage.Title);
                    }

                    // update the aggregation
                    _pageAggregation.AggregationTags.IntersectWith(_currentPage.Tags);
                    _pageAggregation.AggregationTags.UnionWith(_currentPage.Tags);
                }
            }
        }


        /// <summary>
        /// Track changes in the page tags and update the collection of <see cref="SimpleTagButtonModel"/> objects in the UI thread
        /// accordingly
        /// </summary>
        /// <param name="sender">event source (a collection of pages)</param>
        /// <param name="e">event details</param>
        private void OnPageTagsChanged(object sender, NotifyDictionaryChangedEventArgs<string, TagPageSet> e)
        {
            Action uiAction = null;
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:

                    LinkedList<SimpleTagButtonModel> btnModels = new LinkedList<SimpleTagButtonModel>();
                    foreach (var item in e.Items)
                    {
                        btnModels.AddLast(new SimpleTagButtonModel(item.TagName));
                    }
                    uiAction = new Action(() => {
                        _pageTags.AddAll(btnModels);
                                                });
                    break;
                case NotifyDictionaryChangedAction.Remove:
                    LinkedList<string> toRemove = new LinkedList<string>();
                    foreach (var item in e.Items)
                    {
                        toRemove.AddLast(item.TagName);
                    }
                    uiAction = new Action(() =>
                    {
                        _pageTags.RemoveAll(toRemove);
                    });
                    break;
                case NotifyDictionaryChangedAction.Reset:
                    uiAction = new Action(() => {
                        _pageTags.Clear();
                                                });
                    break;
            }

            Dispatcher.Invoke(uiAction);
        }

        #region ITagEditorModel
        /// <summary>
        /// Get the collection of tags on the current page.
        /// </summary>
        public ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> PageTags
        {
            get { return _pageTags; }
        }

        /// <summary>
        /// Get the collection of all tags known to the addin.
        /// </summary>
        /// <remarks>These tags are used to suggest page tags</remarks>
        public ObservableCollection<string> KnownTags
        {
            get { return _knownTags; }
        }

        /// <summary>
        /// Get the title of the current page.
        /// </summary>
        public string PageTitle
        {
            get
            {
                return _currentPage != null ? _currentPage.Title :  Properties.Resources.TagEditor_Windows_Title;
            }
        }

        public string ModificationMarker
        {
            get { return HasUnsavedChanges ? " *" : String.Empty; }
        }

        #endregion ITagEditorModel

        /// <summary>
        /// Associate a tag with the current page
        /// </summary>
        /// <param name="tags">tags to apply</param>
        internal Task ApplyPageTagsAsync(IEnumerable<string> tags)
        {
            return Task.Run(() =>
            {
                lock (_pageAggregation)
                {
                    IEnumerable<TagPageSet> applied = _pageAggregation.TagPage(tags, _currentPage);
                    _tagsChanged = true;
                    _pageAggregation.AggregationTags.UnionWith(applied);
                }
            }).ContinueWith((x) => firePropertyChangedEvent(MODIFICATION_MARKER), TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Dissassociate a tag with the current page.
        /// </summary>
        /// <param name="tagname">name of the tag</param>
        internal Task UnapplyPageTagAsync(string tagname)
        {
            return Task.Run(() =>
                {
                    lock (_pageAggregation)
                    {
                        IEnumerable<TagPageSet> removed = _pageAggregation.UntagPage(tagname, _currentPage);
                        _tagsChanged = true;
                        _pageAggregation.AggregationTags.ExceptWith(removed);
                    }
                }).ContinueWith((x) => firePropertyChangedEvent(MODIFICATION_MARKER), TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Asnchronously load all tags used anywhere on OneNote pages.
        /// </summary>
        /// <returns>task object</returns>
        internal async Task LoadTagAndPageDatabaseAsync()
        {
            if (_tagDatabaseLoader != null && _tagDatabaseLoader.Status == TaskStatus.Running)
            {
                _cancelTagDatabaseLoader.Cancel();
            }
            _tagDatabaseLoader = Task.Run(() => TagDatabaseLoaderAction(), _cancelTagDatabaseLoader.Token);
            IList<string> sortedSuggestions = await _tagDatabaseLoader;
            _knownTags.Clear();
            foreach (string t in sortedSuggestions)
            {
                _knownTags.Add(t);
            }
        }

        /// <summary>
        /// Load the tag database in the background
        /// </summary>
        private IList<string> TagDatabaseLoaderAction()
        {
            lock (_pageAggregation)
            {
                _pageAggregation.Find(String.Empty);
            }

            List<string> sortedSuggestions = new List<string>();

            foreach (string t in OneNotePageProxy.ParseTags(Properties.Settings.Default.KnownTags))
            {
                int index = sortedSuggestions.BinarySearch(t);
                if (index < 0)
                {
                    sortedSuggestions.Insert(~index, t);
                }
            }
            return sortedSuggestions;
        }

        internal async Task SaveChangesAsync()
        {
            if (HasUnsavedChanges)
            {
                _tagsChanged = false;
                firePropertyChangedEvent(MODIFICATION_MARKER);

                // pass tags and current page as parameters so that the undelying objects can further be modified in the foreground

                string[] pageTags = (from t in _pageTags.Values select t.TagName).ToArray();
                await Task.Run(() => SaveChangesAction(pageTags));

                // update suggestions
                if (pageTags != null && pageTags.Length > 0)
                {
                    List<string> sortedSuggestions = new List<string>(_knownTags);
                    foreach (var t in pageTags)
                    {
                        int index = sortedSuggestions.BinarySearch(t);
                        if (index < 0)
                        {
                            sortedSuggestions.Insert(~index, t);
                            _knownTags.Insert(~index, t);
                        }
                    }

                    Properties.Settings.Default.KnownTags = string.Join(",", sortedSuggestions);
                }
            }
        }

        private void SaveChangesAction(string[] tags)
        {
            OneNotePageProxy page = _currentActualPage;
            if ( page == null)
            {
                // load the real page
                page = new OneNotePageProxy(_OneNote, _currentPage.ID, _schema);
            }

            page.PageTags = tags;
            page.Update();
        }

        private void firePropertyChangedEvent(PropertyChangedEventArgs args)
        {
            PropertyChanged(this, args);
        }
#region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
#endregion
    }
}
