using System.Collections.Generic;
using System.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.find
{

    /// <summary>
    ///     A refinement tag required to not be on a OneNote page in order to pass the
    ///     filter.
    /// </summary>
    public class BlockedRefinementTag : RefinementTagBase
    {
        /// <summary>
        ///     Initialize a refinement tag with a tag found on OneNote pages.
        /// </summary>
        /// <param name="tag">
        ///     THe tag a OneNote page must not have in order to pass the filter.
        /// </param>
        public BlockedRefinementTag(TagPageSet tag) : base(tag) {
        }

        /// <summary>
        ///     Generator to select pages which do not have this tag.
        /// </summary>
        /// <remarks>
        ///     This generator expression cannot filter pages in-situ.
        /// </remarks>
        /// <param name="pages">
        ///    Collection of OneNote pages to filter.
        /// </param>
        /// <returns>All pages which do not have this tag.</returns>
        public override IEnumerable<PageNode> FilterPages(IEnumerable<PageNode> pages) {
            return from PageNode pg in pages
                   where !pg.Tags.Contains(Tag)
                   select pg;
        }

        /// <summary>
        ///     Compute the effect this tag has on a collection of pages.
        /// </summary>
        /// <remarks>
        ///     This method sets the properties
        ///     <see cref="RefinementTagBase.FilteredPageCount"/> and
        ///     <see cref="RefinementTagBase.FilteredPageCountDelta"/>.
        /// </remarks>
        /// <param name="pages"></param>
        public override void FilterEffect(IEnumerable<PageNode> pages) {
            int matchcount = 0;
            int delta = 0;
            foreach (PageNode page in pages) {
                if (page.Tags.Contains(Tag)) {
                    delta--;
                } else {
                    matchcount++;
                }
            }
            FilteredPageCountDelta = delta;
            FilteredPageCount = matchcount;
        }
    }
}
