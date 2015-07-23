using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
        SuggestedTagsSource<RemovableTagModel> SuggestedTags { get; }

        /// <summary>
        /// Get the add-in version.
        /// </summary>
        string AddinVersion { get; }

        /// <summary>
        /// Get the add-in version.
        /// </summary>
        string FrameworkVersion { get; }

        /// <summary>
        /// Get the location of the logfile
        /// </summary>
        string Logfile { get; }
    }

    /// <summary>
    /// View model backing the <see cref="TagManager"/> dialog.
    /// </summary>
    [ComVisible(false)]
    public class TagManagerModel : WindowViewModelBase, ITagManagerModel
    {
        private SuggestedTagsSource<RemovableTagModel> _suggestedTags = new SuggestedTagsSource<RemovableTagModel>();
        private TagsAndPages _tags;

        /// <summary>
        /// Create a new instance of the view model backing the <see cref="TagManager"/> dialog.
        /// </summary>
        /// <param name="onenote">OneNote application object</param>
        /// <param name="schema">current schema version</param>
        internal TagManagerModel(Application onenote, XMLSchema schema): base(onenote,schema)
        {
            _tags = new TagsAndPages(OneNoteApp, OneNotePageSchema);
        }

        internal async Task LoadSuggestedTagsAsync()
        {
            Task onenotetags = Task.Run(() => _tags.FindTaggedPages(String.Empty));
            Task suggestions = _suggestedTags.LoadSuggestedTagsAsync();
            await Task.WhenAll(onenotetags, suggestions);
            // update the tags loaded from the settings
            foreach (var t in _suggestedTags.Values)
            {
                TagPageSet tag;
                if (_tags.Tags.TryGetValue(t.TagName, out tag))
                {
                    t.Tag = tag;
                }
            }
        }
       
        #region ITagManagerModel

        /// <summary>
        /// Get the collection of tags used for suggestions.
        /// </summary>
        /// <remarks>This collection includes all tags used on any OneNote pages and additional tags suggestions
        /// which were added manually</remarks>
        public SuggestedTagsSource<RemovableTagModel> SuggestedTags
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
        public string FrameworkVersion
        {
            get { return Environment.Version.ToString(); }
        }

        public string Logfile
        {
            get { return TraceLogger.LogFile; }
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
            _suggestedTags.Save();
        }
    }
}
