using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WetHatLab.OneNote.TaggingKit.nexus
{
    /// <summary>
    /// Interaction logic for RelatedPageLink.xaml
    /// </summary>
    public partial class RelatedPageLink : UserControl
    {
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RelatedPageLink));

        // Provide CLR accessors for the event 
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        public RelatedPageLink()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent));
            e.Handled = true;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RelatedPageLink link = sender as RelatedPageLink;
            if (link != null)
            {
                IRelatedPageLinkModel model = link.DataContext as IRelatedPageLinkModel;

                if (model != null)
                {
                    highlightedTags.Inlines.Clear();
                    foreach (var t in model.HighlightedTags)
                    {
                        if (highlightedTags.Inlines.Count > 0)
                        {
                            highlightedTags.Inlines.Add(new Run(","));
                        }
                        Run r = new Run(t.Item1);
                        if (t.Item2)
                        {
                            r.Background = Brushes.Yellow;
                        }
                        highlightedTags.Inlines.Add(r);
                    }
                }
            }
        }
    }
}
