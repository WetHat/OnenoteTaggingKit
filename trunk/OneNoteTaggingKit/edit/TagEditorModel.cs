using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WetHatLab.OneNote.TaggingKit.common;

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
        ObservableSortedList<string,string,SimpleTagButtonModel> PageTags { get; }

        /// <summary>
        /// Get the collection of all knows tags.
        /// </summary>
        ObservableCollection<string> KnownTags { get; }

        /// <summary>
        /// Get the addin version.
        /// </summary>
        string AddinVersion { get; }
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
    public class TagEditorModel : DependencyObject, ITagEditorModel
    {
        private Microsoft.Office.Interop.OneNote.Application _OneNote;
        private XMLSchema _schema;

        private TaskFactory _taskFactory = new TaskFactory();

        private ObservableSortedList<string,string,SimpleTagButtonModel> _pageTags = new ObservableSortedList<string,string,SimpleTagButtonModel>();
        private ObservableCollection<string> _knownTags = new ObservableCollection<string>();

        private OneNotePageProxy _currentPage;

        /// <summary>
        /// Cancellation token souce for all workers
        /// </summary>
        private CancellationTokenSource _cancelWorker = new CancellationTokenSource();

        private Task _tagLoader;
 
        internal TagEditorModel(Microsoft.Office.Interop.OneNote.Application onenote, string pageID,XMLSchema schema)
        {
            _OneNote = onenote;
            _schema = schema;
        }

        #region ITagEditorModel
        /// <summary>
        /// Get the collection of page tags.
        /// </summary>
        public ObservableSortedList<string,string,SimpleTagButtonModel> PageTags
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
        /// Get the version of the addin.
        /// </summary>
        public string AddinVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }
        #endregion ITagEditorModel

        internal Task RefreshPageTagsAsync()
        {
            if (_tagLoader != null  && _tagLoader.Status == TaskStatus.Running)
            {
                if (_currentPage == null || !_currentPage.PageID.Equals(_OneNote.Windows.CurrentWindow.CurrentPageId))
                {
                    // abandon previous load request
                    _cancelWorker.Cancel();
                }
            }
            SaveChangesAsync(); // persist any pending changes
            _tagLoader = _taskFactory.StartNew(LoadPageWorker, _cancelWorker.Token);
            return _tagLoader;
        }

        private void LoadPageWorker()
        {
            if (_currentPage == null || !_currentPage.PageID.Equals(_OneNote.Windows.CurrentWindow.CurrentPageId))
            {
                _currentPage = new OneNotePageProxy(_OneNote, _OneNote.Windows.CurrentWindow.CurrentPageId, _schema);
            }
            
            string[] tagsuggestions = OneNotePageProxy.ParseTags(Properties.Settings.Default.KnownTags);
            Dispatcher.Invoke(new Action(() =>
            {
                _pageTags.Clear();
                _pageTags.AddAll(from t in _currentPage.PageTags select new SimpleTagButtonModel(t));
                _knownTags.Clear();
                foreach (string t in tagsuggestions)
                {
                    _knownTags.Add(t);
                }
            }));
        }

        internal Task SaveChangesAsync()
        {
            // pass tags and current page as parameters so that the undelying objects can further be modified in the foreground
            return _taskFactory.StartNew(() => SaveChangesWorker(_currentPage, (from st in _pageTags.Values select st.Key).ToArray()));
        }

        private void SaveChangesWorker(OneNotePageProxy page, string[] tags)
        {
            if (page != null)
            {
                page.PageTags = tags;
                page.Update();

                // update suggestions
                if (tags != null)
                {
                    Properties.Settings.Default.KnownTags = string.Join(",", _knownTags.Union(tags));
                }
            }
        }
    }
}
