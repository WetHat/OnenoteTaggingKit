using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace WetHatLab.OneNote.TaggingKit.edit
{

    /// <summary>
    ///  A simple Tag Button.
    /// </summary>
    [ComVisible(false)]
    public partial class SimpleTagButton : UserControl
    {
        /// <summary>
        /// Click event for this button.
        /// </summary>
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SimpleTagButton));

        /// <summary>
        /// Create a new instance of a simple Tag button.
        /// </summary>
        public SimpleTagButton()
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
            RoutedEventArgs newClickEventArgs = new RoutedEventArgs(ClickEvent,this);

            RaiseEvent(newClickEventArgs);
        }
    }
}
