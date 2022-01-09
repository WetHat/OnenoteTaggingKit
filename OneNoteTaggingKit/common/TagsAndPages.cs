// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

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
    /// calling <see cref="FindTaggedPages(string)" />,
    /// <see cref="FindTaggedPages(string,string)" /> or <see cref="LoadPageTags" />.
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
        internal TagsAndPages(OneNoteProxy onenote) {
            _onenote = onenote;
        }

        /// <summary>
        /// Collection of IDs of pre-selected OneNote pages.
        /// </summary>
        /// <remarks>
        /// If not set, the pages selected in the current section of the
        /// OneNote app will be used.
        /// </remarks>
        /// <seealso cref="LoadPageTags"/>
        internal IEnumerable<string> SelectedPages { get; set; } = null;

        /// <summary>
        /// Get a dictionary of pages.
        /// </summary>
        internal ObservableDictionary<string, TaggedPage> Pages {
            get { return _pages; }
        }

        /// <summary>
        /// get a dictionary of tags.
        /// </summary>
        internal ObservableDictionary<string, TagPageSet> Tags {
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
        internal void FindTaggedPages(string scopeID) {
            // collect all page tags on pages which have page tags. Tag search appears to
            // be broken using work around
            if (Properties.Settings.Default.UseWindowsSearch) {
                BuildTagSet(_onenote.FindPagesByMetadata(scopeID, OneNotePageProxy.META_NAME), selectedPagesOnly: false);
            } else {
                BuildTagSet(_onenote.GetHierarchy(scopeID, HierarchyScope.hsPages), selectedPagesOnly: false, omitUntaggedPages: true);
            }

            // attempt to automatically update the tag suggestions, if we have collected all used tags
            HashSet<string> knownTags = new HashSet<String>(from string s in Properties.Settings.Default.KnownTagsCollection select s);
            int countBefore = knownTags.Count;

            // update the list of known tags by adding tags from search result
            foreach (KeyValuePair<string, TagPageSet> t in _tags) {
                knownTags.Add(t.Key);
            }

            if (countBefore != knownTags.Count) { // updated tag suggestions
                string[] sortedTags = knownTags.ToArray();
                Array.Sort(sortedTags);
                Properties.Settings.Default.KnownTagsCollection.Clear();
                Properties.Settings.Default.KnownTagsCollection.AddRange(sortedTags);
            }
        }

        /// <summary>
        /// Find pages by full text search
        /// </summary>
        /// <param name="query">  query string</param>
        /// <param name="scopeID">
        /// OneNote id of the scope to search for pages. This is the element ID of a
        /// notebook, section group, or section. If given as null or empty string;
        /// scope is the entire set of notebooks open in OneNote.
        /// </param>
        internal void FindTaggedPages(string query, string scopeID) {
            BuildTagSet(_onenote.FindPages(scopeID, query), selectedPagesOnly: false);
        }

        /// <summary>
        /// Load tags from pages in subtree of the OneNote page directory structure.
        /// </summary>
        /// <param name="context">the context from where to get pages</param>
        internal void LoadPageTags(TagContext context) {
            // collect all tags and pages from a context

            switch (context) {
                default:
                case TagContext.CurrentNote:
                    BuildTagSet(_onenote.GetHierarchy(_onenote.CurrentPageID, HierarchyScope.hsSelf),
                                         selectedPagesOnly: false);
                    break;

                case TagContext.CurrentSection:
                    BuildTagSet(_onenote.GetHierarchy(_onenote.CurrentSectionID, HierarchyScope.hsPages),
                                          selectedPagesOnly: false);
                    break;

                case TagContext.SelectedNotes:
                    if (SelectedPages != null) {
                        // todo merge docs for selected pages
                        BuildTagSet(from id in SelectedPages
                                    select _onenote.GetHierarchy(id, HierarchyScope.hsSelf).Root,
                                    selectedPagesOnly: false);
                    } else {
                        BuildTagSet(_onenote.GetHierarchy(_onenote.CurrentSectionID, HierarchyScope.hsPages),
                                              selectedPagesOnly: true);
                    }
                    break;
            }
        }
        /// <summary>
        /// Extract tags from OneNote page descriptors.
        /// </summary>
        /// <param name="pages">
        /// XML elemets describing pages in the OneNote hierarchy or search result.
        /// </param>
        /// <param name="selectedPagesOnly">true to process only pages selected by user</param>
        /// <param name="omitUntaggedPages">drop untagged pages</param>
        void BuildTagSet(IEnumerable<XElement>pages, bool selectedPagesOnly, bool omitUntaggedPages = false) {
            Dictionary<string, TagPageSet> tags  = new Dictionary<string, TagPageSet>();
            Dictionary<string, TaggedPage> taggedpages = new Dictionary<string, TaggedPage>();

            foreach (XElement page in pages) {
                TaggedPage tp = new TaggedPage(page);
                if (selectedPagesOnly && !tp.IsSelected) {
                    continue;
                }
                // assign Tags
                int tagcount = 0;
                foreach (string tagname in tp.TagNames) {
                    tagcount++;
                    Tuple<string, string> parsedTag = TagPageSet.ParseTagName(tagname);
                    TagPageSet t;

                    if (!tags.TryGetValue(parsedTag.Item1, out t)) {
                        t = new TagPageSet(parsedTag);
                        tags.Add(t.TagName, t);
                    } else {
                        t.TagType = parsedTag.Item2;
                    }

                    t.AddPage(tp);
                    tp.Tags.Add(t);
                }
                if (!omitUntaggedPages || tagcount > 0) {
                    taggedpages.Add(tp.Key, tp);
                }
            }
            // bulk update of pages
            _pages.IntersectWith(taggedpages.Values); // remove obsolete tags
            _pages.UnionWith(taggedpages.Values); // add new tags

            // bulk update of tags
            _tags.IntersectWith(tags.Values); // remove obsolete tags
            _tags.UnionWith(tags.Values); // add new tags
            TraceLogger.Log(TraceCategory.Info(), "Extracted {0} tags from {1} pages.", _tags.Count, _pages.Count);
        }
        /// <summary>
        /// Extract tags from page descriptors.
        /// </summary>
        /// <param name="pageDescriptors">
        /// XML document describing pages in the OneNote hierarchy or search result.
        /// </param>
        /// <param name="selectedPagesOnly">true to process only pages selected by user</param>
        /// <param name="omitUntaggedPages">drop untagged pages</param>
        internal void BuildTagSet(XDocument pageDescriptors, bool selectedPagesOnly, bool omitUntaggedPages = false) {
            // parse the search results
            try {
                XNamespace one = pageDescriptors.Root.GetNamespaceOfPrefix("one");
                BuildTagSet(pageDescriptors.Descendants(one.GetName("Page")), selectedPagesOnly, omitUntaggedPages);
            } catch (Exception ex) {
                TraceLogger.Log(TraceCategory.Error(), "Parsing Hierarchy data failed: {0}", ex);
                TraceLogger.Flush();
            }
        }
    }
}