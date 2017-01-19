// Author: WetHat | (C) Copyright 2013 - 2016 WetHat Lab, all rights reserved
using System.Collections.Generic;
using System.Windows.Media;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Design time view model for the <see cref="HitHighlightedPageLink"/> control.
    /// </summary>
    public class HitHighlightedPageLinkDesignerModel : IHitHighlightedPageLinkModel
    {
        private string _pageTitle = "Test Page Title";

        /// <summary>
        /// create a new instance of the design time view model.
        /// </summary>
        /// <remarks>this constructor is called by the UI design application</remarks>
        public HitHighlightedPageLinkDesignerModel()
        {
        }

        #region IHitHighlightedPageLinkModel

        public IList<TextFragment> HighlightedTitle
        {
            get
            {
                TextSplitter splitter = new TextSplitter("page");
                return splitter.SplitText(_pageTitle);
            }
        }

        public string MarkerSymbol { get { return "❱"; } }

        public Brush MarkerColor { get { return Brushes.Gray; } }

        #endregion IHitHighlightedPageLinkModel
    }
}