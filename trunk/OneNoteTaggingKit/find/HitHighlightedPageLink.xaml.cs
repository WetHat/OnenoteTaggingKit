using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WetHatLab.OneNote.TaggingKit.common;

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

            foreach (TextFragment f in model.HighlightedTitle)
            {
                Run r = new Run(f.Text);
                if (f.IsMatch)
                {
                    r.Background = Brushes.Yellow;
                }
                link.hithighlightedTitle.Inlines.Add(r);
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

        private void Copy_Link_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement l = sender as FrameworkElement;
            if (l != null)
            {
                HitHighlightedPageLinkModel mdl = l.DataContext as HitHighlightedPageLinkModel;
                string pageTitle = mdl.LinkTitle;
                try
                {
                    if (mdl != null)
                    {
                        string header =
@"Version:0.9
StartHTML:{0:D6}
EndHTML:{1:D6}
StartFragment:{2:D6}
EndFragment:{3:D6}
StartSelection:{4:D6}
EndSelection:{5:D6}";
                        string htmlpre =
@"<HTML>
<BODY>
<!--StartFragment-->";
                        string link = string.Format(@"<a href=""{0}"">{1}</a>", mdl.PageLink, pageTitle);
                        string htmlpost =
@"<!--EndFragment-->
</BODY>
</HTML>";
                        string clip = string.Format(header,
                            header.Length,
                            header.Length + htmlpre.Length + link.Length + htmlpost.Length,
                            header.Length + htmlpre.Length,
                            header.Length + htmlpre.Length + link.Length,
                            header.Length + htmlpre.Length,
                            header.Length + htmlpre.Length + link.Length)
                            + htmlpre + link + htmlpost;
                        Clipboard.SetText(clip, TextDataFormat.Html);
                    }
                }
                catch (Exception ex)
                {
                    TraceLogger.Log(TraceCategory.Error(), "Link to page '{0}' could not be created: {1}", pageTitle,ex);
                    TraceLogger.ShowGenericMessageBox(Properties.Resources.TagSearch_Error_CopyLink, ex);
                }
                e.Handled = true;
            }
        }
    }
}
