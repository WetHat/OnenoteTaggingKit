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
    public class DefinitionObjectBase : KeyedObjectBase, IDisposable {

        int _index;
        /// <summary>
        /// Get the index of the definition object
        /// </summary>
        public int Index {
            get => _index;
            set {
                _index = value;
                SetAttributeValue("index", value.ToString());
            }
        }

        /// <summary>
        /// Initialize a definition object proxy with a
        /// XML element existing on a OneNote page.
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
        /// <param name="element">
        ///     An XML element with an index attribute
        ///     on a OneNote page.
        /// </param>
        public DefinitionObjectBase(XElement element) : base(element) {
            _index = int.Parse(GetAttributeValue("index"));
        }

        /// <summary>
        /// Initialize a new keyed OneNote page element
        /// </summary>
        /// <param name="name">The Xml element name</param>
        /// <param name="key">the unique name</param>
        /// <param name="index">the index of the element</param>
        protected DefinitionObjectBase(XName name, int index, string key) : base(name, key) {
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
        public void Dispose() {
            if (!IsDisposed) {
                Element.Remove();
            }
            Element = null;
        }
        #endregion IDisposable
    }
}
