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
        #region TagClickEvent
        /// <summary>
        /// Definition of the click routed event.
        /// </summary>
        public static readonly RoutedEvent TagClickEvent = EventManager.RegisterRoutedEvent(
            nameof(TagClick),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(Tag));

        /// <summary>
        /// Track changes to tag (de)selection.
        /// </summary>
        public event RoutedEventHandler TagClick {
            add { AddHandler(TagClickEvent, value); }
            remove { RemoveHandler(TagClickEvent, value); }
        }
        private void tagBtn_Click(object sender, RoutedEventArgs e) {
            RaiseEvent(new TagSelectedEventArgs(TagClickEvent, this, true));
        }
        #endregion TagSelectedEvent

        /// <summary>
        /// Initialize a new instance of a tag control.
        /// </summary>
        public Tag() {
            InitializeComponent();
            tagIndicator.FontSize = tagBtn.FontSize - 1.0;
        }
    }
}
