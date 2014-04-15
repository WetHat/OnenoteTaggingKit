using System.ComponentModel;
using System.Windows;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    public class HitHighlightedTagButtonDesignerModel : IHitHighlightedTagButtonModel
    {
        public System.Windows.Visibility Visibility
        {
            get { return System.Windows.Visibility.Visible; }
        }

        public string TagName
        {
            get { return "Sample Tag"; }
        }


        public Hit Hit
        {
            get { return default(Hit); }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public Thickness Margin
        {
            get { return new Thickness(0,5,5,0); }
        }
    }
}
