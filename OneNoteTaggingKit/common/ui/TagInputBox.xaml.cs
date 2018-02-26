// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Event details for the <see cref="E:TagInputBox.TagInput" /> event
    /// </summary>
    [ComVisible(false)]
    public class TagInputEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Determine if tag input is complete
        /// </summary>
        /// <value>true if tag input is complete; false if tag input is still in progress</value>
        public bool TagInputComplete { get; private set; }

        /// <summary>
        /// Create a new instance of the event details
        /// </summary>
        /// <param name="routedEvent"> routed event which fired</param>
        /// <param name="source">      object which fired the event</param>
        /// <param name="enterPressed">true if tag input is complete; false otherwise</param>
        internal TagInputEventArgs(RoutedEvent routedEvent, object source, bool enterPressed)
            : base(routedEvent, source)
        {
            TagInputComplete = enterPressed;
        }
    }

    /// <summary>
    /// handlers for the <see cref="E:TagInputBox.TagInput" /> event
    /// </summary>
    /// <param name="sender">object emitting the event</param>
    /// <param name="e">     event details</param>
    public delegate void TagInputEventHandler(object sender, TagInputEventArgs e);

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
        /// Get or set a collection which provides tags from a OneNote context.
        /// </summary>
        public TagsAndPages ContextTagsSource
        {
            get
            {
                return GetValue(ContextTagsSourceProperty) as TagsAndPages;
            }
            set
            {
                SetValue(ContextTagsSourceProperty, value);
            }
        }

        /// <summary>
        /// Create a new instance of a input box for tag names.
        /// </summary>
        public TagInputBox()
        {
            InitializeComponent();
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
                return from t in OneNotePageProxy.ParseTags(tagInput.Text) select t;
            }
            set
            {
                tagInput.Text = string.Join(",", value);
                UpdateVisibility();
                RaiseEvent(new TagInputEventArgs(TagInputEvent, this, enterPressed: false));
            }
        }

        internal bool FocusInput()
        {
            return tagInput.Focus();
        }

        internal void Clear()
        {
            tagInput.Text = String.Empty;
            RaiseEvent(new TagInputEventArgs(TagInputEvent, this, enterPressed: false));
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

            if (filter == TagContext.SelectedNotes)
            {
                HashSet<TagPageSet> tags = new HashSet<TagPageSet>();
                foreach (var p in (from pg in tagSource.Pages where pg.Value.IsSelected select pg.Value))
                {
                    tags.UnionWith(p.Tags);
                }
                return tags;
            }

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

                Tags = from t in tags select t.TagName;
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