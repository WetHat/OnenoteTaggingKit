﻿using System;
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

        private void AddTagsToPageButton_Click(object sender, RoutedEventArgs e)
        {
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

        private void TagDropDown_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
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
                _model.ApplyPageTags(from t in OneNotePageProxy.ParseTags(tagComboBox.Text) select CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t));
                tagComboBox.Text = string.Empty;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tagComboBox.Focus();
            tagComboBox.SelectedItem = null;
            tagComboBox.Text = "";
            if (_model != null)
            {
                Task t = _model.LoadTagAndPageDatabaseAsync();
                t.ContinueWith((tsk) => {
                    pBar.Visibility = System.Windows.Visibility.Hidden;
                    _model.UpdatePageAsync(false);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void editTags_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (_model != null)
            {
                pBar.Visibility = System.Windows.Visibility.Visible;
                Task t = _model.UpdatePageAsync(true);
                t.ContinueWith((tsk) => { pBar.Visibility = System.Windows.Visibility.Hidden; }, TaskScheduler.FromCurrentSynchronizationContext());
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
                    _model.UnapplyPageTag(mdl.TagName);
                }
            }
        }
    }
}
