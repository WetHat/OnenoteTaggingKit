using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// View model implementation for the UI designer
    /// </summary>
    public class TagEditorDesignerModel : ITagEditorModel
    {
        private ObservableCollection<string> _pageTags = new ObservableCollection<string>();
        private ObservableCollection<string> _knownTags = new ObservableCollection<string>();

        /// <summary>
        /// Create a new instance of the view model
        /// </summary>
        public TagEditorDesignerModel()
        {
            _knownTags.Add("Known Tag 1");
            _knownTags.Add("Known Tag 2");
            _knownTags.Add("Known Tag 3");
        }

        /// <summary>
        /// get the collection of page tags.
        /// </summary>
        public ObservableCollection<string> PageTags
        {
            get { return _pageTags; }
        }

        /// <summary>
        /// Get the collection of all known tags.
        /// </summary>
        public ObservableCollection<string> KnownTags
        {
            get { return _knownTags; }
        }

        /// <summary>
        /// Get the version of the addin
        /// </summary>
        public string AddinVersion
        {
            get { return "1.2.3.4"; }
        }
    }
}
