using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.HierarchyBuilder
{
    /// <summary>
    /// Proxy representation of a OneNote page hierarchy.
    /// </summary>
    public class PageHierarchy {

        OneNoteProxy _onenote;

        Stack<TaggedPage> _pages = new Stack<TaggedPage>();
        /// <summary>
        /// Get the pages of the hierarchy.
        /// </summary>
        public IEnumerable<TaggedPage> Pages { get => _pages; }

        /// <summary>
        /// Add pages of a page hierarchy to this hierarchy
        /// </summary>
        /// <param name="hierarchy">A OneNote page hierarchy</param>
        public void AddPages(XDocument hierarchy) {
            foreach (XElement childNode in hierarchy.Root.Elements()) {
                buildHierarchy(childNode, null);
            }
        }

        void buildHierarchy(XElement hierarchyNode, HierarchyNode parent) {
            string localname = hierarchyNode.Name.LocalName;
            if ("Page".Equals(localname)) {
                _pages.Push(new TaggedPage(hierarchyNode, parent));
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
                    foreach(var childNode in hierarchyNode.Elements()) {
                        buildHierarchy(childNode, thisNode);
                    }
                }
            }
        }
        /// <summary>
        /// Initialize a OneNote page hierarchy from a query result.
        /// </summary>
        /// <remarks>
        /// Builds a page hierarchy from a search result
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
        /// <param name="onenote">OneNote application proxy.</param>
        /// <param name="hierarchy">OneNote page hierarchy</param>
        public PageHierarchy(OneNoteProxy onenote, XDocument hierarchy): this(onenote) {
            AddPages(hierarchy);
        }

        /// <summary>
        /// initialize an empty OneNote page hierarchy.
        /// </summary>
        /// <remarks><see cref="AddPages"/> can be used to populate the hierarchy.</remarks>
        /// <param name="onenote">OneNote application proxy.</param>
        public PageHierarchy(OneNoteProxy onenote) {
            _onenote = onenote;
        }
    }
}
