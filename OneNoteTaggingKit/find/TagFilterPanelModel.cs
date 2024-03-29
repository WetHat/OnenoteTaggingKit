﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    ///     View model backing the <see cref="TagFilterPanel"/> control.
    /// </summary>
    public class TagFilterPanelModel : ObservableObject {
        /// <summary>
        ///     Get the observable list of view models of tags selected for refinement.
        /// </summary>
        public ObservableTagList<SelectedTagModel> SelectedTagModels { get; } = new ObservableTagList<SelectedTagModel>();

        TagsAndPages _ContextTagSource;
        /// <summary>
        ///     Get the source of tags found in a OneNote hierarchy context (section,
        ///     section group, notebook).
        /// </summary>
        /// <remarks>
        ///     This source object is used to control the tag presets.
        /// </remarks>
        public TagsAndPages ContextTagSource {
            get => _ContextTagSource;
            private set {
                _ContextTagSource = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Get the OneNote application object proxy.
        /// </summary>
        OneNoteProxy OneNoteApp { get; set; }

        /// <summary>
        ///     Get a source for <see cref="RefinementTagModel"/> view models.
        /// </summary>
        public RefinementTagModelSource RefinementTagModels { get; private set; }

        string _RefinementTagsPanelHeader = string.Empty;

        /// <summary>
        ///     Get the header text for the panel displaying the tags
        ///     available for filter selection.
        /// </summary>
        public string RefinementTagsPanelHeader {
            get => _RefinementTagsPanelHeader;
            private set {
                if (!value.Equals(_RefinementTagsPanelHeader)) {
                    _RefinementTagsPanelHeader = value;
                    RaisePropertyChanged();
                }
            }
        }
        public void UpdateRefinementTagsPanelHeader() {
            var reductionIndicator = SelectedTagModels.Count > 0 ? "" : string.Empty; // filter symbol from Segoe UI Symbol.
            // ▼↓
            var highlightIndicator = RefinementTagModels.Highlighter.SplitPattern != null ? "🔍" : string.Empty;

            RefinementTagsPanelHeader = reductionIndicator == string.Empty && highlightIndicator == string.Empty
                ? string.Format("{0} ({1})",
                                Properties.Resources.TagSearch_Tags_GroupBox_Title,
                                RefinementTagModels.Count)
                : string.Format("{0} {1}{2}",
                                Properties.Resources.TagSearch_Tags_GroupBox_Title,
                                reductionIndicator,
                                highlightIndicator);
        }

        string _SelectedTagsPanelHeader = string.Empty;
        /// <summary>
        /// Get the header text for the panel displaying selected tags.
        /// </summary>
        public string SelectedTagsPanelHeader {
            get => _SelectedTagsPanelHeader;
            private set {
                if (!value.Equals(_SelectedTagsPanelHeader)) {
                    _SelectedTagsPanelHeader = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// Add a single tag to the refinement filter.
        /// </summary>
        /// <param name="tag">View model of the tag.</param>
        internal void AddTagToFilter(RefinementTagModel tag) {
            var rt = tag.RefinementTag;
            Filter.SelectedTags.Add(rt.Key,rt.TagWithPages);
        }

        /// <summary>
        ///     Clear the tag filter.
        /// </summary>
        public void ClearFilter() {
            SelectedTagModels.Clear();
            RefinementTagModels.ClearFilter();
        }

        /// <summary>
        ///     Reset the filter tags to a given collection of tags
        /// </summary>
        /// <param name="tags">Refinement tag collection.</param>
        public void ResetFilter(IEnumerable<RefinementTagModel> tags) {
            RefinementTagModels.ResetFilter(tags);
        }

        /// <summary>
        ///     Add given refinement tags to the filter,
        /// </summary>
        /// <param name="tags">tags to add to the filter</param>
        public void AddAllTagsToFilter(IEnumerable<RefinementTagModel> tags) {
            RefinementTagModels.AddAllTagsToFilter(tags);
        }
        /// <summary>
        ///     Handle chenges to the collection of refinement tags.
        /// </summary>
        /// <param name="sender">The collection which raised the changed event.</param>
        /// <param name="e">Change details.</param>
        private void TagSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateRefinementTagsPanelHeader();
        }

        /// <summary>
        ///     Handle changes to the tag filter.
        /// </summary>
        /// <remarks>
        ///     Keeps the <see cref="SelectedTagModels"/> in sync with the filter.
        /// </remarks>
        /// <param name="sender">
        ///     The collection of filter tags which raised the change event.
        /// </param>
        /// <param name="e">Chenge details</param>
        private void SelectedTags_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, TagPageSet> e) {
            switch (e.Action) {
                case NotifyDictionaryChangedAction.Add:
                    SelectedTagModels.AddAll(from t in e.Items
                                             select new SelectedTagModel() {
                                                 SelectableTag = RefinementTagModels[t.Key],
                                                 TagIndicator = "",
                                                 TagIndicatorColor = Brushes.Red
                                             });
                    break;
                case NotifyDictionaryChangedAction.Remove:
                    SelectedTagModels.RemoveAll(from t in e.Items select t.Key);
                    break;
                case NotifyDictionaryChangedAction.Reset:
                    SelectedTagModels.Clear();
                    SelectedTagModels.AddAll(from t in Filter.SelectedTags.Values
                                             select new SelectedTagModel() {
                                                 SelectableTag = RefinementTagModels[t.Key],
                                                 TagIndicator = "",
                                                 TagIndicatorColor = Brushes.Red
                                             });
                    break;
            }
            UpdateRefinementTagsPanelHeader();
        }

        /// <summary>
        ///     Get the filter strategy for this model.
        /// </summary>
        public TagFilterBase Filter { get; private set; }

        /// <summary>
        ///     Initialize a new instance of a backing model for the
        ///     <see cref="TagFilterPanel"/> control.
        /// </summary>
        /// <param name="onenote">OneNote application proxy.</param>
        /// <param name="filter">The filter strategy.</param>
        public TagFilterPanelModel(OneNoteProxy onenote,  TagFilterBase filter) {
            Filter = filter;
            ContextTagSource = new TagsAndPages(onenote);
            RefinementTagModels = new RefinementTagModelSource(filter);
            RefinementTagModels.CollectionChanged += TagSource_CollectionChanged;
            filter.SelectedTags.CollectionChanged += SelectedTags_CollectionChanged;
            UpdateRefinementTagsPanelHeader();
            OneNoteApp = onenote;
        }
    }
}