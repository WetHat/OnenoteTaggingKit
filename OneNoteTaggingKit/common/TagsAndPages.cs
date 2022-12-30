// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    ///     Range of notes to select tags from or apply tags to.
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
    ///     Observable collections of OneNote pages and the tags on them.
    /// </summary>
    /// <remarks>
    ///     The tag and page collections are unordered and can be populated
    ///     by a full-text query <see cref="FindPages" /> or be the pages found
    ///     in a sub-tree of the OneNote page hierarchy
    ///     <see cref="LoadPageTags" />.
    /// </remarks>
    public class TagsAndPages : TagsAndPagesBase {
        /// <summary>
        /// Get the scope from which tags and pages where collected
        /// </summary>
        public SearchScope Scope { get; private set; }

        /// <summary>
        /// Get the search query which was used to search for pages.
        /// </summary>
        public string Query {get; private set;}

        /// <summary>
        /// Create a new instance of the tag collection
        /// </summary>
        /// <param name="onenote">OneNote application object</param>
        internal TagsAndPages(OneNoteProxy onenote) :base(onenote) {
            Scope = SearchScope.AllNotebooks;
            Query = null;
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
        ///     Find pages by full text search.
        /// </summary>
        /// <param name="query">Optional query string.</param>
        /// <param name="scope">
        ///     The OneNote id of the scope to search for pages. This is the
        ///     element ID of a notebook, section group, or section. If given as
        ///     null or empty string,scope is the entire set of notebooks open
        ///     in OneNote.
        /// </param>
        internal void FindPages(SearchScope scope, string query = null ) {
            Scope = scope;
            Query = query;

            BuildTagSet(new PageHierarchy(OneNote,scope,query),selectedPagesOnly:false);
            if (!string.IsNullOrEmpty(query)) {
                // attempt to automatically update the tag suggestions, if we have collected all used tags
                HashSet<string> knownTags = new HashSet<String>(from string s in Properties.Settings.Default.KnownTagsCollection select s);
                int countBefore = knownTags.Count;

                // update the list of known tags by adding tags from search result
                knownTags.UnionWith(from TagPageSet t in Tags.Values select t.TagName);

                if (countBefore != knownTags.Count) { // updated tag suggestions
                    string[] sortedTags = knownTags.ToArray();
                    Array.Sort(sortedTags);
                    Properties.Settings.Default.KnownTagsCollection.Clear();
                    Properties.Settings.Default.KnownTagsCollection.AddRange(sortedTags);
                }
            }
        }

        /// <summary>
        ///     Load tags the pages in a subtree of the OneNote page hierarchy.
        /// </summary>
        /// <param name="context">
        ///     The context from where to get pages.
        /// </param>
        internal void LoadPageTags(TagContext context) {
            // collect all tags and pages from a context
            switch (context) {
                default:
                case TagContext.CurrentNote:
                    BuildTagSet(OneNote.GetHierarchy(OneNote.CurrentPageID, HierarchyScope.hsSelf),
                                         selectedPagesOnly: false, omitUntaggedPages: true) ;
                    break;

                case TagContext.CurrentSection:
                    BuildTagSet(OneNote.GetHierarchy(OneNote.CurrentSectionID, HierarchyScope.hsPages),
                                          selectedPagesOnly: false, omitUntaggedPages: true);
                    break;

                case TagContext.SelectedNotes:
                    if (SelectedPages != null) {
                        var ph = new PageHierarchy(OneNote);
                        foreach (var id in SelectedPages) {
                            ph.AddPages(OneNote.GetHierarchy(id, HierarchyScope.hsSelf));
                        }
                        BuildTagSet(ph, selectedPagesOnly: true, omitUntaggedPages: true);
                    } else {
                        BuildTagSet(OneNote.GetHierarchy(OneNote.CurrentSectionID, HierarchyScope.hsPages),
                                              selectedPagesOnly: true, omitUntaggedPages: true);
                    }
                    break;
            }
        }
        /// <summary>
        ///     Extract tags from OneNote pages found in a page hierarchy.
        /// </summary>
        /// <remarks>
        ///     This function attempts to keep the change notifications to
        ///     a minimum.
        /// </remarks>
        /// <param name="hierarchy">
        /// A    hierarchy of OneNote pages.
        /// </param>
        /// <param name="selectedPagesOnly">
        ///     true to process only pages selected by user
        ///     </param>
        /// <param name="omitUntaggedPages">
        ///     drop untagged pages
        /// </param>
        void BuildTagSet(PageHierarchy hierarchy, bool selectedPagesOnly, bool omitUntaggedPages = false) {
            Pages.Reset(UpdateTagSet(hierarchy.Pages, selectedPagesOnly, omitUntaggedPages));
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
                BuildTagSet(new PageHierarchy(OneNote, pageDescriptors), selectedPagesOnly, omitUntaggedPages);
            } catch (Exception ex) {
                TraceLogger.Log(TraceCategory.Error(), "Parsing Hierarchy data failed: {0}", ex);
                TraceLogger.Flush();
            }
        }
    }
}