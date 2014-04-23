using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    public interface IFilterableTagDataContext
    {
        TextSplitter Highlighter { set; }
    }

    public interface ITagSource: INotifyCollectionChanged
    {
        IEnumerable<IFilterableTagDataContext> TagDataContextCollection { get; }
    }

    /// <summary>
    /// Interaction logic for HighlightedTagsPanel.xaml
    /// </summary>
    public partial class HighlightedTagsPanel : UserControl
    {
        /// <summary>
        /// Dependency property for the tag templage.
        /// </summary>
        public static readonly DependencyProperty TagTemplateProperty = DependencyProperty.Register("TagTemplate", typeof(DataTemplate), typeof(HighlightedTagsPanel));

        public DataTemplate TagTemplate
        {
            get
            {
                return GetValue(TagTemplateProperty) as DataTemplate;
            }
            set
            {
                SetValue(TagTemplateProperty, value);
            }
        }

        /// <summary>
        /// Dependency property for panel header.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(HighlightedTagsPanel));
        
        public object Header
        {
            get
            {
                return GetValue(HeaderProperty);
            }
            set
            {
                SetValue(HeaderProperty, value);
            }
        }

        /// <summary>
        /// Dependency property for panel header.
        /// </summary>
        public static readonly DependencyProperty TagSourceProperty = DependencyProperty.Register("TagSource", typeof(ITagSource), typeof(HighlightedTagsPanel),new PropertyMetadata(OnTagSourceChanged));

        private static void OnTagSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HighlightedTagsPanel panel = d as HighlightedTagsPanel;
            if (d != null)
            {
                ITagSource old = e.OldValue as ITagSource;
                if (old != null)
                {
                    old.CollectionChanged -= panel.OnTagdataContextCollectionChanged;
                }
                ITagSource newTagSource = e.NewValue as ITagSource;
                if (newTagSource != null)
                {
                    newTagSource.CollectionChanged += panel.OnTagdataContextCollectionChanged;
                    panel.OnTagdataContextCollectionChanged(newTagSource, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }
        }

        private void OnTagdataContextCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ITagSource tagsource = sender as ITagSource;
            if (tagsource != null)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        DataTemplate tpl = TagTemplate;
                        int newItemCount = e.NewItems.Count;
                        for (int i = 0; i < newItemCount; i++)
                        {
                            FrameworkElement tagControl = tpl.LoadContent() as FrameworkElement;
                            tagControl.DataContext = e.NewItems[i];

                            tagsPanel.Children.Insert(i + e.NewStartingIndex, tagControl);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        int oldItemCount = e.OldItems.Count;
                        for (int i = 0; i < oldItemCount; i++)
                        {
                            tagsPanel.Children.RemoveAt(e.OldStartingIndex);
                        }
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        tagsPanel.Children.Clear();
                        break;
                }
            }
        }

        public ITagSource TagSource
        {
            get
            {
                return GetValue(TagSourceProperty) as ITagSource;
            }
            set
            {
                SetValue(TagSourceProperty, value);
            }
        }

        /// <summary>
        /// Dependency property for the tag highlighter.
        /// </summary>
        public static readonly DependencyProperty HighlighterProperty = DependencyProperty.Register("Highlighter", typeof(TextSplitter), typeof(HighlightedTagsPanel),new PropertyMetadata(OnHighlighterChanged));

        private static void OnHighlighterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HighlightedTagsPanel panel = d as HighlightedTagsPanel;
            if (panel != null)
            {
                ITagSource tagsource = panel.TagSource;
                if (tagsource != null)
                {
                    TextSplitter highlighter = e.NewValue as TextSplitter;
                    if (highlighter == null)
                    {
                        highlighter = new TextSplitter();
                    }

                    foreach (IFilterableTagDataContext ctx in tagsource.TagDataContextCollection)
                    {
                        ctx.Highlighter = highlighter;
                    }
                }
            }
        }


        internal TextSplitter Highlighter
        {
            get
            {
                return GetValue(HighlighterProperty) as TextSplitter;
            }
            set
            {
                SetValue(HighlighterProperty, value);
            }
        }

        public HighlightedTagsPanel()
        {
            InitializeComponent();
        }
    }
}
