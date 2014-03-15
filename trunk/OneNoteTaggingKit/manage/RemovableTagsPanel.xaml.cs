using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    /// Interaction logic for RemovableTagsPanel.xaml user control.
    /// </summary>
    public partial class RemovableTagsPanel : UserControl
    {
        /// <summary>
        /// Dependecy property for the collection of tags this panel displays.
        /// </summary>
        public static readonly DependencyProperty TagsProperty = DependencyProperty.Register("Tags", typeof(ObservableSortedList<TagModelKey, string, RemovableTagModel>), typeof(RemovableTagsPanel), new PropertyMetadata(null, OnTagsPropertyChanged));

        /// <summary>
        /// Get or set the Collection of tags this panel should display
        /// </summary>
        /// <remarks>clr wrapper of the <see cref="TagsProperty"/> dependency property</remarks>
        internal ObservableSortedList<TagModelKey, string, RemovableTagModel> Tags
        {
            get { return (ObservableSortedList<TagModelKey, string, RemovableTagModel>)GetValue(TagsProperty); }
            set { SetValue(TagsProperty, value); }
        }

        /// <summary>
        /// Create a new instance of a simple tags panel.
        /// </summary>
        public RemovableTagsPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Track changes to the property representing the collection of tags. 
        /// </summary>
        /// <param name="source">event source.</param>
        /// <param name="e">even details</param>
        /// <seealso cref="TagsProperty"/>
        private static void OnTagsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            RemovableTagsPanel panel = (RemovableTagsPanel)source;

            if (e.OldValue != null)
            {
                ((ObservableSortedList<TagModelKey, string, RemovableTagModel>)e.OldValue).CollectionChanged -= panel.OnTagCollectionChanged;
            }
            if (e.NewValue != null)
            {
                ((ObservableSortedList<TagModelKey, string, RemovableTagModel>)e.NewValue).CollectionChanged += panel.OnTagCollectionChanged;
                panel.OnTagCollectionChanged(panel, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// create a user control for a tag
        /// </summary>
        /// <param name="tag">backing view model</param>
        /// <returns>tag user control</returns>
        private RemovableTag createTagButton(RemovableTagModel tag)
        {
            RemovableTag btn = new RemovableTag()
            {
                DataContext = tag,
                Margin = new Thickness(5,5,0,0)
            };

            btn.Click += TagButton_Click;
            return btn;
        }

        /// <summary>
        /// remove tag from suggestions when user control is tapped.
        /// </summary>
        /// <param name="sender">user control emitting this event</param>
        /// <param name="e">event details</param>
        private void TagButton_Click(object sender, RoutedEventArgs e)
        {
            RemovableTag btn = sender as RemovableTag;
            Tags.RemoveAll(new string[] { ((RemovableTagModel)btn.DataContext).Key });
        }

        /// <summary>
        /// Handle changes to the collection of tags in the underlying model
        /// </summary>
        /// <param name="sender"><see cref="ObservableSortedList&lt;Tvalue&gt;"/> maintaining the listof tags</param>
        /// <param name="e">event details</param>
        private void OnTagCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableSortedList<TagModelKey, string, RemovableTagModel> sortedTags = sender as ObservableSortedList<TagModelKey, string, RemovableTagModel>;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    int newitemIndex = e.NewStartingIndex;
                    foreach (RemovableTagModel t in e.NewItems)
                    {
                        tagsPanel.Children.Insert(newitemIndex++, createTagButton(t));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    int removedItemIndex = e.OldStartingIndex;
                    foreach (RemovableTagModel t in e.OldItems)
                    {
                        tagsPanel.Children.RemoveAt(removedItemIndex++);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    tagsPanel.Children.Clear();
                    foreach (RemovableTagModel t in Tags.Values)
                    {
                        tagsPanel.Children.Add(createTagButton(t));
                    }
                    break;
            }
        }
    }
}
