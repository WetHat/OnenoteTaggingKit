using System.Collections.Specialized;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// An observable list of tag related view models which can be filtered
    /// and highlighted by applying search criteria.
    /// </summary>
    /// <typeparam name="T">View model type</typeparam>
    public class FilterableTagsSource<T> : ObservableTagList<T> where T : FilterableTagModel
    {
        TextSplitter _highlighter = new TextSplitter();

        /// <summary>
        /// Get/set the text highlighter applied to all tag models in this list.
        /// </summary>
        public TextSplitter Highlighter {
            get => _highlighter;
            set {
                _highlighter = value;
                foreach (T mdl in this) {
                    mdl.Highlighter = value;
                }
            }
        }

        /// <summary>
        /// Apply additional configuration to new items in the list.
        /// </summary>
        /// <param name="sender">List raising the event.</param>
        /// <param name="e">Event details.</param>
        private void FilterableTagsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add) {
                foreach (T mdl in e.NewItems) {
                    mdl.Highlighter = Highlighter;
                }
            }
        }
        /// <summary>
        /// Construct an instance of the collection of new tags.
        /// </summary>
        public FilterableTagsSource() {
            CollectionChanged += FilterableTagsSource_CollectionChanged;
        }
    }
}
