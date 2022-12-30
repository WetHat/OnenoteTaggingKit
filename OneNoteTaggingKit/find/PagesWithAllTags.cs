using System.Collections.Generic;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.find
{
    public class PagesWithAllTags : FilteredPagesBase
    {
        /// <summary>
        /// Initialize a page filter which requires pages to have all
        /// selected tags.
        /// </summary>
        /// <param name="source">Source collections of tag and pages</param>
        public PagesWithAllTags(TagsAndPages source) :base(source) {
        }

        protected override void RecomputeFilteredPages(NotifyDictionaryChangedEventArgs<string, TagPageSet> e) {
            switch (e.Action) {
                case NotifyDictionaryChangedAction.Add:
                    foreach (TagPageSet tps in e.Items) {
                        if (SelectedTags.Count > 1) {
                            Pages.IntersectWith(tps.Pages);
                        } else { // start populating the pages with the first
                                 // refinement tag
                            Pages.UnionWith(tps.Pages);
                        }
                    }
                    break;
                case NotifyDictionaryChangedAction.Reset:
                case NotifyDictionaryChangedAction.Remove:
                    if (SelectedTags.Count > 0) {
                        // update the
                        var filtered = new HashSet<PageNode>();
                        foreach (var tps in SelectedTags.Values) {
                            if (filtered.Count == 0) {
                                filtered.UnionWith(tps.Pages);
                            } else {
                                filtered.IntersectWith(tps.Pages);
                            }
                        }
                        Pages.IntersectWith(filtered);
                        Pages.UnionWith(filtered);
                    } else {
                        // no refinement tag set -> do not display pages
                        Pages.Clear();
                        // reset the filtered page counts of all tags
                        foreach (var tps in RefinementTags.Values) {
                            tps.ClearFilter();
                        }
                        return;
                    }
                    break;
            }
            // update the filtered page counts of all tags
            foreach (var tps in RefinementTags.Values) {
                tps.IntersectWith(Pages.Values);
            }
        }
    }
}
