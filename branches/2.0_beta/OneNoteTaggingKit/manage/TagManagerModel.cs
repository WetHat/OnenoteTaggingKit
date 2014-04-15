using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
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
        ObservableSortedList<TagModelKey, string, RemovableTagModel> SuggestedTags { get; }

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
        private ObservableSortedList<TagModelKey, string, RemovableTagModel> _suggestedTags = new ObservableSortedList<TagModelKey, string, RemovableTagModel>();
        private TagCollection _tags;

        CancellationTokenSource _cancelFinderToken = new CancellationTokenSource();
        private Task<Tuple<IEnumerable<RemovableTagModel>, IEnumerable<RemovableTagModel>>> _finderAction;

        /// <summary>
        /// Create a new instance of the view model backing the <see cref="TagManager"/> dialog.
        /// </summary>
        /// <param name="onenote">OneNote application object</param>
        /// <param name="schema">current schema version</param>
        internal TagManagerModel(Application onenote, XMLSchema schema)
        {
            _tags = new TagCollection(onenote, schema);
        }

        /// <summary>
        /// Background worker method to collect the suggested tags by running a search
        /// against all open OneNote notebooks
        /// </summary>

        private Tuple<IEnumerable<RemovableTagModel>,IEnumerable<RemovableTagModel>> LoadTagSuggestionsAction()
        {
            _tags.Find(String.Empty);

            IEnumerable<RemovableTagModel> suggestions = from s in OneNotePageProxy.ParseTags(Properties.Settings.Default.KnownTags)
                                                         where !_tags.Tags.ContainsKey(s)
                                                         select new RemovableTagModel(new TagPageSet(s));

            IEnumerable<RemovableTagModel> tagsInUse = from t in _tags.Tags.Values select new RemovableTagModel(t);

            return new Tuple<IEnumerable<RemovableTagModel>, IEnumerable<RemovableTagModel>>(suggestions, tagsInUse);
        }

        /// <summary>
        /// Asynchronously load tag suggestions.
        /// </summary>
        /// <remarks>Runs a search for tags over all notebooks open in OneNote to collect tags in use.</remarks>
        /// <param name="continuation">continuation action to be run in the UI thread</param>
        internal async Task LoadTagSuggestionsAsync()
        {
            if (_finderAction != null && _finderAction.Status == TaskStatus.Running)
            {
                _cancelFinderToken.Cancel();
            }
            _finderAction = Task.Run(() => LoadTagSuggestionsAction(), _cancelFinderToken.Token);
            Tuple<IEnumerable<RemovableTagModel>, IEnumerable<RemovableTagModel>> tagModels = await _finderAction; 

            _suggestedTags.AddAll(tagModels.Item1);
            _suggestedTags.AddAll(tagModels.Item2);
        }

        #region ITagManagerModel

        /// <summary>
        /// Get the collection of tags used for suggestions.
        /// </summary>
        /// <remarks>This collection includes all tags used on any OneNote pages and additional tags suggestions
        /// which were added manually</remarks>
        public ObservableSortedList<TagModelKey, string, RemovableTagModel> SuggestedTags
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
            if (_tags != null)
            {
                _tags = null;
            }
            _cancelFinderToken.Cancel();
        }
        #endregion IDisposable
    }
}
