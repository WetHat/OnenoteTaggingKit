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
using WetHatLab.OneNote.TaggingKit.common;

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

        private void UpdateTagFilter(bool clear)
        {
            bool currentlyEmpty = tagInputDefaultMessage.Visibility == System.Windows.Visibility.Visible;
            if (clear)
            {
                if (!currentlyEmpty)
                {
                    tagInputDefaultMessage.Visibility = System.Windows.Visibility.Visible;
                    tagInput.Text = string.Empty;                   
                    clearFilter.Visibility = System.Windows.Visibility.Hidden;
                    _model.UpdateTagFilter(null);
                    //filterPreset.SelectedIndex = 0;
                }
            }
            else
            {
                if (currentlyEmpty)
                {
                    tagInputDefaultMessage.Visibility = System.Windows.Visibility.Hidden;
                    clearFilter.Visibility = System.Windows.Visibility.Visible;
                }
                IEnumerable<string> tags = from t in OneNotePageProxy.ParseTags(tagInput.Text) select CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t);
                _model.UpdateTagFilter(tags);
            }
            tagInput.Focus();
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
                _model.LoadSuggestedTagsAsync().ContinueWith((x) => { pBar.Visibility = System.Windows.Visibility.Hidden; }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void editTags_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.KnownTags = string.Join(",", from t in _model.SuggestedTags.Values select t.TagName);
            Properties.Settings.Default.Save();
        }

        private void ClearFilterButton_Click(object sender, RoutedEventArgs e)
        {
           UpdateTagFilter(true);
        }

        private void TagInput_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (!string.IsNullOrEmpty(tagInput.Text))
                {
                    IEnumerable<string> tags = from t in OneNotePageProxy.ParseTags(tagInput.Text) select CultureInfo.CurrentCulture.TextInfo.ToTitleCase(t);
                    _model.SuggestedTags.AddAll(from t in tags where !_model.SuggestedTags.ContainsKey(t) select new HitHighlightedTagButtonModel(t));
                    _model.PageTags.AddAll(from t in tags where !_model.PageTags.ContainsKey(t) select new SimpleTagButtonModel(t));
                }
                UpdateTagFilter(true);
            }
            else
            {
                UpdateTagFilter(string.IsNullOrEmpty(tagInput.Text));
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
            e.Handled = true;
        }

        private async void Filter_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem itm = sender as MenuItem;

            PresetFilter filter = (PresetFilter)Enum.Parse(typeof(PresetFilter), itm.Tag.ToString());
            IEnumerable<TagPageSet> tags = await _model.GetContextTagsAsync(filter);

            IEnumerable<string> tagNames = from t in tags select t.TagName;

            string filterText = string.Join(",", tagNames);
            if (string.IsNullOrEmpty(filterText))
            {
                UpdateTagFilter(true);
            }
            else
            {
                tagInput.Text = filterText;
                UpdateTagFilter(false);
            }
        }
    }
}
