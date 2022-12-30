using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Observable collection of view models of type
    /// <see cref="RefinementTagModel"/>.
    /// </summary>
    [ComVisible(false)]
    public class RefinementTagsSource : FilterableTagsSource<RefinementTagModel>
    {
        PagesWithAllTags _filteredPages;
        /// <summary>
        /// Initialize a instance of an observable collection of
        /// refinement tag view models from a set of OneNote pages.
        /// </summary>
        /// <param name="pages">Observable collection of OneNote page tags.</param>
        public RefinementTagsSource(PagesWithAllTags pages) {
            _filteredPages = pages;
            pages.RefinementTags.CollectionChanged += Tags_CollectionChanged;
            Tags_CollectionChanged(
                this,
                new NotifyDictionaryChangedEventArgs<string, RefinementTag>(
                    pages.RefinementTags.Values,
                    NotifyDictionaryChangedAction.Add));
        }

        RefinementTagModel MakeRefinementTagModel(RefinementTag rt) {
            var rtm = new RefinementTagModel(rt, OriginalDispatcher);
            rtm.PropertyChanged += RefinementPropertyChanged;
            return rtm;
        }

        private void Tags_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, RefinementTag> e) {
            switch (e.Action) {
                case NotifyDictionaryChangedAction.Add:
                    AddAll(from t in e.Items select MakeRefinementTagModel(t));
                    break;
                case NotifyDictionaryChangedAction.Remove:
                    RemoveAll(from t in e.Items select t.TagName);
                    break;
                case NotifyDictionaryChangedAction.Reset:
                    Clear();
                    break;
            }
        }

        void RefinementPropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(RefinementTagModel.IsSelected):
                    if (sender is RefinementTagModel mdl) {
                        if (mdl.IsSelected) {
                            _filteredPages.SelectedTags.Add(mdl.PageTag.Key, mdl.PageTag);
                        } else {
                            _filteredPages.SelectedTags.Remove(mdl.PageTag.Key);
                        }
                    }
                    break;
            }
        }
    }
}
