using System.Windows;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// View model for tags which change state when they are selected.
    /// </summary>
    public class SelectableTagModel : FilterableTagModel
    {
        bool _isSelected = false;
        /// <summary>
        /// Get/set the tag selection flag.
        /// </summary>
        /// <remarks>Selected tags are collapsed.</remarks>
        public bool IsSelected {
            get => _isSelected;
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    UpdateTagVisibility();
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Compute the visibility based on changes to the <see cref="IsSelected"/>
        /// and/or <see cref="FilterableTagModel.HighlightedTagName"/> property.
        /// </summary>
        protected override void UpdateTagVisibility() {
            if (IsSelected) {
                TagVisibility = Visibility.Collapsed;
            } else {
                base.UpdateTagVisibility();
            }
        }
    }
}
