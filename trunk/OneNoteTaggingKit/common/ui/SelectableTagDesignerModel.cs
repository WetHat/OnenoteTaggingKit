using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    public class SelectableTagDesignerModel : ISelectableTagModel
    {
        public bool IsChecked
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        public int PageCount
        {
            get { return 1; }
        }

        public Visibility Visibility
        {
            get { return System.Windows.Visibility.Visible; }
        }


        public IEnumerable<TextFragment> HitHighlightedTagName
        {
            get { return new TextFragment[] { new TextFragment("Test ", null), new TextFragment("Tag", Brushes.Yellow) }; }
        }
    }
}
