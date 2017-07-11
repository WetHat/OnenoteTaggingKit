using System;
using System.Collections.Generic;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
namespace WetHatLab.OneNote.TaggingKit.nexus
{

    /// <summary>
    /// Design time support for the <see cref="RelatedPageLink"/> control.
    /// </summary>
    public class RelatedPageLinkDesignerModel : IRelatedPageLinkModel
    {
        public string LinkTitle { get { return "Sample page title"; } }


        public string Tag
        {
            get { return "Tag"; }
        }
    }
}