using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WetHatLab.OneNote.TaggingKit.find;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    /// Interaction logic for RemovableTag.xaml
    /// </summary>
    [ComVisible(false)]
    public partial class RemovableTag : UserControl
    {
        /// <summary>
        /// Click event for this button.
        /// </summary>
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RemovableTag));

        /// <summary>
        /// create a new instance of a <see cref="RemovableTag"/> user control
        /// </summary>
        public RemovableTag()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Add or remove the click handler.
        /// </summary>
        /// <remarks>clr wrapper for routed event</remarks>
        internal event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }

            remove { RemoveHandler(ClickEvent, value); }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs newClickEventArgs = new RoutedEventArgs(ClickEvent, this);

            RaiseEvent(newClickEventArgs);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RemovableTag t = sender as RemovableTag;

            RemovableTagModel mdl = t.DataContext as RemovableTagModel;
            if (mdl != null)
            {
                mdl.PropertyChanged += mdl_PropertyChanged;
                mdl_PropertyChanged(mdl,RemovableTagModel.HIGHLIGHTED_TAGNAME);
            }
        }

        void mdl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RemovableTagModel mdl = sender as RemovableTagModel;
            if (mdl != null)
            {
                if (e == RemovableTagModel.HIGHLIGHTED_TAGNAME)
                {
                    highlighedTag.Inlines.Clear();
                    highlighedTag.Inlines.AddRange(mdl.HighlightedTagName.Select((f) =>
                    {
                        Run r = new Run(f.Text);
                        if (f.IsMatch)
                        {
                            r.Background = Brushes.Yellow;
                        }
                        return r;
                    }));
                }
            }
        }
    }
}
