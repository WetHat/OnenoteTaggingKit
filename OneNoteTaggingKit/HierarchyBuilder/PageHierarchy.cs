using Microsoft.Office.Interop.OneNote;
using System.Collections.Generic;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.PageBuilder;

namespace WetHatLab.OneNote.TaggingKit.HierarchyBuilder
{
    /// <summary>
    /// Proxy representation of a OneNote page hierarchy.
    /// </summary>
    public class PageHierarchy {

        OneNoteProxy _onenote;

        Stack<PageNode> _pages = new Stack<PageNode>();
        /// <summary>
        /// Get the pages (leaf nodes) of the hierarchy.
        /// </summary>
        public IEnumerable<PageNode> Pages { get => _pages; }

        /// <summary>
        /// Add pages of a XML page hierarchy.
        /// </summary>
        /// <remarks>
        /// The hierarchy XML document typically has the form.
        /// <code lang="xml">
        /// <one:Notebooks xmlns:one="http://schemas.microsoft.com/office/onenote/2013/onenote">
        ///   <one:Notebook name="CoCreate" nickname="CoCreate (CADM)" ID="{E47C9414-AC84-4002-92B2-47107B2E818A}{1}{B0}" path="https://d.docs.live.net/3e9574def72409d3/CoCreate/CoCreate/" lastModifiedTime="2022-01-31T20:12:44.000Z" color="#4DBCCA" isUnread="true">
        ///     <one:SectionGroup name="Explorations" ID="{449F050E-4399-4980-B7ED-F0F2B30E4AF9}{1}{B0}" path="https://d.docs.live.net/3e9574def72409d3/CoCreate/CoCreate/Explorations/" lastModifiedTime="2020-12-18T07:45:15.000Z">
        ///       <one:Section name="CADM Infrastructure" ID="{2FA28918-6F42-0784-2CAE-7DD7B07B8F95}{1}{B0}" path="https://d.docs.live.net/3e9574def72409d3/CoCreate/CoCreate/Explorations/CADM Infrastructure.one" lastModifiedTime="2020-12-18T07:45:15.000Z" color="#91BAAE">
        ///         <one:Page ID="{2FA28918-6F42-0784-2CAE-7DD7B07B8F95}{1}{E19559522684634106918620142312867471939925861}" name="Subversion (SVN) Hosting Comparison Review Chart" dateTime="2015-01-30T20:15:26.000Z" lastModifiedTime="2017-02-11T12:01:59.000Z" pageLevel="1">
        ///           <one:Meta name="TaggingKit.PageTags" content="Comparison, Hosting, Sourcecode Management, Subversion, SVN" />
        ///         </one:Page>
        ///       </one:Section>
        ///     </one:SectionGroup>
        ///   </one:Notebook>
        /// </one:Notebooks>
        /// </code>
        /// </remarks>
        /// <param name="hierarchy">
        ///     A OneNote page hierarchy XML document. Usually the return value
        ///     of a call to <see cref="OneNoteProxy.GetHierarchy(string, HierarchyScope)"/>
        /// </param>
        public void AddPages(XDocument hierarchy) {
            buildHierarchy(hierarchy.Root, null);
        }

        /// <summary>
        /// Add pages returned by a search..
        /// </summary>
        /// <param name="scope">The search scope</param>
        /// <param name="query">An optional search query string.</param>
        public void AddPages(SearchScope scope, string query = null) {
            string scopeID = _onenote.GetCurrentSearchScopeID(scope);
            XDocument searchResult;
            if (string.IsNullOrEmpty(query)) {
                // search by tags only
                // collect all page tags on pages which have page tags. Tag search appears to
                // be broken using work around
                 searchResult = _onenote.FindPagesByMetadata(scopeID, MetaCollection.PageTagsMetaKey);
            } else {
                 searchResult = _onenote.FindPages(scopeID, query);
            }
            buildHierarchy(searchResult.Root, null);
        }

        void buildHierarchy(XElement hierarchyNode, HierarchyNode parent) {
            string localname = hierarchyNode.Name.LocalName;
            if ("Page".Equals(localname)) {
                _pages.Push(new PageNode(hierarchyNode, parent));
            } else {
                HierarchyElement t = HierarchyElement.heNone;
                switch (localname) {
                    case "Section":
                        t = HierarchyElement.heSections;
                        break;
                    case "SectionGroup":
                        t = HierarchyElement.heSectionGroups;
                        break;
                    case "Notebook":
                        t = HierarchyElement.heNotebooks;
                        break;
                }
                if (t != HierarchyElement.heNone) {
                    var thisNode = new HierarchyNode(hierarchyNode, parent, t);
                    // recurse into the page tree
                    foreach (var childNode in hierarchyNode.Elements()) {
                        buildHierarchy(childNode, thisNode);
                    }
                } else {
                    foreach (var childNode in hierarchyNode.Elements()) {
                        buildHierarchy(childNode, parent);
                    }
                }
            }
        }
        /// <summary>
        /// Initialize a OneNote page hierarchy from a query result.
        /// </summary>
        /// <remarks>
        ///     Only pages with page tags are considered! To build a page
        ///     hierachy containing untagged pages use <see cref="AddPages(XDocument)"/>
        /// </remarks>
        /// <param name="onenote">OneNote application proxy.</param>
        /// <param name="hierarchy">OneNote page hierarchy</param>
        public PageHierarchy(OneNoteProxy onenote, XDocument hierarchy): this(onenote) {
            AddPages(hierarchy);
        }

        /// <summary>
        /// Initialize an empty OneNote page hierarchy.
        /// </summary>
        /// <remarks>
        ///     <see cref="AddPages(SearchScope,string)"/> or
        ///     <see cref="AddPages(XDocument)"/>
        ///     can be used to populate the hierarchy.
        /// </remarks>
        /// <param name="onenote">OneNote application proxy.</param>
        public PageHierarchy(OneNoteProxy onenote) {
            _onenote = onenote;
        }

        /// <summary>
        /// Initialize a new page hierachy instance with a search result.
        /// </summary>
        /// <param name="onenote">The OneNote application proxy.</param>
        /// <param name="scope">The search scope</param>
        /// <param name="query">Optional full text query.</param>
        public PageHierarchy(OneNoteProxy onenote, SearchScope scope, string query = null) : this(onenote) {
            AddPages(scope,query);
        }
    }
}
