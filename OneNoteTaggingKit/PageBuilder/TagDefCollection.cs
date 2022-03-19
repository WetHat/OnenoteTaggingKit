using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// A collection of tag definitions.
    /// </summary>
    /// <remarks>
    /// The collection does not compact. If
    /// </remarks>
    [ComVisible(false)]
    public class TagDefCollection : DefinitionObjectCollection<TagDef> {
        /// Get the definition for in-title marker tags.
        public TagDef InTitleMarkerDef { get; private set; }
        /// <summary>
        /// Get the definition for below-title marker tags.
        /// </summary>
        public TagDef BelowTitleMarkerDef { get; private set; }

        /// <summary>
        /// Get the definition for a marker denoting a saved search
        /// </summary>
        public TagDef SavedSearchMarkerDef { get; private set; }

        /// <summary>
        /// Define a tag participating in a specified process.
        /// </summary>
        /// <param name="tagname">The name of the tag (without type annotation).</param>
        /// <param name="classification">
        ///     Classification for the process tags based on this definition are
        ///     participating in.
        /// </param>
        /// <returns>A new tag definition.</returns>
        public TagDef DefineProcessTag(string tagname,TagProcessClassification classification) {
            TagDef newdef = null;
            switch (classification) {
                case TagProcessClassification.BelowTitleMarker:
                    if (BelowTitleMarkerDef == null) {
                        BelowTitleMarkerDef = newdef = new TagDef(Page, tagname, Items.Count, classification);
                    } else {
                        BelowTitleMarkerDef.Name = tagname;
                        return BelowTitleMarkerDef;
                    }
                    break;
                case TagProcessClassification.InTitleMarker:
                    if (InTitleMarkerDef == null) {
                        InTitleMarkerDef = newdef = new TagDef(Page, tagname, Items.Count, classification);
                    } else {
                        InTitleMarkerDef.Name = tagname;
                        return InTitleMarkerDef;
                    }
                    break;
                case TagProcessClassification.SavedSearchMarker:
                    if (SavedSearchMarkerDef == null) {
                        // TODO Localize
                        SavedSearchMarkerDef = newdef = new TagDef(Page, "Saved Search" , Items.Count, classification);
                    } else {
                        return SavedSearchMarkerDef;
                    }
                    break;
                default:
                    return null;
            }

            Add(newdef);
            return newdef;
        }

        /// <summary>
        /// Define the tags which should be on the page.
        /// </summary>
        /// <remarks>All definitions for tags not in the given list are removed</remarks>
        /// <param name="tags">list of tags.</param>
        public void DefinePageTags(PageTagSet tags) {
            var toDefine = new Stack<PageTag>(tags);

            foreach (var td in Items) {
                if (td.Tag != null) {
                    if (toDefine.Count > 0) {
                        // recycle
                        td.Tag = toDefine.Pop();
                    } else {
                        // tag definition not recyleable
                        td.Dispose();
                    }
                }
            }

            // For the remaining page tags we need to make new definitions.
            while (toDefine.Count > 0) {
                Add(new TagDef(Page, toDefine.Pop(), Items.Count));
            }

        }
        /// <summary>
        /// Intitialize the collection of tag definitions selected from an
        /// OneNote page document.
        /// </summary>
        /// <param name="page">The OneNote page document proxy.</param>
        public TagDefCollection(OneNotePage page) : base(page.GetName(nameof(TagDef)),page) {
        }
        /// <summary>
        /// Create a new instance of a tag definition proxy object
        /// and intialize it with a corresponting XML element from a
        /// OneNote page.
        /// </summary>
        /// <param name="e">XML tag definition element from a OneNote page.</param>
        /// <returns>Ne instance of a tag definition proxy object.</returns>
        protected override TagDef CreateElementProxy(XElement e) {
            var tagdef = new TagDef(Page,e);
            switch (tagdef.ProcessClassification) {
                case TagProcessClassification.BelowTitleMarker:
                    BelowTitleMarkerDef = tagdef;
                    break;
                case TagProcessClassification.InTitleMarker:
                    InTitleMarkerDef = tagdef;
                    break;
                case TagProcessClassification.SavedSearchMarker:
                    SavedSearchMarkerDef = tagdef;
                    break;
                default:
                    if (tagdef.Symbol == 0) {
                        // make sure the type is unique
                        tagdef.Type = Items.Count;
                    }
                    break;
            }
            // We do not pick up page tags here because we are not certain
            // that they qualify. May be somebody has used tags with symbol=0
            // somewhere on the page.
            return tagdef;
        }

        /// <summary>
        /// Get the collection of defined page tags and imported tags.
        /// </summary>
        /// <value>Collection of tag definitions for page tags and import tags.</value>
        public IEnumerable<TagDef> DefinedPageTags {
            get => from TagDef _ in Items
                   where  _.Tag != null
                   select _;
        }
        /// <summary>
        /// Add a new tag definition
        /// </summary>
        /// <param name="td">Tag definition proxy object.</param>
        protected override void Add(TagDef td) {
            if (td.Symbol == 0) {
                // make sure the type is unique
                td.Type = Items.Count;
            }
            base.Add(td);
        }
    }
}
