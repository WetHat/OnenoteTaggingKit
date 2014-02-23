using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    ///  Contract for view models of the <see cref="WetHatLab.OneNote.TaggingKit.manage.TagManager"/> dialog
    /// </summary>
    /// <seealso cref="WetHatLab.OneNote.TaggingKit.manage.TagManager"/>

    internal interface ITagManagerModel
    {
        /// <summary>
        /// Get the collection of all tags used for suggestions.
        /// </summary>
        ObservableCollection<string> SuggestedTags { get; }

        /// <summary>
        /// Get the addin version.
        /// </summary>
        string AddinVersion { get; }
    }

    /// <summary>
    /// View model for the <see cref="TagManager"/> dialog.
    /// </summary>
    public class TagManagerModel : ITagManagerModel
    {
        private ObservableCollection<string> _suggestedTags;

        internal TagManagerModel(Application onenote, XMLSchema schema)
        {
            _suggestedTags = new ObservableCollection<string>(OneNotePageProxy.ParseTags(Properties.Settings.Default.KnownTags));
        }

        #region ITagManagerModel

        /// <summary>
        /// Get the collection of tags used for suggestions.
        /// </summary>
        public ObservableCollection<string> SuggestedTags
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
                foreach (var t in _suggestedTags)
                {
                    if (tags.Length > 0)
                    {
                        tags.Append(',');
                    }
                    tags.Append(t);
                }
                return tags.ToString();
            }
        }

        internal void SaveChanges()
        {
            string[] t = _suggestedTags.ToArray();
            Array.Sort(t);
            Properties.Settings.Default.KnownTags = string.Join(",", t);
            Properties.Settings.Default.Save();
        }
    }
}
