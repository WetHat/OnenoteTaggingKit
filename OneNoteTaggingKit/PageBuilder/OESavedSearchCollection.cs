using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Collection of saved searches on a OneNote page.
    /// </summary>
    public class OESavedSearchCollection : PageObjectCollectionBase<OneNotePage,OESavedSearch>
    {

        static IEnumerable<XElement> SavedSearchCollector(XElement page, TagDef marker) {
            XName tagName = page.Name.Namespace.GetName("Tag");

            foreach (var tag in page.Descendants(tagName)) {
                var indexAtt = tag.Attribute("index");
                if (indexAtt != null && marker.Index == (int)indexAtt) {
                    yield return tag.Parent;
                }
            }
        }
        public OESavedSearchCollection(OneNotePage page, TagDef marker) :
                base(page.GetName("OE"),
                     page,
                     (xe) => SavedSearchCollector(page.Element,marker)) {
        }

        /// <summary>
        /// Add a _Saved Search_ pproxy element to this collection.
        /// </summary>
        /// <param name="search"></param>
        public void AddSearch(OESavedSearch search) {
            Add(search);
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
