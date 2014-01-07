using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Interaction logic for TagSelector.xaml
    /// </summary>
    public partial class TagSelector : UserControl
    {
        private TagPageSet _tag;

        /// <summary>
        /// Event fired when tag is selected.
        /// </summary>
        public event RoutedEventHandler Checked;
        /// <summary>
        /// Event firen when tag is deselected
        /// </summary>
        public event RoutedEventHandler UnChecked;

        /// <summary>
        /// dependency property for the number of pages having a tag
        /// </summary>
        public static readonly DependencyProperty PageCountProperty = DependencyProperty.Register("PageCount", typeof(int), typeof(TagSelector));

        /// <summary>
        /// Number of pages having this tag
        /// </summary>
        public int PageCount
        {
            get
            {
                return (int)GetValue(PageCountProperty);
            }
            set
            {
                SetValue(PageCountProperty, value);
            }
        }

        /// <summary>
        /// Dependency property to indicate if tag is selected
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(TagSelector), new PropertyMetadata(OnCheckedChanged));

        /// <summary>
        /// get or set a tags selectes state
        /// </summary>
        public bool IsChecked
        {
            get
            {
                return (bool)GetValue(IsCheckedProperty);
            }
            set
            {
                if (IsChecked != value)
                {
                    SetValue(IsCheckedProperty, value);
                }
            }
        }

        /// <summary>
        /// dependency property for tag's name
        /// </summary>
        public static readonly DependencyProperty TagNameProperty = DependencyProperty.Register("TagName", typeof(string), typeof(TagSelector));
        /// <summary>
        /// Get or set the tag's name
        /// </summary>
        public string TagName
        {
            get
            {
                return (string)GetValue(TagNameProperty);
            }
            set
            {
                SetValue(TagNameProperty, value);
            }
        }

        internal void UpdateTag()
        {
            if (PageCount != _tag.Pages.Count)
            {
                PageCount = _tag.Pages.Count;
            }
        }

        /// <summary>
        /// Create a new instance of the button control
        /// </summary>
        public TagSelector()
        {
            InitializeComponent();
            TagName = "tag";
        }


        /// <summary>
        /// get or set the tag.
        /// </summary>
        public TagPageSet PageTag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
                TagName = _tag.TagName;
                UpdateTag();
            }
        }

        private static void OnCheckedChanged(object sender,DependencyPropertyChangedEventArgs args)
        {
            TagSelector ts = sender as TagSelector;
            if ((bool)args.NewValue)
            {
                if (ts.Checked != null)
                {
                    ts.Checked(ts, new RoutedEventArgs());
                }
            }
            else
            {
                if (ts.UnChecked != null)
                {
                    ts.UnChecked(ts, new RoutedEventArgs());
                }
            }
        }
    }
}
