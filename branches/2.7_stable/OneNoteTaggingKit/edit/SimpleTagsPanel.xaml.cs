using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// Interaction logic for SimpleTagsPanel.xaml
    /// </summary>
    [ComVisible(false)]
    public partial class SimpleTagsPanel : UserControl
    {

        /// <summary>
        /// Dependency property for the collection of tags this panel displays.
        /// </summary>
        public static readonly DependencyProperty TagsProperty = DependencyProperty.Register("Tags", typeof(ObservableSortedList<TagModelKey, string, SimpleTagButtonModel>), typeof(SimpleTagsPanel), new PropertyMetadata(null, OnTagsPropertyChanged));

        /// <summary>
        /// Click event for this button.
        /// </summary>
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SimpleTagsPanel));

        /// <summary>
        /// Get or set the Collection of tags this panel should display
        /// </summary>
        /// <remarks>clr wrapper of the <see cref="TagsProperty"/> dependency property</remarks>
        internal ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> Tags
        {
            get { return (ObservableSortedList<TagModelKey, string, SimpleTagButtonModel>)GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        /// <summary>
        /// Create a new instance of a simple tags panel.
        /// </summary>
        public SimpleTagsPanel()
        {
            InitializeComponent();
        }

        private static void OnTagsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            SimpleTagsPanel panel = (SimpleTagsPanel)source;

            if (e.OldValue != null)
            {
                ((ObservableSortedList<TagModelKey, string, SimpleTagButtonModel>)e.OldValue).CollectionChanged -= panel.OnTagCollectionChanged;
            }
            if (e.NewValue != null)
            {
                ((ObservableSortedList<TagModelKey, string, SimpleTagButtonModel>)e.NewValue).CollectionChanged += panel.OnTagCollectionChanged;
                panel.OnTagCollectionChanged(panel, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private SimpleTagButton createTagButton(SimpleTagButtonModel tag)
        {
            SimpleTagButton btn = new SimpleTagButton()
            {
                DataContext = tag,
                Margin = new Thickness(0,5,5,0)
            };

            btn.Click += TagButton_Click;
            return btn;
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

        void TagButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (!e.Handled)
            {
                RaiseEvent(new RoutedEventArgs(ClickEvent,sender));
            }
        }

        /// <summary>
        /// Handle changes to the collection of tags in the underlying model
        /// </summary>
        /// <param name="sender">collection of sorted tags</param>
        /// <param name="e">event details</param>
        private void OnTagCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> sortedTags = sender as ObservableSortedList<TagModelKey, string, SimpleTagButtonModel>;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    int newitemIndex = e.NewStartingIndex;
                    foreach (SimpleTagButtonModel t in e.NewItems)
                    {
                        tagsPanel.Children.Insert(newitemIndex++, createTagButton(t));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (SimpleTagButtonModel t in e.OldItems)
                    {
                        tagsPanel.Children.RemoveAt(e.OldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    tagsPanel.Children.Clear();
                    foreach (SimpleTagButtonModel t in Tags.Values)
                    {
                        tagsPanel.Children.Add(createTagButton(t));
                    }

                    break;
            }
        }
    }
}
