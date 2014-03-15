using System.Collections.ObjectModel;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// View model implementation for the UI designer
    /// </summary>
    public class TagEditorDesignerModel : ITagEditorModel
    {
        private ObservableSortedList<string,string,SimpleTagButtonModel> _pageTags = new ObservableSortedList<string,string,SimpleTagButtonModel>();
        private ObservableCollection<string> _knownTags = new ObservableCollection<string>();

        /// <summary>
        /// Create a new instance of the view model
        /// </summary>
        public TagEditorDesignerModel()
        {
            _knownTags.Add("Known Tag 1");
            _knownTags.Add("Known Tag 2");
            _knownTags.Add("Known Tag 3");

            _pageTags.AddAll(new SimpleTagButtonModel[] { new SimpleTagButtonModel("tag 1"), new SimpleTagButtonModel("tag 2") });
        }

        /// <summary>
        /// get the collection of page tags.
        /// </summary>
        public ObservableSortedList<string,string,SimpleTagButtonModel> PageTags
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


        public string PageTitle
        {
            get { return "Designer Page"; }
        }
    }
}
