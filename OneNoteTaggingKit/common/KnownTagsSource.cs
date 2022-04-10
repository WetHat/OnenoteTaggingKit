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

        OneNoteProxy _onenote;

        /// <summary>
        /// Initialize the source of known tags.
        /// </summary>
        /// <param name="onenote">The _OneNote_ application object.</param>
        public KnownTagsSource(OneNoteProxy onenote) {
            _onenote = onenote;
        }

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
            return from pt in _onenote.KnownTags select new T() { Tag = pt };
        }

        /// <summary>
        /// Save the current set of suggested tags to the add-in settings store.
        /// </summary>
        public void Save() {
            _onenote.KnownTags.UnionWith(from t in this.Values select t.Tag);
            _onenote.SaveSettings();
        }
    }
}
