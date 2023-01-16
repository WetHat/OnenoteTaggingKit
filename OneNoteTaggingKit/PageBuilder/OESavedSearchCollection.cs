using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Collection of saved searches on a OneNote page.
    /// </summary>
    [ComVisible(false)]
    public class OESavedSearchCollection : PageObjectCollectionBase<OneNotePage,OESavedSearch>
    {
        /// <summary>
        /// Initialize a collection of saved searches found in the XML
        /// document of a OneNote page.
        /// </summary>
        /// <param name="page">The OneNote page proxy to select the saved searches from.</param>
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
                    base.Add(new OESavedSearch(Owner.OneNoteApp, tag.Parent));
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
        /// <param name="withall">Set of tags to search for.</param>
        /// <param name="scope">Search scope.</param>
        /// <param name="pages">Pages matching the search string and/or tags</param>

        public void Add(string query, PageTagSet withall, SearchScope scope, IEnumerable<PageNode>pages) {
            OESavedSearch ss = new OESavedSearch(Owner,
                                                 query,
                                                 string.Join(", ",from t in withall
                                                                  orderby t.Key ascending
                                                                  select t.ToString()),
                                                 scope,
                                                 Owner.DefineProcessTag(Properties.Resources.SavedSearchTagName, TagProcessClassification.SavedSearchMarker),
                                                 pages);
            base.Add(ss);
            Owner.Element.Add(new XElement(GetName("Outline"),
                                  new XElement(GetName("OEChildren"),
                                        ss.Element)));
        }

        /// <summary>
        /// Create a new proxy for a Saved Search.
        /// </summary>
        /// <param name="searchConfig">A content element with an embedded OneNote search configuration table.</param>
        /// <returns>New proxy instance for the saved search table</returns>
        protected override OESavedSearch CreateElementProxy(XElement searchConfig) {
            return new OESavedSearch(Owner.OneNoteApp,searchConfig);
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
