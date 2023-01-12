using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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
    public class RefinementTagsSource : FilterableTagsSource<RefinementTagModel>
    {
        TagFilterBase _filter;

        private CancellationTokenSource _cancelWorker = new CancellationTokenSource();

        /// <summary>
        /// Initialize a instance of an observable collection of
        /// refinement tag view models from a set of OneNote pages.
        /// </summary>
        /// <param name="filter">Observable collection of OneNote page tags.</param>
        public RefinementTagsSource(TagFilterBase filter) {
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
            OriginalDispatcher.Invoke(() => {
                switch (e.Action) {
                    case NotifyDictionaryChangedAction.Add:
                        AddAll(from t in e.Items select MakeRefinementTagModel(t));
                        break;
                    case NotifyDictionaryChangedAction.Remove:
                        RemoveAll(from t in e.Items select t.Key);
                        break;
                    case NotifyDictionaryChangedAction.Reset:
                        Clear();
                        break;
                }});
        }


        /// <summary>
        ///     Reset the filter to a given set of refinement tagss.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public Task ResetFilterAsync(IEnumerable<RefinementTagModel> tags) {
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
            return Task.Run(() => _filter.SelectedTags.Reset(from tm in tagModels select tm.RefinementTag.TagWithPages), _cancelWorker.Token);
        }

        public Task AddAllTagsToFilterAsync(IEnumerable<RefinementTagModel> tags) {
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
            return Task.Run(() => _filter.SelectedTags.UnionWith(from tm in tagModels select tm.RefinementTag.TagWithPages), _cancelWorker.Token);
        }

        /// <summary>
        ///     Clear the tag filter.
        /// </summary>
        public Task ClearFilterAsync() {
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

            return Task.Run(() => _filter.SelectedTags.Clear(), _cancelWorker.Token);
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
