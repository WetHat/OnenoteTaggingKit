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
    public partial class TagSelector : UserControl
    {
        /// <summary>
        /// Create a new instance of the button control
        /// </summary>
        public TagSelector()
        {
            InitializeComponent();
        }

        private void OnDatacontextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TagSelectorModel oldMdl = e.NewValue as TagSelectorModel;
            if (oldMdl != null)
            {
                oldMdl.PropertyChanged -= mdl_PropertyChanged;
            }
            TagSelectorModel mdl = e.NewValue as TagSelectorModel;
            if (mdl != null)
            {
                mdl.PropertyChanged += mdl_PropertyChanged;
            }
            buildHighlightedTagname();
        }

        void buildHighlightedTagname()
        {
            TagSelectorModel mdl = DataContext as TagSelectorModel;
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
            TagSelectorModel mdl = sender as TagSelectorModel;
            if (e == TagSelectorModel.HIT_HIGHLIGHTED_TAGNAME)
            {
                buildHighlightedTagname();
            }
        }
    }
}
