using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

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
            await ApplyPageTagsAsync(TagOperation.UNITE);
            if (e != null)
            {
                e.Handled = true;
            }
        }

        private async void SetPageTagsButton_Click(object sender, RoutedEventArgs e)
        {
            await ApplyPageTagsAsync(TagOperation.REPLACE);
            if (e != null)
            {
                e.Handled = true;
            }
        }

        private async void RemoveTagsFromPageButton_Click(object sender, RoutedEventArgs e)
        {
            await ApplyPageTagsAsync(TagOperation.SUBTRACT);
            if (e != null)
            {
                e.Handled = true;
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tagInput.Focus();
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

            tagInput.Tags = tagNames;

            string filterText = string.Join(",", tagNames);

            if (string.IsNullOrEmpty(filterText))
            {
                //UpdateTagFilter(true);
                _model.UpdateTagFilter(null);
                filterPopup.IsOpen = true;
            }
            else
            {
                _model.UpdateTagFilter(tagNames);
            }
        }

        private void TagInputBox_Input(object sender, TagInputEventArgs e)
        {
            pagesTaggedPopup.IsOpen = false;
            filterPopup.IsOpen = false;
            if (tagInput.IsEmpty)
            {
                _model.UpdateTagFilter(null);
            }
            else
            {
                IEnumerable<string> tags = tagInput.Tags;
                if (e.TagInputComplete)
                {
                    _model.SuggestedTags.AddAll(from t in tags where !_model.SuggestedTags.ContainsKey(t) select new HitHighlightedTagButtonModel(t));
                    _model.PageTags.AddAll(from t in tags where !_model.PageTags.ContainsKey(t) select new SimpleTagButtonModel(t));
                }
                _model.UpdateTagFilter(tags);
            }
            e.Handled = true;
        }

        private async Task ApplyPageTagsAsync(TagOperation op)
        {
            tagInput.Focus();
            try
            {
                TaggingScope scope = ((TaggingScopeDescriptor)taggingScope.SelectedItem).Scope;
                if (scope == TaggingScope.CurrentSection)
                {
                    taggingScope.SelectedIndex = 0;
                }
                int pagesTagged = await _model.SaveChangesAsync(TagOperation.REPLACE, scope);
                pagesTaggedText.Text = pagesTagged == 0 ? Properties.Resources.TagEditor_Popup_NothingTagged : string.Format(Properties.Resources.TagEditor_Popup_PagesTagged, pagesTagged);

                pagesTaggedPopup.IsOpen = true;
            }
            catch (Exception xe)
            {
                MessageBox.Show(string.Format(Properties.Resources.TagEditor_ErrorMessage_TaggingException, xe), Properties.Resources.TagEditor_ErrorMessageBox_Title);
            }

        }
    }
}
