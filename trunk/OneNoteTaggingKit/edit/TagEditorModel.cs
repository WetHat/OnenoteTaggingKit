using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// View Model to support the tag editor dialog
    /// </summary>
    public class TagEditorModel : ITagEditorModel
    {
        private Application _OneNote;

        private ObservableCollection<string> _pageTags;
        private ObservableCollection<string> _knownTags;

        private OneNotePageProxy _page;

        internal TagEditorModel(Application onenote, string pageID)
        {
            _OneNote = onenote;

            _page = new OneNotePageProxy(_OneNote, _OneNote.Windows.CurrentWindow.CurrentPageId);

            _pageTags = new ObservableCollection<string>(_page.PageTags);

            _knownTags = new ObservableCollection<string>(OneNotePageProxy.ParseTags(Properties.Settings.Default.KnownTags));  
        }

        #region ITagEditorModel
        /// <summary>
        /// Get the collection of page tags.
        /// </summary>
        public ObservableCollection<string> PageTags
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

        internal void SaveChanges()
        {
            string[] tags = _pageTags.ToArray();
            Array.Sort(tags);
            _page.PageTags = tags;
            _page.Update();

            Properties.Settings.Default.KnownTags = string.Join(",", _knownTags.Union(_pageTags));
            Properties.Settings.Default.Save();
        }
    }
}
