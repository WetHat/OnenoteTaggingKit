using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    public class TagInputEventArgs : RoutedEventArgs
    {
        public bool TagInputComplete { get; private set; }
        internal TagInputEventArgs(RoutedEvent routedEvent, object source, bool enterPressed)
            : base(routedEvent, source)
        {
            TagInputComplete = enterPressed;
        }
    }
    public delegate void TagInputEventHandler(object sender, TagInputEventArgs e);

    /// <summary>
    /// Interaction logic for TagInputBox.xaml
    /// </summary>
    public partial class TagInputBox : UserControl
    {
        public static readonly RoutedEvent TagInputEvent = EventManager.RegisterRoutedEvent("TagInput", RoutingStrategy.Bubble, typeof(TagInputEventHandler), typeof(TagInputBox));

        // Provide CLR accessors for the event 
        public event TagInputEventHandler TagInput
        {
            add { AddHandler(TagInputEvent, value); }
            remove { RemoveHandler(TagInputEvent, value); }
        }

        /// <summary>
        /// Dependency property for the context tags source
        /// </summary>
        public static readonly DependencyProperty ContextTagsSourceProperty = DependencyProperty.Register("ContextTagsSource", typeof(TagCollection), typeof(TagInputBox),new PropertyMetadata(OnContextTagSourceChanged));

        private static void OnContextTagSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TagInputBox ib = d as TagInputBox;
            if (ib != null)
            {
                ib.presetsMenu.Visibility = e.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public TagCollection ContextTagsSource
        {
            get
            {
                return GetValue(ContextTagsSourceProperty) as TagCollection;
            }
            set
            {
                SetValue(ContextTagsSourceProperty, value);
            }
        }

        public TagInputBox()
        {
            InitializeComponent();
        }

        internal bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(tagInput.Text);
            }
        }

        internal IEnumerable<string> Tags
        {
            get
            {
                return from t in OneNotePageProxy.ParseTags(tagInput.Text) select CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t);
            }
            set
            {
                tagInput.Text = string.Join(",", value);

                UpdateVisibility();
            }
        }

        internal bool FocusInput()
        {
            return tagInput.Focus();
        }

        internal void Clear()
        {
            tagInput.Text = String.Empty;
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            if (string.IsNullOrEmpty(tagInput.Text))
            {
                tagInput.Background = Brushes.Transparent;
                clearTagInput.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                tagInput.Background = Brushes.White;
                clearTagInput.Visibility = System.Windows.Visibility.Visible;
            }
            tagInput.Focus();
        }

        private void TagInput_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateVisibility();
            RaiseEvent(new TagInputEventArgs(TagInputEvent, this, e.Key == System.Windows.Input.Key.Enter));
            e.Handled = true;
        }

        private void ClearInputButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            RaiseEvent(new TagInputEventArgs(TagInputEvent, this, enterPressed: false));
            e.Handled = true;
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

        private Task<IEnumerable<TagPageSet>> GetContextTagsAsync(PresetFilter filter)
        {
            TagCollection tags = ContextTagsSource;
            return Task<IEnumerable<TagPageSet>>.Run(() => { return GetContextTagsAction(filter,tags); });
        }

        private IEnumerable<TagPageSet> GetContextTagsAction(PresetFilter filter, TagCollection contextTags)
        {
            HashSet<TagPageSet> tags = new HashSet<TagPageSet>();

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
            return tags;
        }

        private async void Filter_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ContextTagsSource == null)
                {
                    return;
                }

                MenuItem itm = sender as MenuItem;

                PresetFilter filter = (PresetFilter)Enum.Parse(typeof(PresetFilter), itm.Tag.ToString());
                IEnumerable<TagPageSet> tags = await GetContextTagsAsync(filter);

                IEnumerable<string> tagNames = from t in tags select t.TagName;

                string taglist = string.Join(",", tagNames);
                tagInput.Text = taglist;
                if (string.IsNullOrEmpty(taglist))
                {
                    filterPopup.IsOpen = true;
                }
                UpdateVisibility();
                RaiseEvent(new TagInputEventArgs(TagInputEvent, this, enterPressed: false));
            }
            catch (Exception ex)
            {
                TraceLogger.Log(TraceCategory.Error(), "Applying preset filter failed {0}", ex);
                TraceLogger.ShowGenericMessageBox(Properties.Resources.TagEditor_Filter_Error, ex);
            }
            finally
            {
                e.Handled = true;
            }
        }
    }
}
