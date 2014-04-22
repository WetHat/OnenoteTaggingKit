using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// Interaction logic for SelectableTag.xaml
    /// </summary>
    public partial class SelectableTag : UserControl
    {
        public SelectableTag()
        {
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ISelectableTagModel mdl = e.NewValue as ISelectableTagModel;

            if (mdl != null)
            {
                mdl.PropertyChanged += OnModelChanged;
                buildHighlightedTagnameUI(mdl.HitHighlightedTagName);
            }
        }

        private void OnModelChanged(object sender, PropertyChangedEventArgs e)
        {
            ISelectableTagModel mdl = sender as ISelectableTagModel;

            if ("HitHighlightedTagName".Equals(e.PropertyName))
            {
                buildHighlightedTagnameUI(mdl.HitHighlightedTagName);
            }
        }

        private void buildHighlightedTagnameUI(IEnumerable<TextFragment> tagname)
        {
            tagName.Inlines.Clear();
            foreach (TextFragment t in tagname)
            {
                Run r = new Run(t.Text);
                if (t.HighLightColor != null)
                {
                    r.Background = t.HighLightColor;
                }
                tagName.Inlines.Add(r);
            }
        }
    }
}
