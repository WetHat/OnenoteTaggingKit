using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// An observable list of tags which are already known.
    /// </summary>
    /// <remarks>
    /// Tags in this list are typically used to suggest tags.
    /// </remarks>
    /// <typeparam name="T">A <see cref="FilterableTagModel"/> or a subclass of it.</typeparam>
    public class KnownTagsSource<T> : ObservableTagList<T> where T : FilterableTagModel,new()
    {
        /// <summary>
        /// Asynchronously load all known tags from the persisted settings.
        /// </summary>
        /// <returns>awaitable task object</returns>
        public async Task LoadKnownTagsAsync() {
            Clear();
            T[] mdls = await Task<T[]>.Run(() => LoadPersistedTags());
            AddAll(mdls);
        }

        private T[] LoadPersistedTags() {
            return (from string t in Properties.Settings.Default.KnownTagsCollection
                    select new T() { TagName = t} ).ToArray();
        }

        private TextSplitter _highlighter = new TextSplitter();

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
        /// Save the suggested tags to the add-in settings store.
        /// </summary>
        public void Save() {
            Properties.Settings.Default.KnownTagsCollection.Clear();
            Properties.Settings.Default.KnownTagsCollection.AddRange((from v in Values select v.TagName).ToArray());
        }

        /// <summary>
        /// Construct an instance of the collection of new tags.
        /// </summary>
        public KnownTagsSource() {
            CollectionChanged += KnownTagsSource_CollectionChanged;
        }

        /// <summary>
        /// Apply additional configuration to new items in the list.
        /// </summary>
        /// <param name="sender">List raising the event.</param>
        /// <param name="e">Event details.</param>
        private void KnownTagsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            if (e.Action == NotifyCollectionChangedAction.Add) {
                foreach (T mdl in e.NewItems) {
                    mdl.Highlighter = Highlighter;
                }
            }
        }

        #region IDisposable
        public override void Dispose() {
            base.Dispose();
        }
        #endregion IDisposable
    }
}
