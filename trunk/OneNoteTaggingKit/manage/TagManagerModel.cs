using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WetHatLab.OneNote.TaggingKit.collections;
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    ///  Contract for view models of the <see cref="WetHatLab.OneNote.TaggingKit.manage.TagManager"/> dialog
    /// </summary>
    /// <seealso cref="TagManager"/>

    internal interface ITagManagerModel
    {
        /// <summary>
        /// Get the collection of all tags used for suggestions.
        /// </summary>
        ObservableSortedList<RemovableTagModel> SuggestedTags { get; }

        /// <summary>
        /// Get the add-in version.
        /// </summary>
        string AddinVersion { get; }
    }

    /// <summary>
    /// View model backing the <see cref="TagManager"/> dialog.
    /// </summary>
    public class TagManagerModel : System.Windows.DependencyObject, ITagManagerModel, IDisposable
    {
        private ObservableSortedList<RemovableTagModel> _suggestedTags = new ObservableSortedList<RemovableTagModel>();
        private FilterablePageCollection _pages;
        private CancellationTokenSource _cancelFinder = new CancellationTokenSource();

        /// <summary>
        /// Create a new instance of the view model backing the <see cref="TagManager"/> dialog.
        /// </summary>
        /// <param name="onenote">OneNote application object</param>
        /// <param name="schema">current schema version</param>
        internal TagManagerModel(Application onenote, XMLSchema schema)
        {
            _pages = new FilterablePageCollection(onenote, schema);
        }

        /// <summary>
        /// Background worker method to collect the suggested tags by running a search
        /// against all open OneNote notebooks
        /// </summary>
        /// <param name="continuation"></param>
        private void LoadTagSuggestionsWorker(Action continuation)
        {
            _pages.Find(String.Empty, String.Empty);

            IEnumerable<RemovableTagModel> suggestions = from s in OneNotePageProxy.ParseTags(Properties.Settings.Default.KnownTags)
                                                         where !_pages.Tags.ContainsKey(s)
                                                         select new RemovableTagModel(new TagPageSet(s));

            IEnumerable<RemovableTagModel> tagsInUse = from t in _pages.Tags.Values select new RemovableTagModel(t);
            Dispatcher.Invoke(new Action(() =>
            {
                _suggestedTags.AddAll(tagsInUse);
                _suggestedTags.AddAll(suggestions);
                continuation();
            }));
        }

        /// <summary>
        /// Asynchronously load tag suggestions.
        /// </summary>
        /// <remarks>Runs a search for tags over all notebooks open in OneNote to collect tags in use.</remarks>
        /// <param name="continuation">continuation action to be run in the UI thread</param>
        internal void LoadTagSuggestionsAsync(Action continuation)
        {
            TaskFactory tf = new TaskFactory();
            tf.StartNew(() => LoadTagSuggestionsWorker(continuation), _cancelFinder.Token);
        }
        #region ITagManagerModel

        /// <summary>
        /// Get the collection of tags used for suggestions.
        /// </summary>
        /// <remarks>This collection includes all tags used on any OneNote pages and additional tags suggestions
        /// which were added manually</remarks>
        public ObservableSortedList<RemovableTagModel> SuggestedTags
        {
            get
            {
                return _suggestedTags;
            }
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
        #endregion

        /// <summary>
        /// Get comma separated list of suggested tags.
        /// </summary>
        public string TagList
        {
            get
            {
                StringBuilder tags = new StringBuilder();
                foreach (var t in _suggestedTags.Values)
                {
                    if (tags.Length > 0)
                    {
                        tags.Append(',');
                    }
                    tags.Append(t.TagName);
                }
                return tags.ToString();
            }
        }

        /// <summary>
        /// Persist any changes
        /// </summary>
        internal void SaveChanges()
        {
            string[] t = (from v in _suggestedTags.Values select v.TagName).ToArray();
            Properties.Settings.Default.KnownTags = string.Join(",", t);
            Properties.Settings.Default.Save();
        }

        #region IDisposable
        /// <summary>
        /// Free resources of the view model
        /// </summary>
        public void Dispose()
        {
            if (_pages != null)
            {
                _pages.Dispose();
                _pages = null;
            }
            _cancelFinder.Cancel();
        }
        #endregion IDisposable
    }
}
