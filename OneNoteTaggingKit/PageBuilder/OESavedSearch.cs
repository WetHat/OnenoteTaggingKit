﻿using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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
        static readonly Regex scopeRE = new Regex("(?<=scope=)(Section|SectionGroup|Notebook)", RegexOptions.Compiled);

        bool _updateRequired;
        OETable _searchConfiguration; // 2-column table with search parameters

        SearchScope _scope;

        string _query;

        OETaglist _tags;
        OET _lastModified;

        Cell _searchResult; // contains a list of page links
        /// <summary>
        /// Initialize a OneNote element proxy from a page element structure
        /// representing a saved search.
        /// </summary>
        /// <param name="searchConfig">The XML element on a OneNote page representing
        /// a _Saved Search_ configuration.</param>
        public OESavedSearch(XElement searchConfig) : base(searchConfig.Parent.Parent.Parent.Parent.Parent) {
            _searchConfiguration = new OETable(searchConfig);
            var configTbl = _searchConfiguration.Table;
            if  (configTbl != null && configTbl.Rows.Count > 3) {
                // Parse search scope
                var rowScope = configTbl.Rows[0];
                if (rowScope.Cells.Count > 1) {
                    var scopeCellContent = rowScope.Cells[1].Content.FirstOrDefault() as OET;
                    var m = scopeRE.Match(scopeCellContent.Text);
                    if (!string.IsNullOrEmpty(m.Value)) {
                        _scope = (SearchScope)Enum.Parse(typeof(SearchScope), m.Value);
                    } else {
                        _scope = SearchScope.AllNotebooks;
                    }
                }

                // parse query
                var rowQuery = configTbl.Rows[1];
                if (rowQuery.Cells.Count > 1) {
                    var queryCellContent = rowQuery.Cells[1].Content.FirstOrDefault() as OET;
                    if (queryCellContent != null) {
                        _query = OET.HTMLtag_matcher.Replace(queryCellContent.Text, string.Empty);
                    }
                }

                // parse taglist
                var rowTags = configTbl.Rows[2];
                if (rowTags.Cells.Count > 1) {
                    var tagsCellContent = rowTags.Cells[1].Content.FirstOrDefault() as OET;
                    if (tagsCellContent != null) {
                        _tags = new OETaglist(tagsCellContent);
                    }
                }

                // last modified
                var rowModified = configTbl.Rows[3];
                if (rowModified.Cells.Count > 1) {
                   _lastModified = rowModified.Cells[1].Content.FirstOrDefault() as OET;
                }

                _searchResult = Table.Rows[1].Cells[0];
            }
            _updateRequired = true;
        }

        IEnumerable<OET> GetPageLinks(OneNotePage page, IEnumerable<PageNode> pages) {
            XNamespace ns = page.Namespace;
            OneNoteProxy onenote = page.OneNoteApp;

            var pagelinks = new LinkedList<OET>();
            var citationsStyle = page.QuickStyleDefinitions.CitationStyleDef;
            foreach (var p in pages) {
                pagelinks.AddLast(new OET(ns,
                                          string.Format("<a href=\"{0}\">{1}",
                                          onenote.GetHyperlinkToObject(p.ID, string.Empty),
                                          p.Name)) {
                    Bullet = 15
                });
                if (_scope != SearchScope.Section) {
                    pagelinks.AddLast(new OET(ns, p.Breadcrumb, citationsStyle));
                }
            }

            if (pagelinks.Count == 0) {
                pagelinks.AddLast(new OET(ns, Properties.Resources.SavedSearchNoMatchError));
            }
            return pagelinks;
        }
        /// <summary>
        /// Initialize a _Saved Search_ proxy object with a new content
        /// structure.
        /// </summary>
        /// <param name="page">The OneNote page for embedding the saved search.</param>
        /// <param name="query">The full-text query.</param>
        /// <param name="marker">Marker tag definition.</param>
        /// <param name="scope">Search scope.</param>
        /// <param name="taglist">Comma Separated list of tags names.</param>
        /// <param name="pages">Collection of pages matching the tags and/or the query.</param>
        public OESavedSearch(OneNotePage page,
                             string query,
                             string taglist,
                             SearchScope scope,
                             TagDef marker,
                             IEnumerable<PageNode>pages)
            : base(new Table(page.Namespace, 1)) {
            _scope = scope;
            _query = query;

            var onenote = page.OneNoteApp;
            var ns = page.Namespace;
            _updateRequired = false;
            Table.BordersVisible = true;

            string scopeID = onenote.GetCurrentSearchScopeID(_scope);
            string scopelink;
            if (string.IsNullOrEmpty(scopeID)) {
                scopelink = Properties.Resources.SavedSearchAllScope;
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
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns, Properties.Resources.SavedSearchScopeLabel, labelstyle)),
                                                 new Cell(ns, new OET(ns, scopelink))));
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns, Properties.Resources.SavedSearchQueryLabel,labelstyle)),
                                                 new Cell(ns, new OET(ns, query))));
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns, Properties.Resources.SavedSearchTagsLabel, labelstyle)),
                                                 new Cell(ns, _tags = new OETaglist(ns,taglist))));
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
            if (_updateRequired) {
                if (_lastModified != null) {
                    _lastModified.Text = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                    var refinementTags = new PageTagSet(_tags.Taglist, TagFormat.AsEntered);
                    var pages = new Stack<PageNode>();
                    if (!refinementTags.IsEmpty || !string.IsNullOrWhiteSpace(_query)) {
                        var ph = new PageHierarchy(onenote, _scope, _query);

                        foreach (var p in ph.Pages) {
                            var pagetags = p.Tags;
                            if (refinementTags.All((t) => pagetags.Contains(t.Key))) {
                                pages.Push(p);
                            }
                        }
                    }
                    _searchResult.Content = GetPageLinks(page,
                                                         from p in pages
                                                         orderby p.Name ascending
                                                         select p);
                }
            }
        }
    }
}
