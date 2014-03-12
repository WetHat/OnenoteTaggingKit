using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WetHatLab.OneNote.TaggingKit.find
{
    public class HitHighlightedPageLinkDesignerModel : IHitHighlightedPageLinkModel
    {
        private MatchCollection _matches;

        private string _pageTitle = "Test Page Title";

        public HitHighlightedPageLinkDesignerModel()
        {
            _matches = Regex.Matches(_pageTitle, "page",RegexOptions.IgnoreCase);
        }

        #region IHitHighlightedPageLinkModel
        public MatchCollection Matches
        {
            get
            {
                return _matches;
            }
        }
        public string PageTitle
        {
            get
            {
                return _pageTitle;
            }
        }
        public int Hits
        {
            get { return _matches != null ? _matches.Count : 0; }
        }
        #endregion


       
    }
}
