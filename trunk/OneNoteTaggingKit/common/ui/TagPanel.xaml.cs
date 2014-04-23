using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    internal enum PresetFilter
    {
        CurrentNote,
        SelectedNotes,
        CurrentSection
    }

    /// <summary>
    /// Contract for data context implmementations for the <see cref="TagPanel"/> control 
    /// </summary>
    public interface ITagPanelDataContext : INotifyCollectionChanged
    {
        TagCollection ContextTags { get; }
        IEnumerable<ISelectableTagModel> TagModels { get; }
    }

    public class TagsEnteredEventArgs : RoutedEventArgs
    {
        public IEnumerable<string> Tags { get; private set; }
        internal TagsEnteredEventArgs(RoutedEvent routedEvent, object source, IEnumerable<string> tags)
            : base(routedEvent, source)
        {
            Tags = tags;
        }
    }

    public delegate void TagsEnteredEventHandler(object sender, TagsEnteredEventArgs e);

    /// <summary>
    /// Interaction logic for TagPanel.xaml
    /// </summary>
    public partial class TagPanel : UserControl
    {
        public static readonly RoutedEvent TagsEnteredEvent = EventManager.RegisterRoutedEvent("TagsEntered", RoutingStrategy.Bubble, typeof(TagsEnteredEventHandler), typeof(TagPanel));

        // Provide CLR accessors for the event 
        public event TagsEnteredEventHandler TagsEntered
        {
            add { AddHandler(TagsEnteredEvent, value); }
            remove { RemoveHandler(TagsEnteredEvent, value); }
        }

        /// <summary>
        /// Dependency property for the tag hit highlight color property.
        /// </summary>
        public static readonly DependencyProperty HighlightColorProperty = DependencyProperty.Register("HighlightColor", typeof(Brush), typeof(TagPanel));

        /// <summary>
        /// Dependency property for the tag input box tooltip.
        /// </summary>
        public static readonly DependencyProperty TagInputTooltipProperty = DependencyProperty.Register("TagInputTooltip", typeof(object), typeof(TagPanel));

        /// <summary>
        /// Dependency property for the tag panel header.
        /// </summary>
        public static readonly DependencyProperty TagPanelHeaderProperty = DependencyProperty.Register("TagPanelHeader", typeof(object), typeof(TagPanel), new PropertyMetadata("Header"));

        public static readonly RoutedEvent TagInputEvent = EventManager.RegisterRoutedEvent("TagInput", RoutingStrategy.Bubble, typeof(TagInputEventHandler), typeof(TagPanel));

        // Provide CLR accessors for the event 
        public event TagInputEventHandler TagInput
        {
            add { AddHandler(TagInputEvent, value); }
            remove { RemoveHandler(TagInputEvent, value); }
        }

        public TagPanel()
        {
            InitializeComponent();
            this.Focus();
        }

        internal void FocusInput()
        {
            tagInput.Focus();
        }

        public Brush HighlightColor
        {
            get
            {
                return (Brush)GetValue(HighlightColorProperty);
            }
            set
            {
                SetValue(HighlightColorProperty, value);
            }
        }

        public object TagInputTooltip
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
        public object TagPanelHeader
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

        internal Task<IEnumerable<TagPageSet>> GetContextTagsAsync(PresetFilter filter)
        {
            return Task<IEnumerable<TagPageSet>>.Run(() => { return GetContextTagsAction(filter); });
        }

        internal void UpdateTagFilter(IEnumerable<string> filter)
        {
            foreach (SelectableTag st in tagPanel.Children)
            {
                st.Filter = filter;
            }
        }

        private async void Filter_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem itm = sender as MenuItem;

                PresetFilter filter = (PresetFilter)Enum.Parse(typeof(PresetFilter), itm.Tag.ToString());
                IEnumerable<TagPageSet> tags = await GetContextTagsAsync(filter);

                IEnumerable<string> tagNames = from t in tags select t.TagName;

                tagInput.Tags = tagNames;

                string filterText = string.Join(",", tagNames);

                if (string.IsNullOrEmpty(filterText))
                {
                    //UpdateTagFilter(true);
                    UpdateTagFilter(null);
                    filterPopup.IsOpen = true;
                }
                else
                {
                    UpdateTagFilter(tagNames);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log(TraceCategory.Error(), "Applying preset filter failed {0}", ex);
                TraceLogger.ShowGenericMessageBox(Properties.Resources.TagEditor_Filter_Error, ex);
            }
        }

        private IEnumerable<TagPageSet> GetContextTagsAction(PresetFilter filter)
        {
            HashSet<TagPageSet> tags = new HashSet<TagPageSet>();
            ITagPanelDataContext ts = DataContext as ITagPanelDataContext;
            if (ts != null)
            {
                TagCollection contextTags = ts.ContextTags;
                if (contextTags != null)
                {
                    contextTags.Find(contextTags.CurrentWindow.CurrentSectionId, includeUnindexedPages: true);

                    switch (filter)
                    {
                        case PresetFilter.CurrentNote:
                            TaggedPage currentPage = (from p in contextTags.Pages where p.Key.Equals(contextTags.CurrentWindow.CurrentPageId) select p.Value).FirstOrDefault();
                            if (currentPage != null)
                            {
                                tags.UnionWith(currentPage.Tags);
                            }
                            break;
                        case PresetFilter.SelectedNotes:
                            foreach (var p in (from pg in contextTags.Pages where pg.Value.IsSelected select pg.Value))
                            {
                                tags.UnionWith(p.Tags);
                            }
                            break;
                        case PresetFilter.CurrentSection:
                            foreach (var p in contextTags.Pages)
                            {
                                tags.UnionWith(p.Value.Tags);
                            }
                            break;
                    }
                }
            }
            return tags;
        }

        private void handlePopupPointerAction(object sender, RoutedEventArgs e)
        {
            Popup p = sender as Popup;
            if (p != null)
            {
                p.IsOpen = false;
                FocusInput();
            }
            e.Handled = true;
        }

        internal void ClearFilter()
        {
            UpdateTagFilter(null);
            filterPopup.IsOpen = false;
            FocusInput();
        }

        private void OnTagInput(object sender, TagInputEventArgs e)
        {
            if (e.TagInputComplete)
            {
                RaiseEvent(new TagsEnteredEventArgs(TagsEnteredEvent, this, tagInput.Tags));
            }
            else
            {
                UpdateTagFilter(tagInput.Tags);
            }
        }

        private void OnTagModelListChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        ISelectableTagModel tagModel = e.NewItems[i] as ISelectableTagModel;
                        tagPanel.Children.Insert(e.NewStartingIndex + i, new SelectableTag(tagModel));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int c = e.OldItems.Count; c < 0; c--)
                    {
                        tagPanel.Children.RemoveAt(e.OldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    tagPanel.Children.Clear();
                    ObservableSortedList<TagModelKey, string, ISelectableTagModel> tags = sender as ObservableSortedList<TagModelKey, string, ISelectableTagModel>;
                    foreach (ISelectableTagModel t in tags.Values)
                    {
                        tagPanel.Children.Add(new SelectableTag(t));
                    }
                    break;
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TagPanel tp = sender as TagPanel;
            Debug.Assert(tp != null, "Parameter d must be non-null and of type TagPanel");

            ITagPanelDataContext t = e.NewValue as ITagPanelDataContext;
            Debug.Assert(t != null, "Property New VAlue d must be of type ITagSource");
            t.CollectionChanged += tp.OnTagModelListChanged;
        }
    }
}
