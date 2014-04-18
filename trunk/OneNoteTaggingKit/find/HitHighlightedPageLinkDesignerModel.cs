using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Design time view model for the <see cref="HitHighlightedPageLink"/> control.
    /// </summary>
    public class HitHighlightedPageLinkDesignerModel : IHitHighlightedPageLinkModel
    {
        private MatchCollection _matches;

        private string _pageTitle = "Test Page Title";

        /// <summary>
        /// create a new instance of the design time view model.
        /// </summary>
        /// <remarks>this constructor is called by the UI design application</remarks>
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
