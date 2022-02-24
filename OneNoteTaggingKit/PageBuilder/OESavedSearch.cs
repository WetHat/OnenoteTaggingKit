using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy class for a OneNpte element structure representing
    /// a <i>Saved Search</i>.
    /// </summary>
    public class OESavedSearch : OETable
    {
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
        }


        /// <summary>
        /// Initialize a  _Saved Search_ proxy object with a new content
        /// structure,
        /// </summary>
        /// <param name="ns">The XML namespace to create the _Saved Search_ element structure in.</param>
        /// <param name="marker">Marker tag definition.</param>
        /// <param name="scope">Search scope</param>
        /// <param name="tags">Refinement tags.</param>
        public OESavedSearch(XNamespace ns, TagDef marker, string scope, IEnumerable<string>tags)
            : base(new Table(ns, 1)) {
            Tags.Add(marker);
            Table searchConfig = new Table(ns, 2);
            // TODO: localize
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns,
                                                 new OET(ns,"Tags"),
                                                 new OET(ns,string.Join(", ",tags)))));
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns,
                                                 new OET(ns, "Scope"),
                                                 new OET(ns, scope))));
            searchConfig.Rows.AddRow(new Row(ns, new Cell(ns,
                                                 new OET(ns, "Updated"),
                                                 new OET(ns, DateTime.Now.ToString(CultureInfo.CurrentCulture)))));
            _searchConfiguration = new OETable(searchConfig);
            Table.Rows.AddRow(new Row(ns, new Cell(ns, _searchConfiguration)));
            Table.Rows.AddRow(new Row(ns, new Cell(ns, new OET(ns,"No matching pages found!"))));
        }
    }
}
