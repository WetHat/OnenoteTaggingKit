using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using System.Threading.Tasks;

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


        private void CheckForUnsavedChanges()
        {
            if (!_model.InSync && _model.HasUnsavedChanges)
            {
                Task t = null;
                MessageBoxResult answer = MessageBox.Show(string.Format("You have navigated away from a page you started to tag.\nTap Ok to save changes, or cancel to discard"), "Unsaved Changes", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (answer == MessageBoxResult.OK)
                {
                    _model.SaveChangesAsync();
                }
            }
        }
        private async void AddTagsToPageButton_Click(object sender, RoutedEventArgs e)
        {
            CheckForUnsavedChanges();

            if (!_model.InSync)
            {
                await _model.UpdatePageAsync(false);
            }
            // Make sure any tag stuck in the combo box is added too.

            AddTagsToModel();
            try
            {
                _model.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.TagEditor_Save_Error, ex), Properties.Resources.TagEditor_ErrorBox_Title);
            }
            if (e != null)
            {
                e.Handled = true;
            }
        }

        private async void TagDropDown_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                CheckForUnsavedChanges();
                if (!_model.InSync)
                {
                    await _model.UpdatePageAsync(false);
                }
                if (string.IsNullOrEmpty(tagComboBox.Text))
                {
                    AddTagsToPageButton_Click(sender, null);
                }
                else
                {
                    AddTagsToModel();
                }
                e.Handled = true;
            }
        }

        private void AddTagsToModel()
        {
            if (!string.IsNullOrEmpty(tagComboBox.Text))
            {
                _model.ApplyPageTagsAsync(from t in OneNotePageProxy.ParseTags(tagComboBox.Text) select CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t));
                tagComboBox.Text = string.Empty;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tagComboBox.Focus();
            tagComboBox.SelectedItem = null;
            tagComboBox.Text = "";
            if (_model != null)
            {
                await _model.LoadTagAndPageDatabaseAsync();
                pBar.Visibility = System.Windows.Visibility.Hidden;
                _model.UpdatePageAsync(false);
            }
        }

        private void editTags_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (_model != null)
            {
                pBar.Visibility = System.Windows.Visibility.Visible;
                if (_model.InSync)
                {
                    await _model.UpdatePageAsync(true);
                }
                else
                {
                    CheckForUnsavedChanges();
                    await _model.UpdatePageAsync(false);
                }
                pBar.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void RemoveTagButton_Click(object sender, RoutedEventArgs e)
        {
            SimpleTagButton btn = e.OriginalSource as SimpleTagButton;
            if (btn != null)
            {
                SimpleTagButtonModel mdl = btn.DataContext as SimpleTagButtonModel;
                if (mdl != null)
                {
                    _model.UnapplyPageTagAsync(mdl.TagName);
                }
            }
        }
    }
}
