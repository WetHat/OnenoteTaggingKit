// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// An input control for a comma separated list of tag names.
    /// </summary>
    [ComVisible(false)]
    public partial class TagInputBox : UserControl
    {
        /// <summary>
        /// Routed event fired for changes to the <see cref="TagNames" /> property
        /// </summary>
        public static readonly RoutedEvent TagInputEvent = EventManager.RegisterRoutedEvent("TagInput", RoutingStrategy.Bubble, typeof(TagInputEventHandler), typeof(TagInputBox));

        /// <summary>
        /// Routed event fired for changes to the <see cref="TagNames" /> property
        /// </summary>
        public event TagInputEventHandler TagInput {
            add { AddHandler(TagInputEvent, value); }
            remove { RemoveHandler(TagInputEvent, value); }
        }
        #region ContextTagsSourceProperty
        /// <summary>
        /// Backing store for the context tags source property <see cref="ContextTagsSource"/>
        /// </summary>
        public static readonly DependencyProperty ContextTagsSourceProperty = DependencyProperty.Register(
            nameof(ContextTagsSource),
            typeof(TagsAndPages),
            typeof(TagInputBox),
            new PropertyMetadata(OnContextTagSourceChanged));

        /// <summary>
        ///     Get or set the source for tag presets.
        /// </summary>
        /// <remarks>
        ///     The tag source can be updated by external components in order
        ///     to preset tags.
        /// </remarks>
        public TagsAndPages ContextTagsSource {
            get => GetValue(ContextTagsSourceProperty) as TagsAndPages;
            set => SetValue(ContextTagsSourceProperty, value);
        }
        private static void OnContextTagSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TagInputBox ib && e.NewValue != null) {
                ib.presetsMenu.Visibility = e.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
                var oldSource = e.OldValue as TagsAndPages;
                var newSource = e.NewValue as TagsAndPages;
                if (oldSource != null) {
                    oldSource.Pages.CollectionChanged -= ib.Pages_CollectionChanged;
                }
                if (newSource != null) {
                    newSource.Pages.CollectionChanged += ib.Pages_CollectionChanged;
                }
            }
        }

        /// <summary>
        ///     Inject presets if the context was updated.
        /// </summary>
        /// <param name="sender">The pages collection which raised the event.</param>
        /// <param name="e">Change details.</param>
        private void Pages_CollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, PageNode> e) {
            // inject the tags from the context into the input box
            Dispatcher.Invoke(() => {
                TagNames = from tps in ContextTagsSource.Tags.Values select tps.TagName;
                IsPreset = true;
            });
        }

        #endregion ContextTagsSourceProperty
        #region IncludeMappedTagsProperty
        /// <summary>
        /// Backing store for the <see cref="IncludeMappedTags"/> property.
        /// </summary>
        public static readonly DependencyProperty IncludeMappedTagsProperty = DependencyProperty.Register(
            nameof(IncludeMappedTags),
            typeof(bool),
            typeof(TagInputBox),
            new PropertyMetadata(false));

        /// <summary>
        /// Get/set a flag to include or exclude mapped (imported) page tags
        /// found in the <see cref="ContextTagsSource"/>.
        /// </summary>
        public bool IncludeMappedTags {
            get => (bool)GetValue(IncludeMappedTagsProperty);
            set => SetValue(IncludeMappedTagsProperty, value);
        }
        #endregion IncludeMappedTagsProperty

        DateTime _lastInput;
        void InputTimer_Tick(object sender, EventArgs e) {
            if ((DateTime.Now - _lastInput) > _inputTimer.Interval) {
                _inputTimer.Stop();
                RaiseEvent(new TagInputEventArgs(TagInputEvent, this, TagNames, null));
                tagInput.Focus();
            }
        }

        private System.Windows.Threading.DispatcherTimer _inputTimer;
        /// <summary>
        /// Create a new instance of a input box for tag names.
        /// </summary>
        public TagInputBox() {
            InitializeComponent();
            _inputTimer = new System.Windows.Threading.DispatcherTimer();
            _inputTimer.Interval = new TimeSpan(0, 0, 0, 0, 400);
            _inputTimer.Tick += new EventHandler(InputTimer_Tick);
        }

        /// <summary>
        /// Check if the tag input is empty
        /// </summary>
        /// <value>true if no input is available; false otherwise</value>
        public bool IsEmpty => string.IsNullOrWhiteSpace(tagInput.Text);
        /// <summary>
        /// Get/set the list of tag names.
        /// </summary>
        /// <remarks>
        ///     The tag names are displayed as comma separated list in the
        ///     input box.
        /// </remarks>
        public IEnumerable<string> TagNames {
            get => PageTagSet.SplitTaglist(tagInput.Text);
            private set
            {
                tagInput.Text = string.Join(",", value);
                UpdateVisibility();
                RaiseEvent(new TagInputEventArgs(TagInputEvent, this, value, null));
                tagInput.Focus();
            }
        }

        /// <summary>
        ///  Get a flag indicating whether the <see cref="TagNames"/> property
        ///  contains a preset.
        /// </summary>
        public bool IsPreset { get; private set; }
        /// <summary>
        /// Set focus on the tag input box.
        /// </summary>
        /// <remarks>`true` if focus was changed.</remarks>
        public bool FocusInput() => tagInput.Focus();

        /// <summary>
        /// Clear any tag input and raise the TagInput event.
        /// </summary>
        internal void Clear() {
            tagInput.Text = String.Empty;
            _inputTimer.Stop();
            var e = new TagInputEventArgs(TagInputEvent, this, TagNames, null);
            e.TagInputComplete = true;
            IsPreset = false;
            RaiseEvent(e);
            UpdateVisibility();
        }

        private void UpdateVisibility() {
            if (string.IsNullOrEmpty(tagInput.Text)) {
                tagInput.Background = Brushes.Transparent;
                clearTagInput.Visibility = Visibility.Collapsed;
            } else {
                tagInput.Background = Brushes.White;
                clearTagInput.Visibility = Visibility.Visible;
            }
            tagInput.Focus();
        }

        private void TagInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) {
                tagInput.Text = String.Empty;
                IsPreset = false;
            }
            TagInputEventArgs evt = new TagInputEventArgs(TagInputEvent, this, TagNames, e);

            UpdateVisibility();

            if (evt.TagInputComplete) {
                // raise event immediately
                _inputTimer.Stop();
                RaiseEvent(evt);
                tagInput.Focus(); // take focus back
            } else {
                IsPreset = false;
                // wait for more input
                _lastInput = DateTime.Now;
                if  (!_inputTimer.IsEnabled) {
                    _inputTimer.Start();
                }
            }
            e.Handled = true;
        }

        private void ClearInputButton_Click(object sender, RoutedEventArgs e)
        {
            Clear();
            IsPreset = false;
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

        private void Filter_MenuItem_Click(object sender, RoutedEventArgs e) {
            try {
                if (ContextTagsSource == null) {
                    return;
                }

                MenuItem itm = sender as MenuItem;
                TagContext context = (TagContext)Enum.Parse(typeof(TagContext), itm.Tag.ToString());
                ContextTagsSource.LoadPageTags(context);
                if (IncludeMappedTags) {
                    TagNames = from t in ContextTagsSource.Tags.Values select t.TagName;
                } else {
                    TagNames = from t in ContextTagsSource.Tags.Values
                               where !t.Tag.IsImported
                               select t.TagName;
                }

                if (string.IsNullOrEmpty(tagInput.Text)) {
                    filterPopup.IsOpen = true;
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log(TraceCategory.Error(), "Applying preset filter failed {0}", ex);
                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagEditor_Filter_Error, ex);
            }
            finally
            {
                e.Handled = true;
            }
        }
    }
}