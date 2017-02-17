////////////////////////////////////////////////////////////
// Author: WetHat
// (C) Copyright 2015, 2016 WetHat Lab, all rights reserved
////////////////////////////////////////////////////////////
using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// Context from which a tags and pages collection has been build from
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
    /// Provides an unordered set of tags and pages. The page collection is populated by
    /// calling <see cref="FindTaggedPages(string,bool)" />,
    /// <see cref="FindTaggedPages(string,string,bool)" /> or <see cref="LoadPageTags" />.
    /// </remarks>
    public class TagsAndPages
    {
        /// <summary>
        /// OneNote application object proxy.
        /// </summary>
        protected OneNoteProxy _onenote;

        private ObservableDictionary<string, TaggedPage> _pages = new ObservableDictionary<string, TaggedPage>();
        private ObservableDictionary<string, TagPageSet> _tags = new ObservableDictionary<string, TagPageSet>();

        /// <summary>
        /// Create a new instance of the tag collection
        /// </summary>
        /// <param name="onenote">OneNote application object</param>
        internal TagsAndPages(OneNoteProxy onenote)
        {
            _onenote = onenote;
        }

        /// <summary>
        /// Get a dictionary of pages.
        /// </summary>
        internal ObservableDictionary<string, TaggedPage> Pages
        {
            get { return _pages; }
        }

        /// <summary>
        /// get a dictionary of tags.
        /// </summary>
        internal ObservableDictionary<string, TagPageSet> Tags
        {
            get { return _tags; }
        }

        /// <summary>
        /// Find tagged OneNote pages in a scope.
        /// </summary>
        /// <param name="scopeID">
        /// OneNote id of the scope to search for pages. This is the element ID of a
        /// notebook, section group, or section. If given as null or empty string scope is
        /// the entire set of notebooks open in OneNote.
        /// </param>
        /// <param name="includeUnindexedPages">
        /// true to include pages in the search which have not been indexed yet
        /// </param>
        internal void FindTaggedPages(string scopeID, bool includeUnindexedPages = false)
        {
            // collect all page tags on pages which have page tags.
            ExtractTags(_onenote.FindPagesByMetadata(scopeID, OneNotePageProxy.META_NAME, includeUnindexedPages: false), selectedPagesOnly: false);

            // attempt to automatically update the tag list, if we have collected all used tags
            HashSet<string> knownTags = new HashSet<String>(OneNotePageProxy.ParseTags(Properties.Settings.Default.KnownTags));
            int countBefore = knownTags.Count;

            // update the list of known tags by adding tags from search result
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
        /// Find pages by full text search
        /// </summary>
        /// <param name="query">                query string</param>
        /// <param name="scopeID">
        /// OneNote id of the scope to search for pages. This is the element ID of a
        /// notebook, section group, or section. If given as null or empty string scope is
        /// the entire set of notebooks open in OneNote.
        /// </param>
        /// <param name="includeUnindexedPages">
        /// true to include pages in the search which have not been indexed yet
        /// </param>
        internal void FindTaggedPages(string query, string scopeID, bool includeUnindexedPages = false)
        {
            ExtractTags(_onenote.FindPages(scopeID, query, includeUnindexedPages), selectedPagesOnly: false);
        }

        /// <summary>
        /// Load tags from pages in subtree of the OneNote page directory structure.
        /// </summary>
        /// <param name="context">the context from where to get pages</param>
        internal void LoadPageTags(TagContext context)
        {
            // collect all tags and pages from a context

            switch (context)
            {
                default:
                case TagContext.CurrentNote:
                    ExtractTags(_onenote.GetHierarchy(_onenote.CurrentPageID, HierarchyScope.hsSelf),
                                         selectedPagesOnly: false);
                    break;

                case TagContext.CurrentSection:
                    ExtractTags(_onenote.GetHierarchy(_onenote.CurrentSectionID, HierarchyScope.hsPages),
                                          selectedPagesOnly: false);
                    break;

                case TagContext.SelectedNotes:
                    ExtractTags(_onenote.GetHierarchy(_onenote.CurrentSectionID, HierarchyScope.hsPages),
                                          selectedPagesOnly: true);
                    break;
            }
        }

        /// <summary>
        /// Extract tags from page descriptors.
        /// </summary>
        /// <param name="pageDescriptors">XML document describing pages in the OneNote hierarchy</param>
        /// <param name="selectedPagesOnly">true to process only pages selected by user</param>
        internal void ExtractTags(XDocument pageDescriptors, bool selectedPagesOnly)
        {
            // parse the search results
            _tags.Clear();
            _pages.Clear();
            try
            {
                XNamespace one = pageDescriptors.Root.GetNamespaceOfPrefix("one");

                Dictionary<string, TagPageSet> tags = new Dictionary<string, TagPageSet>();
                foreach (XElement page in pageDescriptors.Descendants(one.GetName("Page")))
                {
                    TaggedPage tp = new TaggedPage(page);
                    if (selectedPagesOnly && !tp.IsSelected)
                    {
                        continue;
                    }
                    // assign Tags

                    foreach (string tagname in tp.TagNames)
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
                    _pages.Add(tp.Key, tp);
                }
                // bulk update for performance reasons
                _tags.UnionWith(tags.Values);
            }
            catch (Exception ex)
            {
                TraceLogger.Log(TraceCategory.Error(), "Parsing Hierarchy data failed: {0}", ex);
                TraceLogger.Flush();
            }
        }
    }
}