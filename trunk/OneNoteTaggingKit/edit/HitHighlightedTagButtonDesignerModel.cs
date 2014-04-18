using System.ComponentModel;
using System.Windows;

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
        /// get the hit higlighting descriptor
        /// </summary>
        public Hit Hit
        {
            get { return default(Hit); }
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
    }
}
