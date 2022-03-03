using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy class for a OneNpte element structure representing
    /// a <i>Saved Search</i>.
    /// </summary>
    public class OESavedSearch : OETable
    {
        bool _updateRequired;
        OETable _searchConfiguration; // 2-column table with search parameters

        Cell _searchResult; // contains a list of page links
        /// <summary>
        /// Initialize a OneNote element proxy from a page element structure
        /// representing a saved search.
        /// </summary>
        /// <param name="element">The XML element on a OneNote page representing
        /// a _Saved Search_ configuration.</param>
        public OESavedSearch(XElement element) : base(element.Parent.Parent.Parent.Parent.Parent) {
            _searchConfiguration = new OETable(element);
            _searchResult = Table.Rows[1].Cells[0];
            _updateRequired = true;
        }

        /// <summary>
        /// Initialize a _Saved Search_ proxy object with a new content
        /// structure.
        /// </summary>
        /// <param name="page">The OneNote page for embedding the saved search.</param>
        /// <param name="query">The full-text query.</param>
        /// <param name="marker">Marker tag definition.</param>
        /// <param name="scope">Search scope.</param>
        /// <param name="tags">Refinement tags.</param>
        /// <param name="pages">Collection of pages matching the tags and/or the query.</param>
        public OESavedSearch(OneNotePage page,
                             string query,
                             IEnumerable<string> tags,
                             SearchScope scope,
                             TagDef marker,
                             IEnumerable<TaggedPage>pages)
            : base(new Table(page.Namespace, 1)) {
            var onenote = page.OneNoteApp;
            var ns = page.Namespace;
            _updateRequired = false;
            Table.BordersVisible = true;

            string scopeID;
            switch (scope) {
                case SearchScope.Notebook:
                    scopeID = onenote.CurrentNotebookID;
                    break;

                case SearchScope.SectionGroup:
                    scopeID = string.IsNullOrEmpty(onenote.CurrentSectionGroupID)
                        ? onenote.CurrentNotebookID
                        : onenote.CurrentSectionGroupID;
                    break;

                case SearchScope.Section:
                    scopeID = onenote.CurrentSectionID;
                    break;

                default:
                    scopeID = string.Empty;
                    break;
            }

            string scopelink;
            if (string.IsNullOrEmpty(scopeID)) {
                scopelink = "All Notebooks"; // TODO localize
            } else {
                var hn = new HierarchyNode(onenote.GetHierarchy(scopeID,HierarchyScope.hsSelf).Root,null);
                scopelink = string.Format("<a href=\"{0}&scope={1}\">{2}</a>",
                                          onenote.GetHyperlinkToObject(hn.ID, string.Empty),
                                          hn.NodeType.ToString(),
                                          hn.Name);
            }
            Table searchConfig = new Table(ns, 2);
            var labelstyle = page.QuickStyleDefinitions.LabelStyleDef;

            searchConfig.BordersVisible = true;
            // TODO: localize
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns, "Scope", labelstyle)),
                                                 new Cell(ns, new OET(ns, scopelink))));
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns, "Query",labelstyle)),
                                                 new Cell(ns, new OET(ns, query))));
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns, "Tags", labelstyle)),
                                                 new Cell(ns, new OETaglist(ns,tags))));
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns, "Updated", labelstyle)),
                                                 new Cell(ns, new OET(ns, DateTime.Now.ToString(CultureInfo.CurrentCulture)))));
            _searchConfiguration = new OETable(searchConfig);
            _searchConfiguration.Tags.Add(marker);

            Table.Rows.AddRow(new Row(ns, new Cell(ns, _searchConfiguration)));
            var pagelinks = new LinkedList<OE>();
            var citationsStyle = page.QuickStyleDefinitions.CitationStyleDef;
            foreach (var p in pages) {
                pagelinks.AddLast(new OET(ns,
                                          string.Format("<a href=\"{0}\">{1}",
                                          onenote.GetHyperlinkToObject(p.ID, string.Empty),
                                          p.Name)) {
                                        Bullet = 15
                                  });
                if (scope != SearchScope.Section) {
                    pagelinks.AddLast(new OET(ns, p.Breadcrumb, citationsStyle));
                }
            }

            if (pagelinks.Count == 0) {
                pagelinks.AddLast(new OET(ns, "No pages match the search criteria!")); // TODO localize
            }

            Table.Rows.AddRow(new Row(ns, new Cell(ns, pagelinks)));
        }
    }
}
