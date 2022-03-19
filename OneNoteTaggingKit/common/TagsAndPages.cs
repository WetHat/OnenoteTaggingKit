// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// Context from which a tags and pages collection has been build from.
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
    /// calling <see cref="FindPages" />or <see cref="LoadPageTags" />.
    /// </remarks>
    public class TagsAndPages
    {
        /// <summary>
        /// OneNote application object proxy.
        /// </summary>
        protected OneNoteProxy _onenote;

        private ObservableDictionary<string, PageNode> _pages = new ObservableDictionary<string, PageNode>();
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
        internal ObservableDictionary<string, PageNode> Pages {
            get { return _pages; }
        }

        /// <summary>
        /// get a dictionary of tags.
        /// </summary>
        internal ObservableDictionary<string, TagPageSet> Tags {
            get { return _tags; }
        }

        /// <summary>
        /// Find pages by full text search
        /// </summary>
        /// <param name="query">  query string</param>
        /// <param name="scope">
        /// OneNote id of the scope to search for pages. This is the element ID of a
        /// notebook, section group, or section. If given as null or empty string;
        /// scope is the entire set of notebooks open in OneNote.
        /// </param>
        internal void FindPages(SearchScope scope, string query = null ) {
            BuildTagSet(new PageHierarchy(_onenote,scope,query),selectedPagesOnly:false);
            if (!string.IsNullOrEmpty(query)) {
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
        }

        /// <summary>
        /// Load tags from pages in subtree of the OneNote page hierarchy.
        /// </summary>
        /// <param name="context">The context from where to get pages.</param>
        internal void LoadPageTags(TagContext context) {
            // collect all tags and pages from a context
            switch (context) {
                default:
                case TagContext.CurrentNote:
                    BuildTagSet(_onenote.GetHierarchy(_onenote.CurrentPageID, HierarchyScope.hsSelf),
                                         selectedPagesOnly: false, omitUntaggedPages: true) ;
                    break;

                case TagContext.CurrentSection:
                    BuildTagSet(_onenote.GetHierarchy(_onenote.CurrentSectionID, HierarchyScope.hsPages),
                                          selectedPagesOnly: false, omitUntaggedPages: true);
                    break;

                case TagContext.SelectedNotes:
                    if (SelectedPages != null) {
                        var ph = new PageHierarchy(_onenote);
                        foreach (var id in SelectedPages) {
                            ph.AddPages(_onenote.GetHierarchy(id, HierarchyScope.hsSelf));
                        }
                        BuildTagSet(ph, selectedPagesOnly: true, omitUntaggedPages: true);
                    } else {
                        BuildTagSet(_onenote.GetHierarchy(_onenote.CurrentSectionID, HierarchyScope.hsPages),
                                              selectedPagesOnly: true, omitUntaggedPages: true);
                    }
                    break;
            }
        }
        /// <summary>
        /// Extract tags from OneNote page descriptors.
        /// </summary>
        /// <param name="hierarchy">
        /// A hierarchy of OneNote pages.
        /// </param>
        /// <param name="selectedPagesOnly">true to process only pages selected by user</param>
        /// <param name="omitUntaggedPages">drop untagged pages</param>
        void BuildTagSet(PageHierarchy hierarchy, bool selectedPagesOnly, bool omitUntaggedPages = false) {
            Dictionary<string, TagPageSet> tags  = new Dictionary<string, TagPageSet>();
            Dictionary<string, PageNode> taggedpages = new Dictionary<string, PageNode>();

            foreach (var tp in hierarchy.Pages) {
                if (selectedPagesOnly && !tp.IsSelected) {
                    continue;
                }
                // assign Tags
                int tagcount = 0;
                foreach (var tag in tp.Tags) {
                    tagcount++;
                    TagPageSet t;

                    if (!tags.TryGetValue(tag.Key, out t)) {
                        if (_tags.TryGetValue(tag.Key, out t)) {
                            // recycle that existing tag
                            t.ClearFilter();
                            t.Pages.Clear();

                        } else {
                            t = new TagPageSet(tag);
                        }
                        tags.Add(t.TagName, t);
                    } else if (tag.TagType < t.Tag.TagType) {
                        t.Tag = tag;
                    }

                    t.AddPage(tp);
                }
                if (tagcount > 0 || !omitUntaggedPages) {
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
                BuildTagSet(new PageHierarchy(_onenote, pageDescriptors), selectedPagesOnly, omitUntaggedPages);
            } catch (Exception ex) {
                TraceLogger.Log(TraceCategory.Error(), "Parsing Hierarchy data failed: {0}", ex);
                TraceLogger.Flush();
            }
        }
    }
}