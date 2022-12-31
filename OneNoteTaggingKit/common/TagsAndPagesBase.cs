// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Collections.Generic;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    ///     Base class for collections of pages and the tags found in them.
    /// </summary>
    public class TagsAndPagesBase
    {
        /// <summary>
        /// OneNote application object proxy.
        /// </summary>
        public OneNoteProxy OneNote { get; private set; }

        /// <summary>
        /// Get the observable collection of pages.
        /// </summary>
        public ObservableDictionary<string, PageNode> Pages { get; } = new ObservableDictionary<string, PageNode>();

        /// <summary>
        ///     Get the observable collection of tags.
        /// </summary>
        /// <remarks>
        ///     The collection of tags is typically generated from page tags
        ///     passed into <see cref="UpdateTagSet"/>
        /// </remarks>
        public ObservableDictionary<string, TagPageSet> Tags { get; } = new ObservableDictionary<string, TagPageSet>();

        /// <summary>
        /// Create a new instance of the tag collection
        /// </summary>
        /// <param name="onenote">OneNote application object</param>
        protected TagsAndPagesBase(OneNoteProxy onenote) {
            OneNote = onenote;
        }

        /// <summary>
        ///     Extract tags from OneNote pages found in a page hierarchy.
        /// </summary>
        /// <remarks>
        ///     This function attempts to keep the change notifications to
        ///     a minimum.
        ///
        ///     **Note**: The <see cref="Tags"/> property is updated with
        ///     the tags extracted from the given pages. However, the
        ///     <see cref="Pages"/> property needs to be updated by the caller
        ///     with a subset of the returned page collection as determined by
        ///     implementation specific rules.
        /// </remarks>
        /// <param name="pages">
        ///     A set of OneNote page objects to extract the tags from.
        /// </param>
        /// <param name="selectedPagesOnly">
        ///     true to process only pages selected by user
        ///     </param>
        /// <param name="omitUntaggedPages">
        ///     drop untagged pages
        /// </param>
        /// <returns>The collection of tagged pages</returns>
        protected Dictionary<string, PageNode> UpdateTagSet(IEnumerable<PageNode> pages, bool selectedPagesOnly, bool omitUntaggedPages = false) {
            Dictionary<string, TagPageSet> tags = new Dictionary<string, TagPageSet>();
            Dictionary<string, PageNode> taggedpages = new Dictionary<string, PageNode>();
            foreach (var tp in pages) {
                if (selectedPagesOnly && !tp.IsSelected) {
                    continue;
                }
                if (tp.Tags.IsEmpty && omitUntaggedPages) {
                    continue;
                } else {
                    taggedpages.Add(tp.Key, tp); // record for bulk update
                }

                // assign Tags
                foreach (var tag in tp.Tags) {
                    TagPageSet t;

                    if (!tags.TryGetValue(tag.Key, out t)) {
                        if (Tags.TryGetValue(tag.Key, out t)) {
                            // recycle that existing tag
                            t.Pages.Clear();
                            t.Tag = PageTagSet.ChoosePageTag(t.Tag, tag);
                        } else {
                            t = new TagPageSet(tag);
                            // update the set of suggested tags
                            OneNote.KnownTags.Add(t.Tag.ManagedTag);
                        }
                        tags.Add(t.Tag.Key, t);
                    } else {
                        t.Tag = PageTagSet.ChoosePageTag(t.Tag, tag);
                    }

                    t.AddPage(tp);
                }
            }
            // bulk update of tags
            Tags.IntersectWith(tags.Values); // remove obsolete tags
            Tags.UnionWith(tags.Values); // add new tags
            TraceLogger.Log(TraceCategory.Info(), "Extracted {0} tags from {1} pages.", Tags.Count, taggedpages.Count);
            return taggedpages;
        }
    }
}