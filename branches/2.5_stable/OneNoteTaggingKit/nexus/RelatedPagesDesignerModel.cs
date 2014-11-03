using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WetHatLab.OneNote.TaggingKit.nexus
{
    public class RelatedPagesDesignerModel : IRelatedPagesModel
    {
        public string CurrentPageTitle
        {
            get { return "Current Page Title"; }
        }

        public IEnumerable<RelatedPageLinkModel> RelatedPages
        {
            get { return new RelatedPageLinkModel[0] ;}
        }
    }
}
