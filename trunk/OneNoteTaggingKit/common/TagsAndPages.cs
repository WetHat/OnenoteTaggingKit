using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.common
{

    /// <summary>
    /// Context from which a tag collection has been build from
    /// </summary>
    internal enum TagContext
    {
        /// <summary>
        /// Tags from current note.
        /// </summary>
        CurrentNote,
        /// <summary>
        /// Tags from selected notes.
        /// </summary>
        SelectedNotes,
        /// <summary>
        /// Tags from current section.
        /// </summary>
        CurrentSection
    }

    /// <summary>
    /// Observable collections of tags and OneNote pages satisfying a search criterion.
    /// </summary>
    /// <remarks>
    /// Provides an unordered set of tags and pages. The page collection is
    /// built by calling <see cref="FindPages"/> or <see cref="GetPagesFromHierarchy"/>.
    /// </remarks>
    public class TagsAndPages
    {
        /// <summary>
        /// OneNote application object
        /// </summary>
        protected Application _onenote;
        /// <summary>
        /// version specific schema for OneNote pages
        /// </summary>
        protected XMLSchema _schema;

        private ObservableDictionary<string, TagPageSet> _tags = new ObservableDictionary<string, TagPageSet>();
        private ObservableDictionary<string, TaggedPage> _pages = new ObservableDictionary<string, TaggedPage>();

        /// <summary>
        /// Craete a new instance of the tag collection
        /// </summary>
        /// <param name="onenote">OneNote application object</param>
        /// <param name="schema">version dependent page schema</param>
        internal TagsAndPages(Application onenote, XMLSchema schema)
        {
            _onenote = onenote;
            _schema = schema;
        }
        /// <summary>
        /// Get the OneNote current windows object which provides ids of the current page, section, sectiongroup, and notebook
        /// </summary>
        internal Window CurrentWindow
        {
            get
            {
                return _onenote.Windows.CurrentWindow;
            }
        }
        /// <summary>
        /// FindPages OneNote pages in a scope.
        /// </summary>
        /// <param name="scopeID">OneNote id of the scope to search for pages. This is the element ID of a notebook, section group, or section.
        ///                       If given as null or empty string scope is the entire set of notebooks open in OneNote.
        /// </param>
        /// <param name="includeUnindexedPages">true to include pages in the search which have not been indexed yet</param>
        public void FindPages(string scopeID, bool includeUnindexedPages = false)
        {
            string strXml;
            // collect all tags used somewhere on a page
            _onenote.FindMeta(scopeID, OneNotePageProxy.META_NAME, out strXml, includeUnindexedPages, _schema);

            parseOneNoteHierarchy(strXml,false);

            // attempt to automatically update the tag suggestion list, if we have collected all used tags
            HashSet<string> knownTags = new HashSet<String>(OneNotePageProxy.ParseTags(Properties.Settings.Default.KnownTags));
            int countBefore = knownTags.Count;

            // add tags from search result
            foreach (KeyValuePair<string, TagPageSet> t in _tags)
            {
                knownTags.Add(t.Key);
            }

            if (countBefore != knownTags.Count)
            { // updated tag suggestions
                string[] sortedTags = knownTags.ToArray();
                Array.Sort(sortedTags);
                Properties.Settings.Default.KnownTags = string.Join(",", sortedTags);
            }
        }

        /// <summary>
        /// load tags from a subtree of the OneNote page directory structure.
        /// </summary>
        /// <param name="context">the context from where to get pages</param>
        /// <param name="currentWindow">current OneNote window</param>
        internal void GetPagesFromHierarchy(Window currentWindow, TagContext context)
        {
            string strXml;

            // collect all tags and pages from a context

            switch(context)
            {
                default:
                case TagContext.CurrentNote:
                    _onenote.GetHierarchy(currentWindow.CurrentPageId, HierarchyScope.hsSelf, out strXml, _schema);
                    break;

                case TagContext.CurrentSection:
                case TagContext.SelectedNotes:
                    _onenote.GetHierarchy(currentWindow.CurrentSectionId, HierarchyScope.hsPages, out strXml, _schema);
                    break;
            }
            
           

            parseOneNoteHierarchy(strXml,context == TagContext.SelectedNotes);
        }

        /// <summary>
        /// build tags from an XML document returned from OneNote
        /// </summary>
        /// <param name="strXml">string representation of the XML document</param>
        internal void parseOneNoteHierarchy(string strXml, bool selectedPagesOnly)
        {
            // parse the search results
            _tags.Clear();
            _pages.Clear();
            try
            {
                XDocument result = XDocument.Parse(strXml);
                XNamespace one = result.Root.GetNamespaceOfPrefix("one");

                Dictionary<string, TagPageSet> tags = new Dictionary<string, TagPageSet>();
                foreach (XElement page in result.Descendants(one.GetName("Page")))
                {
                    TaggedPage tp = new TaggedPage(page);
                    if (selectedPagesOnly && !tp.IsSelected)
                    {
                        continue;
                    }
                    // assign Tags
                    XElement meta = page.Elements(one.GetName("Meta")).FirstOrDefault(m => OneNotePageProxy.META_NAME.Equals(m.Attribute("name").Value));
                    if (meta != null)
                    {
                        foreach (string tagname in OneNotePageProxy.ParseTags(meta.Attribute("content").Value))
                        {
                            TagPageSet t;

                            if (!tags.TryGetValue(tagname, out t))
                            {
                                t = new TagPageSet(tagname);
                                tags.Add(tagname, t);
                            }
                            t.AddPage(tp);
                            tp.Tags.Add(t);
                        }
                    }
                    _pages.Add(tp.Key, tp);
                }
                // bulk update for performance reasons
                _tags.UnionWith(tags.Values);
            }
            catch (Exception ex)
            {
                TraceLogger.Log(TraceCategory.Error(),"Parsing Hierarchy data failed: {0}",ex);
                TraceLogger.Flush();
                throw;
            }
        }

        /// <summary>
        /// get a dictionary of tags.
        /// </summary>
        internal ObservableDictionary<string, TagPageSet> Tags
        {
            get { return _tags; }
        }

        /// <summary>
        /// Get a dictionary of pages.
        /// </summary>
        internal ObservableDictionary<string, TaggedPage> Pages
        {
            get { return _pages; }
        }
    }
}