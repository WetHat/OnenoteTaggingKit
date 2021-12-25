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

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// Interaction logic for Tag.xaml
    /// </summary>
    public partial class Tag : UserControl
    {
        #region TagSelectedEvent
        public static readonly RoutedEvent TagSelectedEvent = EventManager.RegisterRoutedEvent(
            nameof(TagSelected),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(Tag));

        /// <summary>
        /// Track changes to tag (de)selection.
        /// </summary>
        public event RoutedEventHandler TagSelected {
            add { AddHandler(TagSelectedEvent, value); }
            remove { RemoveHandler(TagSelectedEvent, value); }
        }
        private void tagBtn_Click(object sender, RoutedEventArgs e) {
            RaiseEvent(new TagSelectedEventArgs(TagSelectedEvent, this, true));
        }
        #endregion TagSelectedEvent

        public Tag() {
            InitializeComponent();
        }


    }
}
