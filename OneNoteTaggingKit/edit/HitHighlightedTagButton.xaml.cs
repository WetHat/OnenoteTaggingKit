using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// Interaction logic for HitHighlightedTag.xaml
    /// </summary>
    [ComVisible(false)]
    public partial class HitHighlightedTagButton : UserControl
    {
        /// <summary>
        /// Routed event fired in responsse to button clicks or keyboard input.
        /// </summary>
        public static readonly RoutedEvent SingleTagInputEvent = EventManager.RegisterRoutedEvent("SingleTagInput", RoutingStrategy.Bubble, typeof(TagInputEventHandler), typeof(TagInputBox));

        /// <summary>
        /// Event fired in responsse to button clicks or keyboard input.
        /// </summary>
        public event TagInputEventHandler TagInput {
            add { AddHandler(SingleTagInputEvent, value); }
            remove { RemoveHandler(SingleTagInputEvent, value); }
        }
        /// <summary>
        /// Create a new instance of the control
        /// </summary>
        public HitHighlightedTagButton()
        {
            InitializeComponent();
        }

        private void createHitHighlightedTag(HitHighlightedTagButtonModel mdl)
        {
            hithighlightedTag.Inlines.Clear();
            foreach (TextFragment f in mdl.HighlightedTagName)
            {
                Run r = new Run(f.Text);
                if (f.IsMatch)
                {
                    r.Background = Brushes.Yellow;
                }
                hithighlightedTag.Inlines.Add(r);
            }
        }
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HitHighlightedTagButtonModel mdl = DataContext as HitHighlightedTagButtonModel;

            if (mdl != null)
            {
                createHitHighlightedTag(mdl);
                mdl.PropertyChanged += mdl_PropertyChanged;
            }
        }

        void mdl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == HitHighlightedTagButtonModel.HIGHLIGHTED_TAGNAME)
            {
                HitHighlightedTagButtonModel mdl = sender as HitHighlightedTagButtonModel;
                createHitHighlightedTag(mdl);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HitHighlightedTagButtonModel mdl = DataContext as HitHighlightedTagButtonModel;

            var args = new TagInputEventArgs(SingleTagInputEvent, this, new string[] { mdl.TagName }, null);
            args.TagInputComplete = true;
            RaiseEvent(args);
        }

        private void Button_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            HitHighlightedTagButtonModel mdl = DataContext as HitHighlightedTagButtonModel;
            var args = new TagInputEventArgs(SingleTagInputEvent, this, new string[] { mdl.TagName }, e);
            // Only keyboard entries which trigger an action get through
            if (args.Action != TagInputEventArgs.TaggingAction.None) {
                args.TagInputComplete = true;
                RaiseEvent(args);
            }
        }
    }
}
