using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// View model to support UI designers
    /// </summary>
    public class HitHighlightedTagButtonDesignerModel : IHitHighlightedTagButtonModel
    {
        /// <summary>
        /// Get the UI control's visibility
        /// </summary>
        public System.Windows.Visibility Visibility
        {
            get { return System.Windows.Visibility.Visible; }
        }

        /// <summary>
        /// Get the name of the tag to be displayed in the UI.
        /// </summary>
        public string TagName
        {
            get { return "Sample Tag"; }
        }

        /// <summary>
        /// The event to notify about property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Get the UI control's margin
        /// </summary>
        public Thickness Margin
        {
            get { return new Thickness(0,5,5,0); }
        }


        public IEnumerable<TextFragment> HitHighlightedTagName
        {
            get
            {
                TextSplitter splitter = new TextSplitter("Tag");
                return splitter.SplitText(TagName);
            }
        }
    }
}
