using System.Collections.Generic;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// The collection of tags below an `one:OE` content XML element
    /// </summary>
    public class TagCollection : PageObjectCollectionBase<Tag>
    {
        /// <summary>
        /// The 'one:OE' content element owning the tag.
        /// </summary>
        OE OE{ get; }

        HashSet<int> _tags = new HashSet<int>();
        /// <summary>
        /// Get/set the tags in this collection.
        /// </summary>
        /// <remarks>
        ///     Tags are identified by their tag definition index.
        /// </remarks>
        public IEnumerable<int> TagIndices {
            get => _tags;
            set {
                var tagset = new HashSet<int>(value);
                foreach (var ti in tagset) {
                    if (!_tags.Contains(ti)) {
                        // add that tag
                        Add(new Tag(OE, ti));
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
        /// <param name="i">Tag index.</param>
        /// <returns>`true` if the tag is in the collection.</returns>
        public bool ContainsTag(int i) => _tags.Contains(i);

        /// <summary>
        /// Remove a tag with a given index.
        /// </summary>
        /// <param name="i">Tag index.</param>
        /// <returns>`true` if the tag was sucessfully removed from the collection.</returns>
        public bool RemoveTag(int i) {
            if (_tags.Remove(i)) {
                int index = Items.FindIndex((t) => t.Index == i);
                if (index >= 0) {
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
        /// <param name="oe">The `one:OE` element proxy object.</param>
        public TagCollection(OE oe) : base(oe.GetName("Tag"),oe.Element) {
            OE = oe;
        }

        /// <summary>
        /// Add a new Tag proxy object to the collection.
        /// </summary>
        /// <param name="proxy">Tag proxy pbject to add</param>
        protected override void Add(Tag proxy) {
            base.Add(proxy);
            _tags.Add(proxy.Index);
            OE.Element.AddFirst(proxy.Element);

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
