// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    /// View model backing a <see cref="RemovableTag" /> user control.
    /// </summary>
    /// <remarks>
    /// Provides properties to enable/disable a tag for removal and to adjust the
    /// presentation of the corresponding UI element.
    /// </remarks>
    public class RemovableTagModel : SuggestedTagDataContext
    {
        internal static readonly PropertyChangedEventArgs USE_COUNT = new PropertyChangedEventArgs("UseCount");
        internal static readonly PropertyChangedEventArgs USE_COUNT_COLOR = new PropertyChangedEventArgs("UseCountColor");
        internal static readonly PropertyChangedEventArgs MARKER_VISIBILIY = new PropertyChangedEventArgs("RemoveMarkerVisibility");
        internal static readonly PropertyChangedEventArgs CAN_REMOVE = new PropertyChangedEventArgs("CanRemove");
        internal static readonly PropertyChangedEventArgs LOCAL_NAME = new PropertyChangedEventArgs("LocalName");

        /// <summary>
        /// predefined event descriptor for <see cref="E:PropertyChanged"/> event fired for the <see cref="Visibility"/> property
        /// </summary>
        internal static readonly PropertyChangedEventArgs VISIBILITY_Property = new PropertyChangedEventArgs("Visibility");

        /// <summary>
        /// Create a new instance of the view model.
        /// </summary>
        public RemovableTagModel()
        {
        }

        private TagPageSet _tag;

        /// <summary>
        /// Get or set the Tag for the view model.
        /// </summary>
        /// <remarks>
        /// The tag is used to provide the page count (number of pages with this tag)
        /// </remarks>
        internal TagPageSet Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
                TagName = value.TagName;
                LocalName = value.TagName;
                UseCount = value.Pages.Count;
            }
        }

        private string _localName;

        public string LocalName
        {
            get
            {
                return _localName;
            }
            set
            {
                _localName = value;
                firePropertyChanged(LOCAL_NAME);
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

        private int _useCount = 0;

        /// <summary>
        /// Get the number of pages having this tag.
        /// </summary>
        public int UseCount
        {
            get { return _useCount; }
            internal set
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
                        firePropertyChanged(USE_COUNT_COLOR);
                    }
                }
            }
        }

        /// <summary>
        /// Set a filter string which is used to determine the appearance of the <see cref="HitHighlightedTagButton"/>
        /// control.
        /// </summary>
        /// <remarks>
        /// Setting this property has a side effect on the <see cref="Visibility"/> property.
        /// The appropriate <see cref="E:WetHatLab.OneNote.TaggingKit.edit.PropertyChanged"/> events are fired as necessary.
        /// </remarks>
        public override TextSplitter Highlighter {
            set {
                Visibility visBefore = Visibility;
                base.Highlighter = value;
                if (visBefore != Visibility) {
                    firePropertyChanged(VISIBILITY_Property);
                }
            }
        }

        /// <summary>
        /// Get the color of the tag use count indicator.
        /// </summary>
        public Brush UseCountColor
        {
            get
            {
                return CanRemove ? Brushes.Red : Brushes.Black;
            }
        }

        /// <summary>
        /// Get the visibility the associated <see cref="HitHighlightedTagButton"/> control has.
        /// </summary>
        public Visibility Visibility {
            get {
                return Highlighter.SplitPattern == null || HasHighlights ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }
    }
}