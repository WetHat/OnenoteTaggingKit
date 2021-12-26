namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// A view model which decorates instances of <see cref="FilterableTagModel"/>
    /// where the <see cref="FilterableTagModel.IsSelected"/> is `true`.
    /// </summary>
    /// <remarks>
    /// When disposed sets <see cref="FilterableTagModel.IsSelected"/> of the
    /// decorated view model to `false`.
    /// </remarks>
    public class SelectedTagModel : TagModel
    {

        FilterableTagModel _selectableTag = null;
        /// <summary>
        /// Get/set the view model of a tag that can be selected
        /// </summary>
        /// <remarks>
        ///     As a side-efect also sets the <see cref="TagModel.TagName"/>
        ///     property inherited from its base class.
        /// </remarks>
        public FilterableTagModel SelectableTag {
            get => _selectableTag;
            set {
                _selectableTag = value;
                TagName = value.TagName;
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
