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
            T[] mdls = await Task<T[]>.Run(() => LoadPersistedTags());
            AddAll(mdls);
        }

        private T[] LoadPersistedTags() {
            return (from string t in Properties.Settings.Default.KnownTagsCollection
                    where !string.IsNullOrWhiteSpace(t)
                    select new T() { TagName = t.Trim() } ).ToArray();
        }

        /// <summary>
        /// Save the suggested tags to the add-in settings store.
        /// </summary>
        public void Save() {
            Properties.Settings.Default.KnownTagsCollection.Clear();
            Properties.Settings.Default.KnownTagsCollection.AddRange((from v in Values select v.TagName).ToArray());
        }
    }
}
