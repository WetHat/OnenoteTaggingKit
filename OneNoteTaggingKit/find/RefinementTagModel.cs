// Author: WetHat | (C) Copyright 2013 - 2022 WetHat Lab, all rights reserved
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// View model to support <see cref="SelectableTag" /> controls.
    /// </summary>
    [ComVisible(false)]
    public class RefinementTagModel : SelectableTagModel
    {
        /// <summary>
        /// Create a new view model instance from a tag.
        /// </summary>
        /// <param name="tag">tag object</param>
        internal RefinementTagModel(TagPageSet tag) {
            PageTag = tag;
            tag.PropertyChanged += OnTagPropertyChanged;
        }

        /// <summary>
        /// Create a new view model instance for a tag and an event handler.
        /// </summary>
        /// <param name="tag">        tag object</param>
        /// <param name="propHandler">listener for property changes</param>
        internal RefinementTagModel(TagPageSet tag, PropertyChangedEventHandler propHandler)
            : this(tag) {
            PropertyChanged += propHandler;
        }

        /// <summary>
        /// Handle property changes of the underlying page tag of type <see cref="Tag"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTagPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(TagPageSet.FilteredPageCount):
                    Tooltip = String.Format(Properties.Resources.TagSelector_PageCount_Tooltip, PageTag.FilteredPages.Count,PageTag.Pages.Count);
                    UpdateTagIndicator();
                    break;
            }
        }
        void UpdateTagIndicator() {
            if (IsSelected) {
                TagIndicator = "";
                TagIndicatorColor = Brushes.Red;
            } else {
                TagIndicator = string.Format(" ↓{0}", PageTag.FilteredPageCount);
                TagIndicatorColor = Brushes.Black;
            }
        }

        /// <summary>
        /// Handle base class property changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void TagModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.TagModelPropertyChanged(sender, e);
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    UpdateTagIndicator();
                    UpdateTagVisibility();
                    break;
            }
        }
        /// <summary>
        /// Compute the visibility based on changes to the <see cref="SelectableTagModel.IsSelected"/>
        /// property.
        /// </summary>
        protected override void UpdateTagVisibility() {
            if (PageTag.FilteredPageCount == 0) {
                TagVisibility = Visibility.Collapsed;
            } else {
                base.UpdateTagVisibility();
            }
        }

        TagPageSet _pageTag = null;
        /// <summary>
        /// Get/set the page tag object.
        /// </summary>
        public TagPageSet PageTag {
            get => _pageTag;
            set {
                _pageTag = value;
                _pageTag.PropertyChanged += _pageTag_PropertyChanged;
                TagName = _pageTag.TagName;
                TagType = _pageTag.TagType;
                UpdateTagIndicator();
            }
        }

        private void _pageTag_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(TagPageSet.FilteredPageCount):
                    UpdateTagVisibility();
                    if (TagVisibility == Visibility.Visible) {
                        UpdateTagIndicator();
                    }
                    break;
            }
        }

        string _tooltip = string.Empty;
        /// <summary>
        /// Get the tooltip of the page count
        /// </summary>
        public string Tooltip {
            get => _tooltip;
            private set {
                if (!_tooltip.Equals(value)) {
                    _tooltip = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}