// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Windows.Media;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    /// View model backing a <see cref="RemovableTag" /> user control.
    /// </summary>
    /// <remarks>
    /// Provides properties to allow adding/removing/renaming
    /// tags
    /// </remarks>
    public class RemovableTagModel : FilterableTagModel
    {
        /// <summary>
        /// Create a new instance of the view model.
        /// </summary>
        public RemovableTagModel()
        {
            UseCount = 0;
        }

        private TagPageSet _tag;

        /// <summary>
        /// Set the page tag which keeps track of the pages having this
        /// tag.
        /// </summary>
        /// <remarks>
        /// The page tag is used to provide the page count
        /// (number of pages with this tag). If the page count is 0, the
        /// tag isn't used anywhere.
        /// </remarks>
        internal TagPageSet PageTag {
            get => _tag;
            set {
                _tag = value;
                if (Tag == null) {
                    Tag = value.Tag;
                }
                LocalName = Tag.DisplayName;
                UseCount = value.Pages.Count;
            }
        }

        private string _localName = string.Empty;
        /// <summary>
        /// Get/set the editable tag name.
        /// </summary>
        /// <remarks>Use in edit controls to allow modifications without
        /// affecting the original tag name.
        /// </remarks>
        public string LocalName {
            get => _localName;
            set {
                _localName = value;
                RaisePropertyChanged();
            }
        }

        private int _useCount = -1;

        /// <summary>
        /// Get the number of pages having this tag.
        /// </summary>
        /// <value>If 0, the tag is not  used anywhere.</value>
        public int UseCount
        {
            get => _useCount;
            set
            {
                if (_useCount != value)
                {
                    _useCount = value;
                    // Compute the presentation of the indicator
                    TagIndicator = string.Format("({0})", UseCount);
                    IndicatorForeground = UseCount == 0 ? Brushes.Red : Brushes.Black;
                }
            }
        }
        Brush _indicatorForeground = Brushes.Red;
        /// <summary>
        /// Get the color of the tag use count indicator.
        /// </summary>
        public Brush IndicatorForeground {
            get => _indicatorForeground;
            set {
                if (_indicatorForeground != value) {
                    _indicatorForeground = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}