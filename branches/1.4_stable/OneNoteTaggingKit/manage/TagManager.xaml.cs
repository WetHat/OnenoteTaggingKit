using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    /// Interaction logic for TagManager.xaml
    /// </summary>
    /// <remarks>Implements a tag management dialog</remarks>
    public partial class TagManager : Window, IOneNotePageWindow<TagManagerModel>
    {
        private TagManagerModel _model;

        /// <summary>
        /// Create a new instance of a tag management dialog.
        /// </summary>
        public TagManager()
        {
            InitializeComponent();
        }

        private void NewTagButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(newTag.Text))
            {
                foreach (string tag in OneNotePageProxy.ParseTags(newTag.Text))
                {
                    string titlecased = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tag);
                    if (!_model.SuggestedTags.Contains(titlecased))
                    {
                        _model.SuggestedTags.Add(titlecased);
                    }
                }
                newTag.Text = String.Empty;
            }
            if (e != null)
            {
                e.Handled = true;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _model.SaveChanges();
            e.Handled = true;
            DialogResult = true;
        }

        private void newTag_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                NewTagButton_Click(sender, null);
            }
            e.Handled = true;
        }

        #region IOneNotePageDialog<TagManagerModel>
        /// <summary>
        /// Get or set the dialog's view model.
        /// </summary>
        public TagManagerModel ViewModel
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
                DataContext = _model;
            }
        }
        #endregion

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;

            string navigateUri = hl.NavigateUri.ToString();

            Process.Start(new ProcessStartInfo(navigateUri));  

            e.Handled = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, _model.TagList);
        }
    }
}
