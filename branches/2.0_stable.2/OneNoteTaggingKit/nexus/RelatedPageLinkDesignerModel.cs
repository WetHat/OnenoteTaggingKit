using System;
using System.Collections.Generic;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
namespace WetHatLab.OneNote.TaggingKit.nexus
{
    public class RelatedPageLinkDesignerModel : IRelatedPageLinkModel
    {


        public string PageTitle { get { return "Sample page title";  } }

        public IEnumerable<Tuple<string, bool>> HighlightedTags
        {
            get
            {
                return new List<Tuple<string, bool>>()
                {
                    new Tuple<string, bool>("Tag 1",false),
                    new Tuple<string, bool>("Tag 2",true),
                    new Tuple<string, bool>("Tag 3",false)
                };
            }
        }
    }
}