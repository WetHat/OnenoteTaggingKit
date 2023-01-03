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
    public class RemovableTagModel : FilterableTagModel {
        /// <summary>
        /// Create a new instance of the view model.
        /// </summary>
        public RemovableTagModel() {
            UseCount = 0;
        }

        public override PageTag Tag {
            set {
                base.Tag = value;
                LocalName = value.DisplayName;
            }
        }

        private TagPageSet _pages;

        /// <summary>
        ///     Set the page collection which have a particular tag.
        /// </summary>
        /// <remarks>
        ///     The page collection is used to provide the page count
        ///     (number of pages with a particular tag). If the page count is 0,
        ///     the tag isn't used anywhere.
        ///
        ///     This property can be set only once
        /// </remarks>
        internal TagPageSet PagesOfTag {
            get => _pages;
            set {
                if (_pages == null) {
                    Tag = value.Tag;
                    _pages = value;
                    LocalName = Tag.DisplayName;
                    UseCount = value.Pages.Count;
                    IsModifiable = !_pages.Tag.IsImported;
                }
            }
        }

        bool _modifiable = true; // make sure tags are modifyable by default.
        /// <summary>
        /// Predicate to determine if the tag can be modified.
        /// </summary>
        public bool IsModifiable {
            get => _modifiable;
            set {
                if (_modifiable != value) {
                    _modifiable = value;
                    RaisePropertyChanged();
                }
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