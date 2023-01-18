using System.Collections.Generic;
using System.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    ///     A set-subtraction based tag filter.
    /// </summary>
    /// <remarks>
    ///     Computes the set of pages which do not have certain tags.
    /// </remarks>
    public class ExceptWithTagsFilter : TagFilterBase
    {
        /// <summary>
        ///     Initialize a tag filter instance by adding it to the tail filter
        ///     of a filter chain.
        /// </summary>
        /// <param name="source">
        ///     The tail filter instance of a filter chain.
        /// </param>
        public ExceptWithTagsFilter(TagFilterBase source): base(source) {
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
        public override RefinementTagBase MakeRefinementTag(TagPageSet tag) => new BlockedRefinementTag(tag);

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
                    foreach (TagPageSet tps in e.Items) {
                        // narrow down the result.
                        Pages.ExceptWith(tps.Pages);
                    }
                    break;
                case NotifyDictionaryChangedAction.Remove:
                    if (SelectedTags.Count > 0) {
                        // we have more matching pages now
                        Pages.UnionWith(FilterPages(Source.Pages.Values));
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
    }
}
