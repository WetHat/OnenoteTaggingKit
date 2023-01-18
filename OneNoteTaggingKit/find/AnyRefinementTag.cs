using System.Collections.Generic;
using System.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.find
{
    public class AnyRefinementTag : RefinementTagBase
    {
        WithAnyTagsFilter _filterContext;

        /// <summary>
        ///     Initialize a refinement tag with a tag found on OneNote pages.
        /// </summary>
        /// <param name="tag">
        ///     The tag a OneNote page mist have to pass the filter.
        /// </param>
        /// <param name="filterContext">
        ///     The filter which generated this tag.
        /// </param>
        public AnyRefinementTag(TagPageSet tag, WithAnyTagsFilter filterContext) : base(tag) {
            _filterContext = filterContext;
        }

        /// <summary>
        ///     AGenerator to select pages which have this tag.
        /// </summary>
        /// <remarks>
        ///     This generator expression cannot filter pages in-situ.
        /// </remarks>
        /// <param name="pages">
        ///     A collection of OneNote pages.
        /// </param>
        /// <returns>All pages which have this tag</returns>
        public override IEnumerable<PageNode> FilterPages(IEnumerable<PageNode> pages) {
            return from PageNode pg in pages
                   where pg.Tags.Contains(Tag)
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
            if (_filterContext.SelectedTags.Count == 0) {
                // all pages which have that tag
                foreach (PageNode page in pages) {
                    if (page.Tags.Contains(Tag)) {
                        matchcount++;
                    } else {
                        delta--;
                    }
                }
            } else {
                var givenPages = new HashSet<PageNode>(pages);
                matchcount = givenPages.Count;
                foreach(PageNode page in Pages) {
                    if (_filterContext.Source.Pages.ContainsKey(page.Key)
                        && !givenPages.Contains(page)) {
                        matchcount++;
                        delta++;
                    }
                }
            }
            FilteredPageCountDelta = delta;
            FilteredPageCount = matchcount;
        }
    }
}
