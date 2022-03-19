using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    [ComVisible(false)]
    public class KnownTagsSource<T> : FilterableTagsSource<T> where T : FilterableTagModel, new()
    {
        /// <summary>
        /// Asynchronously load all known tags from the persisted settings.
        /// </summary>
        /// <returns>awaitable task object</returns>
        public async Task LoadKnownTagsAsync() {
            Clear();
            IEnumerable<T> mdls = await Task<IEnumerable<T>>.Run(() => LoadPersistedTags());
            AddAll(mdls);
        }

        private IEnumerable<T> LoadPersistedTags() {
            return from pt in new PageTagSet(from string n in Properties.Settings.Default.KnownTagsCollection select n, TagFormat.AsEntered)
                   select new T() { Tag = pt };
        }

        /// <summary>
        /// Save the suggested tags to the add-in settings store.
        /// </summary>
        public void Save() {
            Properties.Settings.Default.KnownTagsCollection.Clear();
            Properties.Settings.Default.KnownTagsCollection.AddRange((from v in Values select v.Tag.ToString()).ToArray());
        }
    }
}
