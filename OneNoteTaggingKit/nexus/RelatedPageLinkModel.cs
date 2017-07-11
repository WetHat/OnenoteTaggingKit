using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.nexus
{
    public interface IRelatedPageLinkModel
    {
       string LinkTitle { get; }
       string Tag { get; }
    }

    public class RelatedPageLinkModel : IRelatedPageLinkModel
    {
        TaggedPage _page;
        TagPageSet _tag;
        internal RelatedPageLinkModel(TaggedPage page, TagPageSet tag)
        {
            _page = page;
            _tag = tag;
        }

        #region IRelatedPageLinkModel
        public string LinkTitle
        {
            get { return _page.Title; }
        }
        public string Tag
        {
            get { return _tag.TagName; }
        }
        #endregion IRelatedPageLinkModel
    }
}
