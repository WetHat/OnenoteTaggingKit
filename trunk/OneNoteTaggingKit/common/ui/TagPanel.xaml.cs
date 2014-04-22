using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    internal enum PresetFilter
    {
        CurrentNote,
        SelectedNotes,
        CurrentSection
    }

    public interface ITagSource : INotifyCollectionChanged
    {
        TagCollection ContextTags { get; }
        IEnumerable<ISelectableTagModel> TagModels { get; }
    }

    /// <summary>
    /// Interaction logic for TagPanel.xaml
    /// </summary>
    public partial class TagPanel : UserControl
    {
        /// <summary>
        /// Dependency property for the tag input box tooltip.
        /// </summary>
        public static readonly DependencyProperty TagInputTooltipProperty = DependencyProperty.Register("TagInputTooltip", typeof(object), typeof(TagPanel));

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

        /// <summary>
        /// Dependency property for the tag panel  tooltip.
        /// </summary>
        public static readonly DependencyProperty TagPanelTooltipProperty = DependencyProperty.Register("TagPanelTooltip", typeof(object), typeof(TagPanel));

        public object TagPanelTooltip
        {
            get
            {
                return GetValue(TagPanelTooltipProperty);
            }
            set
            {
                SetValue(TagPanelTooltipProperty, value);
            }
        }

        /// <summary>
        /// Dependency property for the tag hit highlight color property.
        /// </summary>
        public static readonly DependencyProperty HighlightColorProperty = DependencyProperty.Register("HighlightColor", typeof(Brush), typeof(TagPanel));

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

        /// <summary>
        /// Dependency property for the tag panel header.
        /// </summary>
        public static readonly DependencyProperty TagPanelHeaderProperty = DependencyProperty.Register("TagPanelHeader", typeof(object), typeof(TagPanel),new PropertyMetadata("Header"));

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

        /// <summary>
        /// Dependency property for the tag panel header.
        /// </summary>
        public static readonly DependencyProperty TagSourceProperty = DependencyProperty.Register("TagSource",
                                                                                                  typeof(object),
                                                                                                  typeof(ITagSource),
                                                                                                  new PropertyMetadata(OnTagSourceChanged));
        public ITagSource TagSource
        {
            get
            {
                return (ITagSource)GetValue(TagSourceProperty);
            }
            set
            {
                SetValue(TagSourceProperty, value);
            }
        }

        private static void OnTagSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
           TagPanel tp = d as TagPanel;
           Debug.Assert(tp != null, "Parameter d must be non-null and of type TagPanel");

           ITagSource t = e.NewValue as ITagSource;
           Debug.Assert(t != null, "Property New VAlue d must be of type ITagSource");
           t.CollectionChanged += tp.OnTagModelListChanged;
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
        public TagPanel()
        {
            InitializeComponent();
        }

        private void OnTagInput(object sender, TagInputEventArgs e)
        {

        }

        internal Task<IEnumerable<TagPageSet>> GetContextTagsAsync(PresetFilter filter)
        {
            return Task<IEnumerable<TagPageSet>>.Run(() => { return GetContextTagsAction(filter); });
        }

        private IEnumerable<TagPageSet> GetContextTagsAction(PresetFilter filter)
        {
            HashSet<TagPageSet> tags = new HashSet<TagPageSet>();
            ITagSource ts = TagSource;
            if (ts != null)
            {
                TagCollection contextTags = ts.ContextTags;
                if (contextTags != null)
                {
                    contextTags.Find(contextTags.CurrentWindow.CurrentSectionId, includeUnindexedPages: true);

                    switch (filter)
                    {
                        case PresetFilter.CurrentNote:
                            TaggedPage currentPage = (from p in contextTags.Pages where p.Key.Equals(OneNote.Windows.CurrentWindow.CurrentPageId) select p.Value).FirstOrDefault();
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
                    _model.UpdateTagFilter(null);
                    filterPopup.IsOpen = true;
                }
                else
                {
                    _model.UpdateTagFilter(tagNames);
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log(TraceCategory.Error(), "Applying preset filter failed {0}", ex);
                TraceLogger.ShowGenericMessageBox(Properties.Resources.TagEditor_Filter_Error, ex);
            }
        }

        private void handlePopupPointerAction(object sender, RoutedEventArgs e)
        {
            Popup p = sender as Popup;
            if (p != null)
            {
                p.IsOpen = false;
            }
            e.Handled = true;
        }
    }
}
