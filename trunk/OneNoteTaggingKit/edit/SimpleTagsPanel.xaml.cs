using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
        public static readonly DependencyProperty TagsProperty = DependencyProperty.Register("Tags", typeof(ObservableSortedList<SimpleTag>), typeof(SimpleTagsPanel), new PropertyMetadata(null, OnTagsPropertyChanged));

        /// <summary>
        /// Get or set the Collection of tags this panel should display
        /// </summary>
        /// <remarks>clr wrapper of the <see cref="TagsProperty"/> dependency property</remarks>
        internal ObservableSortedList<SimpleTag> Tags
        {
            get { return (ObservableSortedList<SimpleTag>)GetValue(TagsProperty); }
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
                ((ObservableSortedList<SimpleTag>)e.OldValue).CollectionChanged -= panel.OnTagCollectionChanged;
            }
            if (e.NewValue != null)
            {
                ((ObservableSortedList<SimpleTag>)e.NewValue).CollectionChanged += panel.OnTagCollectionChanged;
                panel.OnTagCollectionChanged(panel, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private SimpleTagButton createTagButton(SimpleTag tag)
        {
            SimpleTagButton btn = new SimpleTagButton()
            {
                TagName = tag,
                Margin = new Thickness(0,5,5,0)
            };

            btn.Click += TagButton_Click;
            return btn;
        }

        void TagButton_Click(object sender, RoutedEventArgs e)
        {
            SimpleTagButton btn = sender as SimpleTagButton;
            Tags.RemoveAll(new SimpleTag[] { btn.TagName });
        }

        /// <summary>
        /// Handle changes to the collection of tags in the underlying model
        /// </summary>
        /// <param name="sender"><see cref="ObservableSortedList&lt;Tvalue&gt;"/> maintaining the listof tags</param>
        /// <param name="e">event details</param>
        private void OnTagCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ObservableSortedList<SimpleTag> sortedTags = sender as ObservableSortedList<SimpleTag>;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    int newitemIndex = e.NewStartingIndex;
                    foreach (SimpleTag t in e.NewItems)
                    {
                        tagsPanel.Children.Insert(newitemIndex++, createTagButton(t));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    int removedItemIndex = e.OldStartingIndex;
                    foreach (SimpleTag t in e.OldItems)
                    {
                        tagsPanel.Children.RemoveAt(removedItemIndex++);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    tagsPanel.Children.Clear();
                    foreach (SimpleTag t in Tags.Values)
                    {
                        tagsPanel.Children.Add(createTagButton(t));
                    }

                    break;
            }
        }
    }
}
