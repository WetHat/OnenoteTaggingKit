using System.Linq;
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
        /// <param name="page">The OneNote page Xml document.</param>
        /// <param name="position">THe position of elemnts of this type on a OneNote page.</param>
        public DefinitionObjectCollection(XDocument page,PagePosition position) : base (page,position) {
            foreach (var m in page.Root.Elements(ElementName)) {
                Items.InsertRange(0, from xe in page.Root.Elements(ElementName)
                                        select CreateElement(xe));
            }
        }

        /// <summary>
        /// Dispose a definition.
        /// </summary>
        /// <remarks>
        ///     The XML element associated with the item is removed from the
        ///     page, but the item's slot in the collection is not recycled.
        /// </remarks>
        /// <param name="index">Index of the item to dispose</param>
        public void Dispose(int index) {
            Items[index].Dispose();
        }

        /// <summary>
        /// Add a new proxy obkect to the end of the collection.
        /// </summary>
        /// <param name="proxy"></param>
        public override void Add(T proxy) {
            proxy.Index = Items.Count;
        }
    }
}
