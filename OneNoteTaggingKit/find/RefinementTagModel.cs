﻿// Author: WetHat | (C) Copyright 2013 - 2022 WetHat Lab, all rights reserved
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// View model to support <see cref="SelectableTag" /> controls.
    /// </summary>
    [ComVisible(false)]
    public class RefinementTagModel : SelectableTagModel {
        Dispatcher _dispatcher;

        /// <summary>
        /// Get  page refinement tag.
        /// </summary>
        public RefinementTagBase RefinementTag { get; set; }

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
        internal RefinementTagModel(RefinementTagBase tag, Dispatcher dispatcher) {
            RefinementTag = tag;
            _dispatcher = dispatcher;
            Tag = tag.Tag; // initialize the base class
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
                    case nameof(RefinementTagBase.FilteredPageCount):
                        UpdateUI();
                        break;
                    case nameof(RefinementTagBase.FilteredPageCountDelta):
                        UpdateUI();
                        break;
                    case nameof(RefinementTagBase.TagWithPages):
                        if (sender is RefinementTagBase rtb) {
                            Tag = rtb.Tag; // maybe name changesd
                            UpdateUI();
                        }
                        break;
                }
            });
        }

        void UpdateUI() {
            UpdateTagIndicator();
            UpdateTagVisibility();
        }
        void UpdateTagIndicator() {
            if (IsSelected) {
                TagIndicator = "";
                TagIndicatorColor = Brushes.Red;
            } else {
                //                                                  down --.     .-- up
                string arrow = RefinementTag.FilteredPageCountDelta < 0 ? "" : "";

                TagIndicator = string.Format("{0}{1}", arrow, RefinementTag.FilteredPageCount);
                TagIndicatorColor = Brushes.Black;
            }
        }

        /// <summary>
        ///     Handle base class property changes.
        /// </summary>
        /// <param name="sender">
        ///     Instance of the model which raised the event.
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
            if (RefinementTag.FilteredPageCount == 0
                || IsSelected // selected models are always hidden in the refinements panel
                || RefinementTag.FilteredPageCountDelta == 0) {
                TagVisibility = Visibility.Collapsed;
            } else {
                base.UpdateTagVisibility();
            }
        }
    }
}