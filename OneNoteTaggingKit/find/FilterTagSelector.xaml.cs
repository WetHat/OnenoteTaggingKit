using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Interaction logic for TagSelector.xaml
    /// </summary>
    [ComVisible(false)]
    public partial class FilterTagSelector : UserControl
    {
        /// <summary>
        /// Create a new instance of the button control
        /// </summary>
        public FilterTagSelector()
        {
            InitializeComponent();
        }

        private void OnDatacontextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            FilterTagSelectorModel oldMdl = e.OldValue as FilterTagSelectorModel;
            if (oldMdl != null)
            {
                oldMdl.PropertyChanged -= mdl_PropertyChanged;
            }
            FilterTagSelectorModel mdl = e.NewValue as FilterTagSelectorModel;
            if (mdl != null)
            {
                mdl.PropertyChanged += mdl_PropertyChanged;
            }
            buildHighlightedTagname();
        }

        void buildHighlightedTagname()
        {
            FilterTagSelectorModel mdl = DataContext as FilterTagSelectorModel;
            if (mdl != null)
            {
                tagName.Inlines.Clear();
                foreach (var f in mdl.HitHighlightedTagName)
                {
                    Run r = new Run(f.Text);
                    if (f.IsMatch)
                    {
                        r.Background = Brushes.Yellow;
                    }
                    tagName.Inlines.Add(r);
                }
            }
        }
        void mdl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            FilterTagSelectorModel mdl = sender as FilterTagSelectorModel;
            if (e == FilterTagSelectorModel.HIT_HIGHLIGHTED_TAGNAME)
            {
                buildHighlightedTagname();
            }
        }
    }
}
