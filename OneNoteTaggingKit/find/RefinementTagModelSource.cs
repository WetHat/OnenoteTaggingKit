using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    ///     Observable collection of view models of type
    ///     <see cref="RefinementTagModel"/>..
    /// </summary>
    /// <remarks>
    ///     The view model in this collection are used to back the UI for tag based
    ///     search refinment.
    /// </remarks>
    [ComVisible(false)]
    public class RefinementTagModelSource : FilterableTagsSource<RefinementTagModel>
    {
        TagFilterBase _filter;

        /// <summary>
        /// Initialize a instance of an observable collection of
        /// refinement tag view models from a set of OneNote pages.
        /// </summary>
        /// <param name="filter">Observable collection of OneNote page tags.</param>
        public RefinementTagModelSource(TagFilterBase filter) {
            _filter = filter;
            _filter.RefinementTags.CollectionChanged += RefinementTags_CollectionChanged;
            // Synthesize an initial event to initialize the state of whatever is listening.
            RefinementTags_CollectionChanged(
                this,
                new NotifyDictionaryChangedEventArgs<string, RefinementTagBase>(
                    _filter.RefinementTags.Values,
                    NotifyDictionaryChangedAction.Add));
        }

        RefinementTagModel MakeRefinementTagModel(RefinementTagBase rt) {
            var rtm = new RefinementTagModel(rt, OriginalDispatcher);
            rtm.PropertyChanged += RefinementTagPropertyChanged;
            return rtm;
        }

        private void RefinementTags_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, RefinementTagBase> e) {
            List<RefinementTagModel> models;
            switch (e.Action) {
                case NotifyDictionaryChangedAction.Add:
                    models = (from t in e.Items select MakeRefinementTagModel(t)).ToList();
                    OriginalDispatcher.Invoke(() => AddAll(models));
                    break;
                case NotifyDictionaryChangedAction.Remove:
                    OriginalDispatcher.Invoke(() => RemoveAll(from t in e.Items select t.Key));
                    break;
                case NotifyDictionaryChangedAction.Reset:
                    models = (from t in _filter.RefinementTags.Values select MakeRefinementTagModel(t)).ToList();
                    OriginalDispatcher.Invoke(() => {
                        Clear();
                        AddAll(models);
                    });
                    break;
            }
        }


        /// <summary>
        ///     Reset the filter to a given set of refinement tagss.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public void ResetFilter(IEnumerable<RefinementTagModel> tags) {
            var tagModels = new HashSet<RefinementTagModel>(tags);
            try {
                _handleRefinementTagPropertyChanges = false;
                foreach (var tps in _filter.SelectedTags) {
                    RefinementTagModel found;
                    if (TryGetValue(tps.Key, out found) && !tagModels.Contains(found)) {
                        found.IsSelected = false;
                    }
                }
                // Select the tags for refinement
                foreach (var t in tagModels) {
                    t.IsSelected = true;
                }

            } finally {
                _handleRefinementTagPropertyChanges = true;
            }
            _filter.SelectedTags.Reset(from tm in tagModels select tm.RefinementTag.TagWithPages);
        }

        public void AddAllTagsToFilter(IEnumerable<RefinementTagModel> tags) {
            var tagModels = new HashSet<RefinementTagModel>(tags);
            try {
                _handleRefinementTagPropertyChanges = false;
                // Select the tags for refinement
                foreach (var t in tagModels) {
                    t.IsSelected = true;
                }
            } finally {
                _handleRefinementTagPropertyChanges = true;
            }
            _filter.SelectedTags.UnionWith(from tm in tagModels select tm.RefinementTag.TagWithPages);
        }

        /// <summary>
        ///     Clear the tag filter.
        /// </summary>
        public void ClearFilter() {
            try {
                _handleRefinementTagPropertyChanges = false;
                foreach (var tps in _filter.SelectedTags) {
                    RefinementTagModel found;
                    if (TryGetValue(tps.Key, out found)) {
                        found.IsSelected = false;
                    }
                }
            } finally {
                _handleRefinementTagPropertyChanges = true;
            }

            _filter.SelectedTags.Clear();
        }

        bool _handleRefinementTagPropertyChanges = true;
        void RefinementTagPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (_handleRefinementTagPropertyChanges) {
                switch (e.PropertyName) {
                    case nameof(RefinementTagModel.IsSelected):
                        if (sender is RefinementTagModel mdl) {
                            if (mdl.IsSelected) {
                                _filter.SelectedTags.Add(mdl.RefinementTag.Key, mdl.RefinementTag.TagWithPages);
                            } else {
                                _filter.SelectedTags.Remove(mdl.RefinementTag.Key);
                            }
                        }
                        break;
                }
            }
        }
    }
}
