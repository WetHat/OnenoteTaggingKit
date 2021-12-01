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

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// Capture input of one or more tags separated by comma ','
    /// </summary>
    [ComVisible(false)]
    public partial class TagInputBox : UserControl
    {
        /// <summary>
        /// Routed event fired for changes to the <see cref="Tags" /> property
        /// </summary>
        public static readonly RoutedEvent TagInputEvent = EventManager.RegisterRoutedEvent("TagInput", RoutingStrategy.Bubble, typeof(TagInputEventHandler), typeof(TagInputBox));

        /// <summary>
        /// Routed event fired for changes to the <see cref="Tags" /> property
        /// </summary>
        public event TagInputEventHandler TagInput
        {
            add { AddHandler(TagInputEvent, value); }
            remove { RemoveHandler(TagInputEvent, value); }
        }

        /// <summary>
        /// Dependency property for the context tags source
        /// </summary>
        public static readonly DependencyProperty ContextTagsSourceProperty = DependencyProperty.Register("ContextTagsSource", typeof(TagsAndPages), typeof(TagInputBox), new PropertyMetadata(OnContextTagSourceChanged));

        private static void OnContextTagSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TagInputBox ib = d as TagInputBox;
            if (ib != null)
            {
                ib.presetsMenu.Visibility = e.NewValue != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Dependency property for the handling of mapped page tags.
        /// </summary>
        public static readonly DependencyProperty IncludeMappedTagsProperty = DependencyProperty.Register("IncludeMappedTags", typeof(bool), typeof(TagInputBox),new PropertyMetadata(false));

        /// <summary>
        /// Get or set a collection which provides tags from a OneNote context.
        /// </summary>
        public TagsAndPages ContextTagsSource {
            get {
                return GetValue(ContextTagsSourceProperty) as TagsAndPages;
            }
            set {
                SetValue(ContextTagsSourceProperty, value);
            }
        }

        public bool IncludeMappedTags {
            get {
                return (bool)GetValue(IncludeMappedTagsProperty);
            }
            set {
                SetValue(IncludeMappedTagsProperty, value);
            }
        }

        DateTime _lastInput;
        void InputTimer_Tick(object sender, EventArgs e) {
            if ((DateTime.Now - _lastInput) > _inputTimer.Interval) {
                _inputTimer.Stop();
                RaiseEvent(new TagInputEventArgs(TagInputEvent, this, Tags, null));
            }
        }

        private System.Windows.Threading.DispatcherTimer _inputTimer;
        /// <summary>
        /// Create a new instance of a input box for tag names.
        /// </summary>
        public TagInputBox()
        {
            InitializeComponent();
            _inputTimer = new System.Windows.Threading.DispatcherTimer();
            _inputTimer.Interval = new TimeSpan(0, 0, 0, 0, 400);
            _inputTimer.Tick += new EventHandler(InputTimer_Tick);
        }

        /// <summary>
        /// Check if the tag input is empty
        /// </summary>
        /// <value>true if no input is available; false otherwise</value>
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
                return  OneNotePageProxy.ParseTags(tagInput.Text);
            }
            set
            {
                tagInput.Text = string.Join(",", value);
                UpdateVisibility();
                RaiseEvent(new TagInputEventArgs(TagInputEvent, this, value, null));
            }
        }

        internal bool FocusInput()
        {
            return tagInput.Focus();
        }

        internal void Clear()
        {
            tagInput.Text = String.Empty;
            _inputTimer.Stop();
            var e = new TagInputEventArgs(TagInputEvent, this, Tags, null);
            e.TagInputComplete = true;
            e.Action = TagInputEventArgs.TaggingAction.Clear;
            RaiseEvent(e);
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
            TagInputEventArgs evt = new TagInputEventArgs(TagInputEvent, this, Tags, e);

            if (e.Key == Key.Escape) {
                tagInput.Text = String.Empty;
            }
            UpdateVisibility();

            if (evt.TagInputComplete) {
                // raise event immediately
                _inputTimer.Stop();
                RaiseEvent(evt);
            } else {
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

        private Task<IEnumerable<TagPageSet>> GetContextTagsAsync(TagContext filter)
        {
            TagsAndPages tagSource = ContextTagsSource; // must be assigned here to avoid access from another thread
            return Task<IEnumerable<TagPageSet>>.Run(() => { return GetContextTagsAction(filter, tagSource); });
        }

        private IEnumerable<TagPageSet> GetContextTagsAction(TagContext filter, TagsAndPages tagSource)
        {
            tagSource.LoadPageTags(filter);
            return tagSource.Tags.Values;
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

                TagContext filter = (TagContext)Enum.Parse(typeof(TagContext), itm.Tag.ToString());
                IEnumerable<TagPageSet> tags = await GetContextTagsAsync(filter);
                if (IncludeMappedTags) {
                    Tags = from t in tags select t.TagName;
                }
                else {
                    Tags = from t in tags
                           where !t.IsImported
                           select t.TagName;
                }

                if (string.IsNullOrEmpty(tagInput.Text))
                {
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