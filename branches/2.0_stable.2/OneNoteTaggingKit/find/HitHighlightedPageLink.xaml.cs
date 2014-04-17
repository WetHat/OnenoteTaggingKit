using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Interaction logic for HitHighlightedPageLink.xaml
    /// </summary>
    public partial class HitHighlightedPageLink : UserControl
    {
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HitHighlightedPageLink));

        // Provide CLR accessors for the event 
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HitHighlightedPageLink link = sender as HitHighlightedPageLink;
            HitHighlightedPageLinkModel model = e.NewValue as HitHighlightedPageLinkModel;
            // build the highlighted inline Text
           
            // rebuild the hithighlighted Title
            link.hithighlightedTitle.Inlines.Clear();
            if (model.Matches != null && model.Matches.Count > 0)
            {
                int afterLastHighlight = 0;
                foreach (Match m in model.Matches)
                {
                    // create a plain run between the last highlight and this highlight
                    if (m.Index > afterLastHighlight)
                    {
                        link.hithighlightedTitle.Inlines.Add(new Run(model.PageTitle.Substring(afterLastHighlight, m.Index - afterLastHighlight)));
                    }
                    // add a highlighted Run
                    Run r = new Run(model.PageTitle.Substring(m.Index, m.Length));
                    r.Background=Brushes.Yellow;
                    link.hithighlightedTitle.Inlines.Add(r);
                    afterLastHighlight = m.Index + m.Length;
                }
                // add remaining plain text
                if (afterLastHighlight < model.PageTitle.Length)
                {
                    link.hithighlightedTitle.Inlines.Add(new Run(model.PageTitle.Substring(afterLastHighlight, model.PageTitle.Length - afterLastHighlight)));
                }
            }
            else
            {
                link.hithighlightedTitle.Inlines.Add(new Run(model.PageTitle));
            }

            // rebuild the hit highlighted Tooltip
            ToolTip tt = new ToolTip();
            tt.Style = new Style(); // override the global style

            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Vertical;
            tt.Content = stack;

            TextBlock path = new TextBlock();
            path.Foreground = Brushes.Gray;
            //path.FontSize = 10;
            path.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            path.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            foreach (var he in model.Path)
            {
                if (path.Inlines.Count > 0)
                {
                    Run r = new Run(" ñ ");
                    r.FontFamily = new FontFamily("Symbol");
                    r.FontWeight = FontWeights.ExtraBold;
                    r.Foreground = Brushes.Black;
                    path.Inlines.Add(r);
                }
                path.Inlines.Add(new Run(he.Name));
            }
            stack.Children.Add(path);
            
            TextBlock tb = new TextBlock();
            tb.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            
            foreach (Run r in link.hithighlightedTitle.Inlines)
            {
                Run newR = new Run(r.Text);
                newR.Background = r.Background;
                newR.FontSize = r.FontSize + 2;
                newR.FontWeight = FontWeights.Medium;
                tb.Inlines.Add(newR);
            }
            stack.Children.Add(tb);

            link.ToolTip = tt;
            ToolTipService.SetShowDuration(link, 10000);
        }

        public HitHighlightedPageLink()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent( new RoutedEventArgs(ClickEvent));
        }
    }
}
