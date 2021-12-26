using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
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
                _isSelected = value;
                TagVisibility = ComputeTagVisibility();
                TagIndicator = ComputeTagIndicator();
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Compute the visibility based on changes to the <see cref="IsSelected"/>
        /// and/or <see cref="HighlightedTagName"/> property.
        /// </summary>
        protected override Visibility ComputeTagVisibility() {
            if (IsSelected) {
                return Visibility.Collapsed;
            }

            return base.ComputeTagVisibility();
        }
    }
}
