using System.Collections.Generic;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Collection of saved searches on a OneNote page.
    /// </summary>
    public class OESavedSearchCollection : PageObjectCollectionBase<OneNotePage,OESavedSearch>
    {
        /// <summary>
        /// Initialize a collection of saved searches found in the XML
        /// document of a OneNote page.
        /// </summary>
        /// <param name="page">The OneNote page proxy to select the saved searches from.</param>
        /// <param name="marker">
        ///     Definition of the tag marking search definitions.
        ///     If `null` the page is not scanned for existing saved searches.</param>
        public OESavedSearchCollection(OneNotePage page) :
                base(page.GetName("OE"),
                     page,
                     (xe) => new XElement[0]) {
        }

        /// <summary>
        /// Add saved searches found in an outline element of an OneNote page document.
        /// </summary>
        /// <param name="outline">The Outline element of an OneNOte page document.</param>
        /// <param name="marker">Definition of the tag marking search definitions.</param>
        /// <returns>`true` if ad least one saved search was added;</returns>
        public bool Add(XElement outline, TagDef marker) {
            bool added = false;
            XName tagName = outline.Name.Namespace.GetName("Tag");

            foreach (var tag in outline.Descendants(tagName)) {
                var indexAtt = tag.Attribute("index");
                if (indexAtt != null && marker.Index == (int)indexAtt) {
                    base.Add(new OESavedSearch(tag.Parent));
                    added = true;
                }
            }
            return added;
        }
        /// <summary>
        /// Add a proxy for a new updatable tag search element structure to this collection
        /// and to the OneNote page.
        /// </summary>
        /// <param name="query">The search string.</param>
        /// <param name="tags">Collection of tags to search for.</param>
        /// <param name="scope">Search scope.</param>
        /// <param name="pages">Pages matching the search string and/or tags</param>

        public void Add(string query, IEnumerable<string> tags, SearchScope scope, IEnumerable<TaggedPage>pages) {
            OESavedSearch ss = new OESavedSearch(Owner,
                                                 query,
                                                 tags,
                                                 scope,
                                                 Owner.DefineProcessTag("Saved Search", TagProcessClassification.SavedSearchMarker),
                                                 pages);
            base.Add(ss);
            Owner.Element.Add(new XElement(GetName("Outline"),
                                  new XElement(GetName("OEChildren"),
                                        ss.Element)));
        }


        /// <summary>
        /// Create a new proxy for a Saved Search.
        /// </summary>
        /// <param name="e">A content element with an embedded OneNote table.</param>
        /// <returns>New proxy instance for the saved search table</returns>
        protected override OESavedSearch CreateElementProxy(XElement e) {
            return new OESavedSearch(e);
        }

        /// <summary>
        /// Update all saved searches.
        /// </summary>
        public void Update() {
            foreach (var ss in Items) {
                ss.Update(Owner);
            }
        }
    }
}
