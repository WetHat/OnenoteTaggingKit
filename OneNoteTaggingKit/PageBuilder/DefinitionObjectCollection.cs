using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// An abstract base class for definition object collections.
    /// </summary>
    /// <remarks>
    ///     When items get removed from the collection, the corresponding XML
    ///     elements get removed from the page but their slot in the collections
    ///     does not get recycled, so that all other items retain their index.
    /// </remarks>
    /// <typeparam name="T">Proxy object type.</typeparam>
    public abstract class DefinitionObjectCollection<T> : PageStructureObjectCollection<T> where T: DefinitionObjectBase
    {
        /// <summary>
        /// Initialize an instance of this collection for elements with a
        /// given name found on a OneNote page XML document .
        /// </summary>
        /// <param name="name">XML name of the elements in this collection.</param>
        /// <param name="page">The OneNote page Xml document.</param>
        public DefinitionObjectCollection(XName name,OneNotePage page) : base (name,page) {
        }

        /// <summary>
        /// Add a new proxy object to the end of the collection.
        /// </summary>
        /// <param name="proxy"></param>
        protected override void Add(T proxy) {
            proxy.Index = Items.Count;
            base.Add(proxy);
        }

        /// <summary>
        /// Get the definition proxy object at a given list position.
        /// </summary>
        /// <param name="index">Index of the proxy object.</param>
        /// <returns></returns>
        public T this[int index] => Items[index];
    }
}
