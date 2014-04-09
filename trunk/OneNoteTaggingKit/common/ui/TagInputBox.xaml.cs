using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        public TagInputBox()
        {
            InitializeComponent();
        }

        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(tagInput.Text);
            }
        }

        public IEnumerable<string> Tags
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

        public bool Focus()
        {
            return tagInput.Focus();
        }

        public void Clear()
        {
            tagInput.Text = String.Empty;
            tagInput.Background=Brushes.Transparent;
            clearTagInput.Visibility = System.Windows.Visibility.Collapsed;
            tagInput.Focus();
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
        }
        private void TagInput_KeyUp(object sender, KeyEventArgs e)
        {
            UpdateVisibility();
            RaiseEvent(new TagInputEventArgs(TagInputEvent, this, e.Key == System.Windows.Input.Key.Enter));
        }

        private void ClearInputButton_Click(object sender, RoutedEventArgs e)
        {
            Tags = new string[0];
            RaiseEvent(new TagInputEventArgs(TagInputEvent, this, false));

        }
    }
}
