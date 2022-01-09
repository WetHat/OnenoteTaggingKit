using System.Collections.Generic;
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
        FilteredPages _pages;
        /// <summary>
        /// Initialize a instance of an observable collection of
        /// refinement tag view models.
        /// </summary>
        /// <param name="pages">Observable collection of OneNote page tags.</param>
        public RefinementTagsSource(FilteredPages pages) {
            _pages = pages;
            pages.Tags.CollectionChanged += Tags_CollectionChanged;
            Tags_CollectionChanged(
                this,
                new NotifyDictionaryChangedEventArgs<string, TagPageSet>(
                    pages.Tags.Values,
                    NotifyDictionaryChangedAction.Add));
        }

        private void Tags_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, TagPageSet> e) {
            switch (e.Action) {
                case NotifyDictionaryChangedAction.Add:
                    AddAll(from t in e.Items select new RefinementTagModel(t, RefinementPropertyChanged));
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
                            _pages.AddTagToFilter(mdl.PageTag);
                        } else {
                            _pages.RemoveTagFromFilter(mdl.PageTag);
                        }
                    }
                    break;
            }
        }
    }
}
