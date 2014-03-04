using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using WetHatLab.OneNote.TaggingKit.collections;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// Interaction logic for SimpleTagsPanel.xaml
    /// </summary>
    public partial class SimpleTagsPanel : UserControl
    {

        /// <summary>
        /// Dependecy property for the collection of tags this panel displays.
        /// </summary>
        public static readonly DependencyProperty TagsProperty = DependencyProperty.Register("Tags", typeof(ObservableSortedList<SimpleTagButtonModel>), typeof(SimpleTagsPanel), new PropertyMetadata(null, OnTagsPropertyChanged));

        /// <summary>
        /// Get or set the Collection of tags this panel should display
        /// </summary>
        /// <remarks>clr wrapper of the <see cref="TagsProperty"/> dependency property</remarks>
        internal ObservableSortedList<SimpleTagButtonModel> Tags
        {
            get { return (ObservableSortedList<SimpleTagButtonModel>)GetValue(TagsProperty); }
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
                ((ObservableSortedList<SimpleTagButtonModel>)e.OldValue).CollectionChanged -= panel.OnTagCollectionChanged;
            }
            if (e.NewValue != null)
            {
                ((ObservableSortedList<SimpleTagButtonModel>)e.NewValue).CollectionChanged += panel.OnTagCollectionChanged;
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

        void TagButton_Click(object sender, RoutedEventArgs e)
        {
            SimpleTagButton btn = sender as SimpleTagButton;
            Tags.RemoveAll(new SimpleTagButtonModel[] { (SimpleTagButtonModel)btn.DataContext });
        }

        /// <summary>
        /// Handle changes to the collection of tags in the underlying model
        /// </summary>
        /// <param name="sender"><see cref="ObservableSortedList&lt;Tvalue&gt;"/> maintaining the listof tags</param>
        /// <param name="e">event details</param>
        private void OnTagCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableSortedList<SimpleTagButtonModel> sortedTags = sender as ObservableSortedList<SimpleTagButtonModel>;
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
                    int removedItemIndex = e.OldStartingIndex;
                    foreach (SimpleTagButtonModel t in e.OldItems)
                    {
                        tagsPanel.Children.RemoveAt(removedItemIndex++);
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
