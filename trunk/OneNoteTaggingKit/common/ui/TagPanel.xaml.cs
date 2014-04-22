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
    public class TagSelectionChangedEventArgs : RoutedEventArgs
    {
        public bool IsSelected { get; private set; }
        internal TagSelectionChangedEventArgs(RoutedEvent routedEvent, object source, bool isSelected)
            : base(routedEvent, source)
        {
            IsSelected = isSelected;
        }
    }
    
    public delegate void TagSelectionChangedEventHandler(object sender, TagSelectionChangedEventArgs args);

    /// <summary>
    /// Interaction logic for TagPanel.xaml
    /// </summary>
    public partial class TagPanel : UserControl
    {

        public static readonly RoutedEvent TagSelectionChangedEvent = EventManager.RegisterRoutedEvent("TagSelectionChanged", RoutingStrategy.Bubble, typeof(TagInputEventHandler), typeof(TagPanel));

        // Provide CLR accessors for the event 
        public event TagInputEventHandler TagSelectionChanged
        {
            add { AddHandler(TagSelectionChangedEvent, value); }
            remove { RemoveHandler(TagSelectionChangedEvent, value); }
        }


        /// <summary>
        /// Dependency property for the tag input box tooltip.
        /// </summary>
        public static readonly DependencyProperty TagInputTooltipProperty = DependencyProperty.Register("TagInputTooltip", typeof(object), typeof(TagPanel));

        internal object TagInputTooltip
        {
            get
            {
                return GetValue(TagInputTooltipProperty);
            }
            set
            {
                SetValue(TagInputTooltipProperty, value);
            }
        }


        /// <summary>
        /// Dependency property for the tag panel header.
        /// </summary>
        public static readonly DependencyProperty TagPanelHeaderProperty = DependencyProperty.Register("TagPanelHeaderTooltip", typeof(object), typeof(TagPanel),new PropertyMetadata("Header"));

        internal object TagPanelHeaderTooltip
        {
            get
            {
                return GetValue(TagPanelHeaderProperty);
            }
            set
            {
                SetValue(TagPanelHeaderProperty, value);
            }
        }

        public TagPanel()
        {
            InitializeComponent();
        }

        private void OnTagInput(object sender, TagInputEventArgs e)
        {

        }

        private void Filter_MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
