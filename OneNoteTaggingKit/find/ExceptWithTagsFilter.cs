using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    ///     A set of pages which do not have certain tags.
    /// </summary>
    public class ExceptWithTagsFilter : TagFilterBase
    {
        public ExceptWithTagsFilter(TagFilterBase source): base(source) {
        }

        /// <summary>
        /// Re-apply the tag filter and compute the set of pages
        /// </summary>
        /// <param name="e">Change details for the thagss in the filter.</param>
        protected override void RecomputeFilteredPages(NotifyDictionaryChangedEventArgs<string, TagPageSet> e) {

        }
    }
}
