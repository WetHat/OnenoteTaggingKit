using System;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Base class for indexed definition proxy objects.
    /// </summary>
    /// <remarks>
    ///     Definition objects are referred to by index by page content
    ///     elements.
    /// </remarks>
    public class DefinitionObjectBase : NamedObjectBase, IDisposable {

        /// <summary>
        /// Get/set the index of the definition object.
        /// </summary>
        public int Index {
            get {
                string value = GetAttributeValue("index");
                return value != null ? int.Parse(value) : default(int);
            }
            set {
                SetAttributeValue("index", value.ToString());
            }
        }

        /// <summary>
        /// Initialize a definition element proxy with a
        /// XML element selected from an OneNote page document.
        /// </summary>
        /// <remarks>
        ///     Definition elements must have:
        ///     <list type="bullet">
        ///         <item>a `name` property containing a key
        ///               which is unique to the page.</item>
        ///         <item>an `index` property containing a unique index (integer)
        ///               which is used by content elements to refer to the
        ///               definition</item>
        ///     </list>
        /// </remarks>
        /// <param name="page">Proxy of the page which owns this object.</param>
        /// <param name="element">
        ///     An XML element with an index attribute
        ///     on a OneNote page.
        /// </param>
        protected DefinitionObjectBase(OneNotePage page,XElement element) : base(page,element) {
        }

        /// <summary>
        /// Initialize a new keyed OneNote page element
        /// </summary>
        /// <remarks>
        ///     The index of the object is supposed to be defined by the owning
        ///     collection.
        /// </remarks>
        /// <param name="page">Proxy of the page which owns this object.</param>
        /// <param name="element">The Xml element for this proxy.</param>
        /// <param name="name">the value of the `name` attribute.</param>
        /// <param name="index">Defintion index.</param>
        protected DefinitionObjectBase(OneNotePage page,XElement element, string name, int index) : base(page,element, name) {
            Index = index;
        }

        #region IDisposable
        /// <summary>
        /// Determine if this definition has been dsposed.
        /// </summary>
        public bool IsDisposed => Element == null;
        /// <summary>
        /// Dispose a definition object.
        /// </summary>
        public virtual void Dispose() {
            if (!IsDisposed) {
                Remove();
            }
            Element = null;
        }
        #endregion IDisposable
    }
}
