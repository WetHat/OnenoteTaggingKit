using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy class for a OneNpte element structure representing
    /// a <i>Saved Search</i>.
    /// </summary>
    public class OESavedSearch : OETable
    {
        static readonly string allNotebookEmoji = "&#128218;"; // 📚
        static readonly string notebookEmoji = "&#128217;"; // 🗄
        static readonly string sectionGroupEmoji = "&#128450;"; // 🗂
        static readonly string sectionEmoji = "&#128464;"; // 🗐

        OETable _searchConfiguration; // 2-column table with search parameters

        OneNoteProxy _onenote;
        SearchScope _scope;

        string _query;

        OETaglist _withAllTags;
        OETaglist _withoutTags;
        OETaglist _withAnyTags;
        OET _lastModified;

        Cell _searchResult; // contains a list of page links
        /// <summary>
        /// Initialize a OneNote element proxy from a page element structure
        /// representing a saved search.
        /// </summary>
        /// <param name="onenote">OneNote proxy object.</param>
        /// <param name="searchConfig">The XML element on a OneNote page representing
        /// a _Saved Search_ configuration.</param>
        public OESavedSearch(OneNoteProxy onenote, XElement searchConfig) : base(searchConfig.Parent.Parent.Parent.Parent.Parent) {
            _onenote = onenote;
            _searchConfiguration = new OETable(searchConfig);
            var configTbl = _searchConfiguration.Table;
            if  (configTbl != null && configTbl.Rows.Count > 3) {
                // Parse search scope
                var rowScope = configTbl.Rows[0];
                if (rowScope.Cells.Count > 1) {
                    var scopeCellContent = rowScope.Cells[1].Content.FirstOrDefault() as OET;

                    var scopeTxt = scopeCellContent.Text;
                    string scopeID;
                    if (scopeTxt.Contains(notebookEmoji)) {
                        _scope = SearchScope.Notebook;
                        scopeID = _onenote.CurrentNotebookID;
                    } else if (scopeTxt.Contains(sectionGroupEmoji)) {
                        _scope = SearchScope.SectionGroup;
                        scopeID = _onenote.CurrentSectionGroupID;
                        if (string.IsNullOrEmpty(scopeID)) {
                            scopeID = _onenote.CurrentSectionID;
                        }
                    } else if (scopeTxt.Contains(sectionEmoji)) {
                        _scope = SearchScope.Section;
                        scopeID = _onenote.CurrentSectionID;
                    } else {
                        _scope = SearchScope.AllNotebooks;
                        scopeID = string.Empty;
                    }
                    scopeCellContent.Text = formatScopeLink(scopeID);
                }
                // parse query
                var rowQuery = configTbl.Rows[1];
                if (rowQuery.Cells.Count > 1) {
                    var queryCellContent = rowQuery.Cells[1].Content.FirstOrDefault() as OET;
                    if (queryCellContent != null) {
                        _query = OET.HTMLtag_matcher.Replace(queryCellContent.Text, string.Empty);
                    }
                }

                // parse 'with all' taglist
                var withAllTagsRow = configTbl.Rows[2];
                if (withAllTagsRow.Cells.Count > 1) {
                    var tagsCellContent = withAllTagsRow.Cells[1].Content.FirstOrDefault() as OET;
                    if (tagsCellContent != null) {
                        _withAllTags = new OETaglist(tagsCellContent);
                    }
                }
                // parse 'with any' taglist, if present
                if (configTbl.Rows.Count > 5) {
                    var exceptWithTagsRow = configTbl.Rows[3];
                    if (exceptWithTagsRow.Cells.Count > 1) {
                        var tagsCellContent = exceptWithTagsRow.Cells[1].Content.FirstOrDefault() as OET;
                        if (tagsCellContent != null) {
                            _withoutTags = new OETaglist(tagsCellContent);
                        }
                    }
                    var withAnyTagsRow = configTbl.Rows[4];
                    if (withAnyTagsRow.Cells.Count > 1) {
                        var tagsCellContent = withAnyTagsRow.Cells[1].Content.FirstOrDefault() as OET;
                        if (tagsCellContent != null) {
                            _withAnyTags = new OETaglist(tagsCellContent);
                        }
                    }
                }

                // last modified
                var rowModified = configTbl.Rows[configTbl.Rows.Count - 1];
                if (rowModified.Cells.Count > 1) {
                   _lastModified = rowModified.Cells[1].Content.FirstOrDefault() as OET;
                }

                _searchResult = Table.Rows[1].Cells[0];
            }
        }

        IEnumerable<OE> GetPageLinks(OneNotePage page, IEnumerable<PageNode> pages) {
            XNamespace ns = page.Namespace;
            OneNoteProxy onenote = page.OneNoteApp;

            var pagelinks = new LinkedList<OE>(); 
            var breadcrumbStyle = page.QuickStyleDefinitions.BreadcrumbStyleDef;
            var sectionTable = new SortedDictionary<string, LinkedList<OET>>();

            foreach (var p in pages) {
                if (p.IsInRecycleBin) {
                    continue;
                }
                // put into bucket
                var breadcrumb = p.Breadcrumb;

                LinkedList<OET> pagegroup;
                if (!sectionTable.TryGetValue(breadcrumb,out pagegroup)) {
                    // a section we have not seen so far.
                    pagegroup = new LinkedList<OET>();
                    sectionTable[breadcrumb] = pagegroup;
                }
                pagegroup.AddLast(new OET(ns,
                                          string.Format("<a href=\"{0}\">{1}",
                                          onenote.GetHyperlinkToObject(p.ID, string.Empty),
                                          p.Name)) {
                    Bullet = 15
                });
            }

            // return the result ordered by sections
            if (sectionTable.Count == 0) {
                pagelinks.AddLast(new OET(ns, Properties.Resources.SavedSearchNoMatchError));
            } else {
                foreach(var entry in sectionTable) {
                    if (_scope != SearchScope.Section) {
                        pagelinks.AddLast(new OET(ns, entry.Key, breadcrumbStyle) {
                                                Language = "yo",
                                                Children = new OEChildren(ns, entry.Value)
                                            });
                    } else {
                        // just add the links flat
                        foreach (var lnk in entry.Value) {
                            pagelinks.AddLast(lnk);
                        }
                    }
                }
            }
            return pagelinks;
        }

        /// <summary>
        /// Format the search scope in an identifiable manner.
        /// </summary>
        /// <param name="scopeID">OneNote object id of the hierarchy element defining the scope</param>
        /// <returns>HTML fragment containing an emoji identifier and a hyperlink.</returns>
        string formatScopeLink(string scopeID) {
            if (string.IsNullOrEmpty(scopeID)) {
                return string.Format("<span style=\"font-family:'Segoe UI Emoji'\">{0}</span>{1}",
                                     allNotebookEmoji,
                                     Properties.Resources.SavedSearchAllScope);
            }
            var hn = new HierarchyNode(_onenote.GetHierarchy(scopeID, HierarchyScope.hsSelf).Root, null);
            string markerEmoji;
            switch (hn.NodeType) {
                case HierarchyElement.heNotebooks:
                    markerEmoji = notebookEmoji;
                    break;
                case HierarchyElement.heSectionGroups:
                    markerEmoji = sectionGroupEmoji;
                    break;
                case HierarchyElement.heSections:
                    markerEmoji = sectionEmoji;
                    break;
                default:
                    markerEmoji = allNotebookEmoji;
                    break;
            }

            return string.Format("<span style=\"font-family:'Segoe UI Emoji'\">{0}</span><a href=\"{1}\">{2}</a>",
                                    markerEmoji,
                                    _onenote.GetHyperlinkToObject(hn.ID, string.Empty),
                                    hn.Name);
        }
        /// <summary>
        /// Initialize a _Saved Search_ proxy object with a new content
        /// structure.
        /// </summary>
        /// <param name="page">The OneNote page for embedding the saved search.</param>
        /// <param name="query">The full-text query.</param>
        /// <param name="marker">Marker tag definition.</param>
        /// <param name="scope">Search scope.</param>
        /// <param name="withAllTags">Comma Separated list of tags names which must be on pages.</param>
        /// <param name="withoutTags">Comma Separated list of tags names which must not be on pages.</param>
        /// <param name="withAnyTags">Comma Separated list of tags names where any of them must be on pages.</param>
        /// <param name="pages">Collection of pages matching the tags and/or the query.</param>
        public OESavedSearch(OneNotePage page,
                             SearchScope scope,
                             string query,
                             string withAllTags,
                             string withoutTags,
                             string withAnyTags,
                             TagDef marker,
                             IEnumerable<PageNode>pages)
            : base(new Table(page.Namespace, 1)) {
            _onenote = page.OneNoteApp;
            _scope = scope;
            _query = query;

            var ns = page.Namespace;
            Table.BordersVisible = true;

            Table searchConfig = new Table(ns, 2);
            var labelstyle = page.QuickStyleDefinitions.LabelStyleDef;

            searchConfig.BordersVisible = true;
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns, Properties.Resources.SavedSearchScopeLabel, labelstyle)),
                                                 new Cell(ns, new OET(ns, formatScopeLink(_onenote.GetCurrentSearchScopeID(_scope))))));
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns, Properties.Resources.SavedSearchQueryLabel,labelstyle)),
                                                 new Cell(ns, new OET(ns, query))));
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns, Properties.Resources.SavedSearchAllTagsLabel, labelstyle)),
                                                 new Cell(ns, _withAllTags = new OETaglist(ns,withAllTags))));
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns, Properties.Resources.SavedSearchExceptTagsLabel, labelstyle)),
                                                 new Cell(ns, _withoutTags = new OETaglist(ns, withoutTags))));
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns, Properties.Resources.SavedSearchAnyTagsLabel, labelstyle)),
                                                 new Cell(ns, _withAnyTags = new OETaglist(ns, withAnyTags))));
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns, Properties.Resources.SavedSearchUpdatedLabel, labelstyle)),
                                                 new Cell(ns, _lastModified = new OET(ns, DateTime.Now.ToString(CultureInfo.CurrentCulture)))));
            _searchConfiguration = new OETable(searchConfig);
            _searchConfiguration.Tags.Add(marker);

            Table.Rows.AddRow(new Row(ns, new Cell(ns, _searchConfiguration)));

            Table.Rows.AddRow(new Row(ns, new Cell(ns, GetPageLinks(page,pages))));
        }

        /// <summary>
        /// Bring the search result up-to-data.
        /// </summary>
        /// <param name="page">The OneNote page proxy.</param>
        public void Update(OneNotePage page) {
            OneNoteProxy onenote = page.OneNoteApp;
 
            if (_lastModified != null) {
                _lastModified.Text = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                var withAllTags = new PageTagSet(_withAllTags.Taglist, TagFormat.AsEntered);
                var withoutTags = new PageTagSet(_withoutTags == null ? string.Empty : _withoutTags.Taglist, TagFormat.AsEntered);
                var withAnyTags = new PageTagSet(_withAnyTags == null ? string.Empty : _withAnyTags.Taglist, TagFormat.AsEntered);
                var tagsWithPages = new TagsAndPages(onenote);

                if (!withAllTags.IsEmpty
                    || !withoutTags.IsEmpty
                    || !withAnyTags.IsEmpty) {
                    tagsWithPages.FindPages(_scope, _query);

                    // process required tags
                    if (!withAllTags.IsEmpty) {
                        foreach (var tag in withAllTags) {
                            TagPageSet tps;
                            if (tagsWithPages.Tags.TryGetValue(tag.Key, out tps)) {
                                tagsWithPages.Pages.IntersectWith(tps.Pages);
                            } else {
                                // tag not in the search result -> no result
                                tagsWithPages.Pages.Clear();
                            }
                        }
                    }

                    // process excluded tags
                    if (!withoutTags.IsEmpty && tagsWithPages.Pages.Count > 0) {
                        foreach (var tag in withoutTags) {
                            TagPageSet tps;
                            if (tagsWithPages.Tags.TryGetValue(tag.Key, out tps)) {
                                tagsWithPages.Pages.ExceptWith(tps.Pages);
                            }
                        }
                    }

                    // process 'with any of these' filter
                    if (!withAnyTags.IsEmpty && tagsWithPages.Pages.Count > 0) {
                        var pagesWithAnyTags = new HashSet<PageNode>();
                        foreach (var tag in withAnyTags) {
                            TagPageSet tps;
                            if (tagsWithPages.Tags.TryGetValue(tag.Key, out tps)) {
                                pagesWithAnyTags.UnionWith(tps.Pages);
                            }
                        }
                        tagsWithPages.Pages.IntersectWith(pagesWithAnyTags);
                    }
                }

                _searchResult.Content = GetPageLinks(page,
                                                        from p in tagsWithPages.Pages.Values
                                                        orderby p.Name ascending
                                                        select p);
            }
        }
    }
}
