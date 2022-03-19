using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// The collection of tags below an `one:OE` content XML element
    /// </summary>
    [ComVisible(false)]
    public class TagCollection : PageObjectCollectionBase<OE,Tag>
    {
        HashSet<int> _tags = new HashSet<int>();
        /// <summary>
        /// Set the tags in this collection.
        /// </summary>
        /// <remarks>
        ///     Tags are identified by their tag definitions.
        /// </remarks>
        public IEnumerable<TagDef> Tags {
            set {

                var tagset = new HashSet<int>();
                foreach (var td in value) {
                    if (!td.IsDisposed) {
                        int i = td.Index;
                        tagset.Add(i);
                        if (!_tags.Contains(i)) {
                            // add that tag
                            Add(new Tag(Namespace, i));
                        }
                    }
                }

                // now that all necessary tags are on the page,
                // remove the obsolete ones
                for (int i = 0; i < Items.Count;) {
                    var tag = Items[i];
                    if (tagset.Contains(tag.Index)) {
                        i++; // next
                    } else {
                        Items.RemoveAt(i);
                        _tags.Remove(tag.Index);
                        tag.Remove();
                    }
                }
            }
        }
        /// <summary>
        /// Determine if the collection contains a tag with a given index.
        /// </summary>
        /// <param name="tag">Tag definition.</param>
        /// <returns>`true` if the tag is in the collection.</returns>
        public bool Contains(TagDef tag) => _tags.Contains(tag.Index);

        /// <summary>
        /// Remove a tag with a given index.
        /// </summary>
        /// <param name="tag">Definition of tag to remove.</param>
        /// <returns>`true` if the tag was sucessfully removed from the collection.</returns>
        public bool Remove(TagDef tag) {
            int i = tag.Index;
            if (_tags.Remove(i)) {
                int index = Items.FindIndex((t) => t.Index == i);
                if (index >= 0) {
                    Items[i].Remove();
                    Items.RemoveAt(index);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Initialize an instance of this collection using
        /// `one:Tag` elements below an OneSpace content element.
        /// </summary>
        /// <remarks>
        ///     New tags are added as first children of the XML content element.
        /// </remarks>
        /// <param name="oe">The `one:OE` element proxy object owning the colelction.</param>
        public TagCollection(OE oe) : base(oe.GetName("Tag"),oe) {
        }

        /// <summary>
        /// Get the last tag in the tag collection.
        /// </summary>
        /// <remarks>Primarily used to add other content at the right place</remarks>
        public Tag LastTag { get => Items.Count > 0 ? Items[Items.Count - 1] : null; }
        /// <summary>
        /// Add a new Tag proxy object to the collection.
        /// </summary>
        /// <param name="tag">Tag proxy pbject to add</param>
        protected override void Add(Tag tag) {
            if (_tags.Add(tag.Index)) {
                if (LastTag == null) {
                    Owner.Element.AddFirst(tag.Element);
                } else {
                    LastTag.Element.AddAfterSelf(tag.Element);
                }
                base.Add(tag);
            }
        }

        /// <summary>
        /// Add a tag by definition.
        /// </summary>
        /// <param name="tag">Definition of the tag to add.</param>
        public bool Add (TagDef tag) {
            int i = tag.Index;
            if (!_tags.Contains(tag.Index)) {
                // this is a new tag
                Add(new Tag(Namespace, i));
                return true;
            }
            else {
                return false;
            }
        }
        /// <summary>
        /// Create a tag proxy object for tags collected from an content element
        /// on a page document.
        /// </summary>
        /// <param name="e">The `one:Tag` XML element</param>
        /// <returns>A new instance of a proxy object for the tag element.</returns>
        protected override Tag CreateElementProxy(XElement e) {
            var t = new Tag(e);
            _tags.Add(t.Index);
            return t;
        }
    }
}
