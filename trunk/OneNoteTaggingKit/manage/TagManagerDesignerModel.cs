using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    /// View model to support design mode for the <see cref="TagManager"/> dialog
    /// </summary>
    public class TagManagerDesignerModel : ITagManagerModel
    {
        private ObservableCollection<string> _tags = new ObservableCollection<string>();

        /// <summary>
        /// Create a new instance of the view model including some dummy data.
        /// </summary>
        public TagManagerDesignerModel()
        {
            _tags.Add("Tag 1");
            _tags.Add("Tag 2");
        }

        /// <summary>
        /// Get the collection of tags used for suggestions.
        /// </summary>
        public ObservableCollection<string> SuggestedTags
        {
            get
            {
                return _tags;
            }
        }

        /// <summary>
        /// Get the version string for the addin
        /// </summary>
        public string AddinVersion
        {
            get { return "1.2.3.4"; }
        }
    }
}
