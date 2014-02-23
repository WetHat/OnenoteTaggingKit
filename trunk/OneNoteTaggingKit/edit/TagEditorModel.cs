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
    /// A simple tag consisting just of a name (key)
    /// </summary>
    public class SimpleTag : IKeyedItem
    {
        private string _tag;

        /// <summary>
        /// Create a new instance of a <see cref="SimpleTag"/> object
        /// </summary>
        /// <param name="tag"></param>
        public SimpleTag(string tag)
        {
            _tag = tag;
        }

        #region IKeyedItem
        /// <summary>
        /// Get the key (name) of this tag
        /// </summary>
        public string Key
        {
            get { return _tag; }
        }
        #endregion IKeyedItem
    }
    /// <summary>
    /// Contract used by the tag editor view model
    /// </summary>
    /// <seealso cref="WetHatLab.OneNote.TaggingKit.edit.TagEditor"/>
    internal interface ITagEditorModel
    {
        /// <summary>
        /// Get the collection of tags on current OneNote page.
        /// </summary>
        ObservableSortedList<SimpleTag> PageTags { get; }

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
    /// View Model to support the tag editor dialog
    /// </summary>
    public class TagEditorModel : ITagEditorModel
    {
        private Application _OneNote;

        private ObservableSortedList<SimpleTag> _pageTags;
        private ObservableCollection<string> _knownTags;

        private OneNotePageProxy _page;

        internal TagEditorModel(Application onenote, string pageID,XMLSchema schema)
        {
            _OneNote = onenote;

            _page = new OneNotePageProxy(_OneNote, _OneNote.Windows.CurrentWindow.CurrentPageId,schema);

            _pageTags = new ObservableSortedList<SimpleTag>();
            _pageTags.AddAll(from t in _page.PageTags select new SimpleTag(t));

            _knownTags = new ObservableCollection<string>(OneNotePageProxy.ParseTags(Properties.Settings.Default.KnownTags));  
        }

        #region ITagEditorModel
        /// <summary>
        /// Get the collection of page tags.
        /// </summary>
        public ObservableSortedList<SimpleTag> PageTags
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
            string[] tags = (from st in _pageTags.Values select st.Key).ToArray();

            _page.PageTags = tags;
            _page.Update();

            Properties.Settings.Default.KnownTags = string.Join(",", _knownTags.Union(tags));
            Properties.Settings.Default.Save();
        }
    }
}
