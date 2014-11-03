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
        internal static readonly PropertyChangedEventArgs CAN_REMOVE = new PropertyChangedEventArgs("CanRemove");

        /// <summary>
        /// Create a new instance of the view model.
        /// </summary>
        public RemovableTagModel()
        {
        }

        /// <summary>
        /// Set the Tag for the view model.
        /// </summary>
        /// <remarks>The tag is used to provide the page count (number of pages with this tag)</remarks>
        internal TagPageSet Tag
        {
            set
            {
                TagName = value.TagName;
                UseCount = value.FilteredPages.Count;
            }
        }
        
        /// <summary>
        /// Check whether the tag can be removed
        /// </summary>
        /// <value>true, if the tag has a page count of 0 and can be removed; false otherwise</value>
        public bool CanRemove
        {
            get { return UseCount == 0; }
        }

        int _useCount = 0;
        /// <summary>
        /// Get the number of pages having this tag.
        /// </summary>
        public int UseCount
        {
            get { return _useCount;  }
            private set
            {
                if (_useCount != value)
                {
                    int oldValue = _useCount;
                    _useCount = value;
                    firePropertyChanged(USE_COUNT);

                    if (value == 0 || oldValue == 0)
                    {
                        firePropertyChanged(MARKER_VISIBILIY);
                        firePropertyChanged(CAN_REMOVE);
                    }
                }
            }
        }

        /// <summary>
        /// Get the visibility of the <i>remove</i> marker
        /// </summary>
        /// <remarks>The marker is visible for tags with page count 0</remarks>
        public Visibility RemoveMarkerVisibility
        {
            get { return CanRemove ? Visibility.Visible : Visibility.Collapsed; }
        }
    }
}
