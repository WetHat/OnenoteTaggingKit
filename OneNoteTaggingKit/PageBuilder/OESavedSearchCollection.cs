using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        static IEnumerable<XElement> SavedSearchCollector(XElement page, TagDef marker) {
            if (marker != null) {
                XName tagName = page.Name.Namespace.GetName("Tag");

                foreach (var tag in page.Descendants(tagName)) {
                    var indexAtt = tag.Attribute("index");
                    if (indexAtt != null && marker.Index == (int)indexAtt) {
                        yield return tag.Parent;
                    }
                }
            }
        }
        /// <summary>
        /// Initialize a collection of saved searches found in the XML
        /// document of a OneNote page.
        /// </summary>
        /// <param name="page">The OneNote page proxy to select the saved searches from.</param>
        /// <param name="marker">
        ///     Definition of the tag marking search definitions.
        ///     If `null` the page is not scanned for existing saved searches.</param>
        public OESavedSearchCollection(OneNotePage page, TagDef marker = null) :
                base(page.GetName("OE"),
                     page,
                     (xe) => SavedSearchCollector(xe,marker)) {
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
            OESavedSearch ss = new OESavedSearch(Namespace,
                                                 Owner.OneNoteApp,
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
    }
}
