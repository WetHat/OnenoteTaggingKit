using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using System.Windows;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    /// View model backing a <see cref="RemovableTag"/> user control.
    /// </summary>
    /// <remarks>Provides properties to enable/disable a tag for removal and to adjust the presentation of the corresponding UI element.</remarks>
    public class RemovableTagModel : SuggestedTagsDataContext
    {
        internal static readonly PropertyChangedEventArgs USE_COUNT = new PropertyChangedEventArgs("UseCount");
        internal static readonly PropertyChangedEventArgs MARKER_VISIBILIY = new PropertyChangedEventArgs("RemoveMarkerVisibility");

        public RemovableTagModel()
        {
        }

        internal TagPageSet Tag
        {
            set
            {
                TagName = value.TagName;
                UseCount = value.PageCount;
            }
        }
        
        /// <summary>
        /// Check whether the tag can be removed
        /// </summary>
        private bool CanRemove
        {
            get { return UseCount == 0; }
        }

        int _useCount = 0;
        /// <summary>
        /// Get the number of uses of this tag 
        /// </summary>
        public int UseCount
        {
            get { return _useCount;  }
            private set
            {
                if (_useCount != value)
                {
                    _useCount = value;
                    firePropertyChanged(USE_COUNT);
                    firePropertyChanged(MARKER_VISIBILIY);
                }
            }
        }

        /// <summary>
        /// Get the visibility of the <i>remove</i> marker
        /// </summary>
        public Visibility RemoveMarkerVisibility
        {
            get { return CanRemove ? Visibility.Visible : Visibility.Collapsed; }
        }
    }
}
