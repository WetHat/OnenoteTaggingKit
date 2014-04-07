using System.Collections.ObjectModel;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// View model implementation for the UI designer
    /// </summary>
    public class TagEditorDesignerModel : ITagEditorModel
    {
        ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> _pageTags = new ObservableSortedList<TagModelKey, string, SimpleTagButtonModel>();
        ObservableSortedList<TagModelKey, string, HitHighlightedTagButtonModel> _suggestedTags = new ObservableSortedList<TagModelKey, string, HitHighlightedTagButtonModel>();

        /// <summary>
        /// Create a new instance of the view model
        /// </summary>
        public TagEditorDesignerModel()
        {
            _suggestedTags.AddAll(new HitHighlightedTagButtonModel[] {
                new HitHighlightedTagButtonModel("Suggested Tag 1"),
                new HitHighlightedTagButtonModel("Suggested Tag 2")
            });

            _pageTags.AddAll(new SimpleTagButtonModel[] { new SimpleTagButtonModel("tag 1"), new SimpleTagButtonModel("tag 2") });
        }

        /// <summary>
        /// get the collection of page tags.
        /// </summary>
        public ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> PageTags
        {
            get { return _pageTags; }
        }

        public ObservableSortedList<TagModelKey, string, HitHighlightedTagButtonModel> SuggestedTags
        {
            get { return _suggestedTags; }
        }
    }
}
