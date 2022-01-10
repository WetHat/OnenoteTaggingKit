namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// A view model which decorates instances of <see cref="SelectableTagModel"/>
    /// where the <see cref="SelectableTagModel.IsSelected"/> is `true`.
    /// </summary>
    /// <remarks>
    /// When disposed sets <see cref="SelectableTagModel.IsSelected"/> of the
    /// decorated view model to `false`.
    /// </remarks>
    public class SelectedTagModel : TagModel
    {

        SelectableTagModel _selectableTag = null;
        /// <summary>
        /// Get/set the view model of a tag that can be selected
        /// </summary>
        /// <remarks>
        ///     As a side-efect also sets the <see cref="TagModel.TagName"/>
        ///     property inherited from its base class.
        /// </remarks>
        public SelectableTagModel SelectableTag {
            get => _selectableTag;
            set {
                _selectableTag = value;
                TagName = value.TagName;
                TagType = value.TagType;
            }
        }

        /// <summary>
        /// Create a new instance of this decorator class.
        /// </summary>
        public SelectedTagModel() {
            TagIndicator = "❌";
        }
    }
}
