using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// The <i>Tag Editor</i> dialog.
    /// </summary>
    public partial class TagEditor : Window, IOneNotePageWindow<TagEditorModel>
    {
        private TagEditorModel _model;

        /// <summary>
        /// Create a new instance of the tag editor 
        /// </summary>
        public TagEditor()
        {
            InitializeComponent();
        }

        #region IOneNotePageDialog<TagEditorModel>
        /// <summary>
        /// Get or set the view model backing the dialog
        /// </summary>
        public TagEditorModel ViewModel
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

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            // Make sure any tag stuck in the combo box is added too.
 
            AddTagButton_Click(sender, null);
            try
            {
                _model.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.TagEditor_Save_Error, ex), Properties.Resources.TagEditor_ErrorBox_Title);
            }
            if (e != null)
            {
                e.Handled = true;
            }
            DialogResult = true;
        }

        private void AddTagButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TagComboBox.Text))
            {
                foreach (string itm in OneNotePageProxy.ParseTags(TagComboBox.Text))
                {
                    string titlecased = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(itm);
                    if (!_model.PageTags.Contains(titlecased))
                    {
                        _model.PageTags.Add(titlecased);
                    }
                }
               
                TagComboBox.Text = string.Empty;
            }
            if (e != null)
            {
                e.Handled = true;
            }
        }

        private void TagDropDown_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (string.IsNullOrEmpty(TagComboBox.Text))
                {
                    DoneButton_Click(sender, null);
                }
                else
                {
                    AddTagButton_Click(sender, null);
                }
                e.Handled = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TagComboBox.Focus();
            TagComboBox.SelectedItem = null;
            TagComboBox.Text = "";
        }

        private void editTags_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
