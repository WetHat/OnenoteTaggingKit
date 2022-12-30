// Author: WetHat | (C) Copyright 2013 - 2022 WetHat Lab, all rights reserved
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// View model to support <see cref="SelectableTag" /> controls.
    /// </summary>
    [ComVisible(false)]
    public class RefinementTagModel : SelectableTagModel {
        private RefinementTag _refinementTag;

        Dispatcher _dispatcher;

        /// <summary>
        ///     Initialize a new view model instance from a tag.
        /// </summary>
        /// <remarks>
        ///     Instances of this model support asynchronous changes in the
        ///     underlying data.
        /// </remarks>
        /// <param name="tag">tag object</param>
        /// <param name="dispatcher">
        ///     The displatcher use to raise property change events
        /// </param>
        internal RefinementTagModel(RefinementTag tag, Dispatcher dispatcher) {
            _refinementTag = tag;
            _dispatcher = dispatcher;
            Tag = tag.Tag.Tag; // initialize the base class
            tag.PropertyChanged += OnTagPropertyChanged;
            UpdateUI();
        }

        /// <summary>
        /// Handle property changes of the underlying page tag of type <see cref="Tag"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTagPropertyChanged(object sender, PropertyChangedEventArgs e) {
            _dispatcher.Invoke(() => {
                switch (e.PropertyName) {
                    case nameof(RefinementTag.FilteredPageCount):
                        UpdateUI();
                        break;
                }
            });
        }

        void UpdateUI() {
            Tooltip = String.Format(Properties.Resources.TagSearch_Tag_Tooltip, _refinementTag.FilteredPageCount, PageTag.Pages.Count);
            UpdateTagIndicator();
            UpdateTagVisibility();
        }
        void UpdateTagIndicator() {
            if (IsSelected) {
                TagIndicator = "";
                TagIndicatorColor = Brushes.Red;
            } else {
                TagIndicator = string.Format(" ↓{0}",_refinementTag.FilteredPageCount);
                TagIndicatorColor = Brushes.Black;
            }
        }

        /// <summary>
        ///     Handle base class property changes.
        /// </summary>
        /// <param name="sender">
        ///     Instance of the model which reised the event.
        /// </param>
        /// <param name="e">
        ///     Change details.
        /// </param>
        protected override void TagModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            _dispatcher.Invoke(() => {
                base.TagModelPropertyChanged(sender, e);
                switch (e.PropertyName) {
                    case nameof(IsSelected):
                        UpdateTagIndicator();
                        UpdateTagVisibility();
                        break;
                }
            });
        }
        /// <summary>
        /// Compute the visibility based on changes to the <see cref="SelectableTagModel.IsSelected"/>
        /// property.
        /// </summary>
        protected override void UpdateTagVisibility() {
            if (_refinementTag.FilteredPageCount == 0) {
                TagVisibility = Visibility.Collapsed;
            } else {
                base.UpdateTagVisibility();
            }
        }

        /// <summary>
        /// Get/set the page tag object.
        /// </summary>
        public TagPageSet PageTag => _refinementTag.Tag;

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