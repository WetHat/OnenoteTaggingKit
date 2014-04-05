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
using System.Threading;

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
                 _model.SuggestedTags.CollectionChanged += OnSuggestedTagsChanged;
            }
        }

        #endregion

        private void OnSuggestedTagsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        HitHighlightedTagButton tag = new HitHighlightedTagButton()
                        {
                            DataContext = e.NewItems[i] 
                        };
                        tag.Click += OnSuggestedTagClick;

                        suggestedTagsPanel.Children.Insert(i + e.NewStartingIndex, tag);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        suggestedTagsPanel.Children.RemoveAt(e.OldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    suggestedTagsPanel.Children.Clear();
                    break;
            }
        }

        private void OnSuggestedTagClick(object sender, RoutedEventArgs e)
        {
            tagInput.Focus();
            HitHighlightedTagButton btn = sender as HitHighlightedTagButton;
            if (btn != null)
            {
                IHitHighlightedTagButtonModel mdl = btn.DataContext as IHitHighlightedTagButtonModel;
                if (mdl != null)
                {
                    _model.PageTags.AddAll(new SimpleTagButtonModel[] { new SimpleTagButtonModel(mdl.TagName) });
                }
            }
        }

        private void RemovePageTagButton_Click(object sender, RoutedEventArgs e)
        {
            if (_model != null)
            {
                tagInput.Focus();
                SimpleTagButton tagBtn = e.OriginalSource as SimpleTagButton;
                if (tagBtn != null)
                {
                    SimpleTagButtonModel mdl = tagBtn.DataContext as SimpleTagButtonModel;
                    if (mdl != null)
                    {
                        _model.PageTags.RemoveAll(new string[] { mdl.TagName});
                    }
                }
            }
        }

        private async void AddTagsToPageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tagInput.Focus();
                _model.SaveChangesAsync(TagOperation.UNITE);
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

        private void RemoveTagsFromPageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tagInput.Focus();
                _model.SaveChangesAsync(TagOperation.SUBTRACT);
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

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tagInput.Focus();
            tagInput.Text = String.Empty;
            if (_model != null)
            {
                 await _model.LoadSuggestedTagsAsync();
                pBar.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void editTags_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.KnownTags = string.Join(",", from t in _model.SuggestedTags.Values select t.TagName);
            Properties.Settings.Default.Save();
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            tagInput.Text = String.Empty;
            tagInput.Focus();

            _model.UpdateTagFilter(null);
            clearFilter.Visibility = System.Windows.Visibility.Hidden;
        }

        private void TagInput_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                IEnumerable<string> tags = from t in OneNotePageProxy.ParseTags(tagInput.Text) select CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t);
                _model.SuggestedTags.AddAll(from t in tags where !_model.SuggestedTags.ContainsKey(t) select new HitHighlightedTagButtonModel(t));
                _model.PageTags.AddAll(from t in tags where !_model.PageTags.ContainsKey(t) select new SimpleTagButtonModel(t));
                tagInput.Text = String.Empty;
                _model.UpdateTagFilter(null);

            }
            else if (tagInput.Text.Length > 0)
            {
                clearFilter.Visibility = System.Windows.Visibility.Visible;
                _model.UpdateTagFilter(OneNotePageProxy.ParseTags(tagInput.Text));
            }
            else
            {
                clearFilter.Visibility = System.Windows.Visibility.Hidden;
                _model.UpdateTagFilter(null);
            }
            e.Handled = true;
        }

        private void ClearTagsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_model != null)
            {
                _model.PageTags.Clear();
                tagInput.Focus();
            }
        }
    }
}
