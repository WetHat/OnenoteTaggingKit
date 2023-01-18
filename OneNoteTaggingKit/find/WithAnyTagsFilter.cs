using System.Collections.Generic;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    ///     A set-union based tag filter.
    /// </summary>
    /// <remarks>
    ///     Computes the set of pages which have a any tag from a set of tags.
    /// </remarks>
    public class WithAnyTagsFilter : TagFilterBase
    {
        /// <summary>
        /// Initialize a page filter which requires pages to have all
        /// selected tags.
        /// </summary>
        /// <param name="source">Source collections of tag and pages</param>
        public WithAnyTagsFilter(TagFilterBase source) : base(source) {
            source.Next = this;
        }

        /// <summary>
        ///     Make a refinement tag which is required to be on a page to satisfy
        ///     the filter requirement
        /// </summary>
        /// <param name="tag">
        ///     The tag to base the refinement tag on.
        /// </param>
        /// <returns>
        ///     A new instance of a refinement tag which is required to be on a OneNote page.
        /// </returns>
        public override RefinementTagBase MakeRefinementTag(TagPageSet tag) => new AnyRefinementTag(tag,this);

        /// <summary>
        ///     Process changes to the collection of filter tags and update the
        ///     collection of filtered pages accordingly.
        /// </summary>
        /// <param name="e">
        ///     Change details for the collection of filter tags.
        /// </param>
        protected override void UpdateTagFilter(NotifyDictionaryChangedEventArgs<string, TagPageSet> e) {
            switch (e.Action) {
                case NotifyDictionaryChangedAction.Add:
                    if (Source.Pages.Count == Pages.Count) {
                        // no filter applied jet
                        var filteredPages = new HashSet<PageNode>();
                        foreach (var tag in e.Items) {
                            RefinementTagBase rt;
                            if (RefinementTags.TryGetValue(tag.Key, out rt)) {
                                filteredPages.UnionWith(rt.FilterPages(Source.Pages.Values));
                            }
                        }
                        // update the collection of pages
                        Pages.IntersectWith(filteredPages);
                    } else {
                        // pages already filteres - add additional pages
                        foreach (var tag in e.Items) {
                            RefinementTagBase rt;
                            if (RefinementTags.TryGetValue(tag.Key, out rt)) {
                                Pages.UnionWith(rt.FilterPages(Source.Pages.Values));
                            }
                        }
                    }
                    break;
                case NotifyDictionaryChangedAction.Remove:
                    if (SelectedTags.Count > 0) {
                        // we have less matching pages now
                        Pages.IntersectWith(FilterPages(Source.Pages.Values));
                    } else {
                        Pages.UnionWith(Source.Pages.Values);
                    }
                    break;
                case NotifyDictionaryChangedAction.Reset:
                    if (SelectedTags.Count > 0) {
                        // do not know what changed - reapply the filter
                        Pages.Reset(FilterPages(Source.Pages.Values));
                    } else {
                        Pages.UnionWith(Source.Pages.Values);
                    }
                    break;
            }
            if (AutoUodateEnabled) {
                RefreshRefinementTags();
            }
        }

        /// <summary>
        ///     Apply the `With Sny` tag filter to a given set of pages.
        /// </summary>
        /// <param name="pages">
        ///     Collection of pages.
        /// </param>
        /// <returns>
        ///     Collection of pages which staisfy the filter condition.
        /// </returns>
        protected override IEnumerable<PageNode> FilterPages(IEnumerable<PageNode> pages) {
           var filtered = new HashSet<PageNode>();
            if (SelectedTags.Count > 0) {
                foreach (TagPageSet t in SelectedTags.Values) {
                    RefinementTagBase rt;
                    if (RefinementTags.TryGetValue(t.Key, out rt)) {
                        filtered.UnionWith(rt.FilterPages(pages));
                    }
                }
                return filtered;
            }
            return pages;
        }
    }
}
