using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WetHatLab.OneNote.TaggingKit
{
    /// <summary>
    /// Interaction logic for TagsPanel.xaml
    /// </summary>
    public partial class TagsPanel : UserControl
    {
        /// <summary>
        /// Dependecy property for the collection of tags this panel displays.
        /// </summary>
        public static readonly DependencyProperty TagsProperty = DependencyProperty.Register("Tags", typeof(ObservableCollection<string>), typeof(TagsPanel), new PropertyMetadata(null, OnTagsPropertyChanged));

        // list of buttons sorted by tag
        private SortedList<string, Button> _TagButtons = new SortedList<string, Button>();

        private static void OnTagsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            TagsPanel panel = (TagsPanel)source;

            if (e.OldValue != null)
            {
                ((ObservableCollection<string>)e.OldValue).CollectionChanged -=  panel.OnTagCollectionChanged;
            }
            if (e.NewValue != null)
            {
                ((ObservableCollection<string>)e.NewValue).CollectionChanged += panel.OnTagCollectionChanged;
                panel.OnTagCollectionChanged(panel, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Create a new instance of the panel.
        /// </summary>
        public TagsPanel()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Get or set the collections of tags displayed by this panel 
        /// </summary>
        /// <remarks>This is the CLR interface for the <see cref="TagsProperty"/> dependency property.</remarks>
        public ObservableCollection<string> Tags
        {
            get
            {
                return (ObservableCollection<string>)GetValue(TagsProperty);
            }
            set
            {
                SetValue(TagsProperty, value);
            }
        }
        

        private Button createTagButton(string tag)
        {
            // create a button for a tag like so:
            //<Button Name="RemoveTagButton" Background="Transparent" Click="RemoveTagButton_Click" BorderBrush="LightBlue" ToolTip="Remove Tag">
            //    <StackPanel Orientation="Horizontal">
            //        <TextBlock Background="White" Text="tag" Margin="0,0,5,0"/>
            //        <TextBlock FontFamily="Segoe UI Symbol" Background="White" Foreground="Red" Text="❌" Width="10" Height="10" FontSize="8" TextAlignment="Center"/>
            //    </StackPanel>
            //</Button>
            Button tagButton = new Button();
            tagButton.Tag = tag;
            tagButton.Margin = new Thickness(3, 3, 0, 0);
            tagButton.Background = Brushes.White;
            tagButton.BorderBrush = Brushes.LightBlue;
            tagButton.Click += RemoveTagButton_Click;
            tagButton.ToolTip = Properties.Resources.TagsPanel_RemoveButton_Tooltip;

            StackPanel stk = new StackPanel();
            stk.Orientation = Orientation.Horizontal;

            tagButton.Content = stk;
            TextBlock t1 = new TextBlock();
            t1.Background = Brushes.White;
            t1.Text = tag;
            t1.Margin = new Thickness(0, 0, 5, 0);
            stk.Children.Add(t1);

            TextBlock t2 = new TextBlock();
            t2.Background = Brushes.White;
            t2.Foreground = Brushes.Red;
            t2.Text = "❌";
            t2.FontFamily = new FontFamily("Segoe UI Symbol");
            t2.Width = 10;
            t2.Height = 10;
            t2.FontSize = 8;
            t2.TextAlignment = TextAlignment.Center;

            stk.Children.Add(t2);

            return tagButton;
        }

        private void OnTagCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (string t in e.NewItems)
                    {
                        Button btn = createTagButton(t);

                        _TagButtons.Add(t, btn);
                        tagsPanel.Children.Insert(_TagButtons.IndexOfKey(t), btn);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (string t in e.OldItems)
                    {
                        int i = _TagButtons.IndexOfKey(t);
                        _TagButtons.RemoveAt(i);
                        tagsPanel.Children.RemoveAt(i);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    tagsPanel.Children.Clear();
                    foreach (string t in Tags)
                    {
                        Button btn = createTagButton(t);
                        _TagButtons.Add(t, btn);
                        tagsPanel.Children.Insert(_TagButtons.IndexOfKey(t), btn);
                    }
                    break;
            }
        }

        private void RemoveTagButton_Click(object sender, RoutedEventArgs e)
        {
            Tags.Remove((string)(((Button)sender).Tag));
            e.Handled = true;
        }
    }
}
