// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
// Author: WetHat | (C) Copyright 2013 - 2016 WetHat Lab, all rights reserved
using System.Collections.Generic;
using System.Windows.Media;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Design time view model for the <see cref="HitHighlightedPageLink" /> control.
    /// </summary>
    public class HitHighlightedPageLinkDesignerModel : IHitHighlightedPageLinkModel
    {
        private readonly string _pageTitle = "Test Page Title";

        /// <summary>
        /// create a new instance of the design time view model.
        /// </summary>
        /// <remarks>this constructor is called by the UI design application</remarks>
        public HitHighlightedPageLinkDesignerModel() {
        }

        #region IHitHighlightedPageLinkModel

        /// <summary>
        /// Get the title of an item in a hit highlighted page search result list.
        /// </summary>
        public IList<TextFragment> HighlightedTitle {
            get {
                TextSplitter splitter = new TextSplitter("page");
                return splitter.SplitText(_pageTitle);
            }
        }

        /// <summary>
        /// Get the item 'bullet' symbol in a search result list.
        /// </summary>
        public string MarkerSymbol { get { return "❱"; } }

        /// <summary>
        /// Get the color of the 'bullet' in a search result list.
        /// </summary>
        public Brush MarkerColor { get { return Brushes.DarkOrange; } }

        #endregion IHitHighlightedPageLinkModel
    }
}