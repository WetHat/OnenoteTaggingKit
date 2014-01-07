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
    /// View model for the <see cref="TagManager"/> dialog.
    /// </summary>
    public class TagManagerModel : ITagManagerModel
    {
        private ObservableCollection<string> _suggestedTags;


        internal TagManagerModel()
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

        internal void SaveChanges()
        {
            string[] t = _suggestedTags.ToArray();
            Array.Sort(t);
            Properties.Settings.Default.KnownTags = string.Join(",", t);
            Properties.Settings.Default.Save();
        }
    }
}
