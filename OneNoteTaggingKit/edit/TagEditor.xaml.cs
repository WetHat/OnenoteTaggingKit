// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.Tagger;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// The <i>Tag Editor</i> dialog.
    /// </summary>
    [ComVisible(false)]
    public partial class TagEditor : Window, IOneNotePageWindow<TagEditorModel>
    {
        private TagEditorModel _model;

            /// <summary>
            /// Create a new instance of the tag editor
            /// </summary>
        public TagEditor() {
            InitializeComponent();
        }

        #region IOneNotePageDialog<TagEditorModel>

        /// <summary>
        /// Get or set the view model backing the dialog
        /// </summary>
        public TagEditorModel ViewModel {
            get {
                return _model;
            }
            set {
                _model = value;
                DataContext = _model;
            }
        }

        #endregion IOneNotePageDialog<TagEditorModel>

        private void AddTagsToPageButton_Click(object sender, RoutedEventArgs e) {
            if (_model != null && _model.SelectedTags.Count > 0) {
                ApplyPageTags(TagOperation.UNITE);
                if (e != null) {
                    e.Handled = true;
                }
            } else {
                MessageBox.Show(Properties.Resources.TagEditor_NoTagsSelectedWarning, Properties.Resources.TagEditor_WarningMessageBox_Title, MessageBoxButton.OK);
            }
        }

        private void SetPageTagsButton_Click(object sender, RoutedEventArgs e) {
            ApplyPageTags(TagOperation.REPLACE);
            if (e != null) {
                e.Handled = true;
            }
        }

        private void RemoveTagsFromPageButton_Click(object sender, RoutedEventArgs e) {
            if (_model != null && _model.SelectedTags.Count > 0) {
                ApplyPageTags(TagOperation.SUBTRACT);
                if (e != null) {
                    e.Handled = true;
                }
            } else {
                MessageBox.Show(Properties.Resources.TagEditor_NoTagsSelectedWarning, Properties.Resources.TagEditor_WarningMessageBox_Title, MessageBoxButton.OK);
            }
        }
        private void ResyncTagsFromPageButton_Click(object sender, RoutedEventArgs e) {
            if (_model != null) {
                ApplyPageTags(TagOperation.RESYNC);
                if (e != null) {
                    e.Handled = true;
                }
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e) {
            tagInput.FocusInput();
            if (_model != null) {
                await _model.TagSuggestions.LoadKnownTagsAsync().ContinueWith((x) => { pBar.Visibility = System.Windows.Visibility.Hidden; }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void editTags_Closing(object sender, CancelEventArgs e) {
            if (_model != null) {
                _model.Dispose();
                _model = null;
            }
        }

        private void ClearTagsButton_Click(object sender, RoutedEventArgs e) {
            if (_model != null) {
                // suggest all tags again
                foreach (var mdl in from SelectableTagModel ts in _model.TagSuggestions
                                    where ts.IsSelected
                                    select ts) {
                    mdl.IsSelected = false;
                }
                _model.SelectedTags.Clear();
                tagInput.FocusInput();
            }
            e.Handled = true;
        }

        private void TagInputBox_Input(object sender, TagInputEventArgs e) {
            suggestedTags.Notification = String.Empty;
            _model.TagSuggestions.Highlighter = tagInput.IsEmpty ? new TextSplitter() : new TextSplitter(e.Tags);

            try {
                if (e.TagInputComplete) {
                    selectMatchingTags();
                    var tagset = new PageTagSet(e.Tags,(TagFormat)Properties.Settings.Default.TagFormatting);
                    // create new tags
                    _model.SelectedTags.AddAll(from pt in tagset
                                            where !_model.SelectedTags.ContainsSortKey(pt.Key)
                                            select new SelectedTagModel() {
                                                Tag = pt
                                            });
                    switch (e.Action) {
                        case TagInputEventArgs.TaggingAction.Add:
                            AddTagsToPageButton_Click(e.Source, e);
                            break;

                        case TagInputEventArgs.TaggingAction.Set:
                            SetPageTagsButton_Click(e.Source, e);
                            break;

                        case TagInputEventArgs.TaggingAction.Remove:
                            RemoveTagsFromPageButton_Click(e.Source, e);
                            break;
                        case TagInputEventArgs.TaggingAction.Clear:
                            _model.TagSuggestions.Highlighter = new TextSplitter();
                            ClearTagsButton_Click(e.Source, e);
                            break;
                    }
                }
                if (sender is TagInputBox) {
                    tagInput.FocusInput();
                }
            } catch (Exception ex) {
                TraceLogger.Log(TraceCategory.Error(), "Processing Tag input failed with {0}", ex);
                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagEditor_Input_Error, ex);
            }
            e.Handled = true;
        }

        private void ApplyPageTags(TagOperation op) {
            tagInput.FocusInput();
            try {
                _model.Scope = ((TaggingScopeDescriptor)taggingScope.SelectedItem).Scope;

                int pagesTagged = _model.EnqueuePagesForTagging(op);
                if (_model.ScopesEnabled) {
                    // reset the scope to avoid unintended tagging of pages
                    taggingScope.SelectedIndex = 0;
                }
                tagInput.Clear();
                _model.TagSuggestions.Highlighter = new TextSplitter();
                suggestedTags.Notification = pagesTagged == 0 ? Properties.Resources.TagEditor_Popup_NothingTagged : string.Format(Properties.Resources.TagEditor_Popup_TaggingInProgress, pagesTagged);
            } catch (Exception xe) {
                TraceLogger.Log(TraceCategory.Error(), "Applying tags to page failed: {0}", xe);
                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagEditor_TagUpdate_Error, xe);
            }
        }

        void selectMatchingTags() {
            _model.SelectedTags.AddAll(from t in _model.TagSuggestions.Values
                                       where t.IsFullMatch && !t.IsSelected
                                       select new SelectedTagModel() {
                                            SelectableTag = t
                                       });
        }
        private void SelectMatchingTagsButton_Click(object sender, RoutedEventArgs e) {
            selectMatchingTags();
        }

        private void SelectableTag_TagSelected(object sender, RoutedEventArgs e) {
            var btn = sender as SelectableTag;
            if (btn.DataContext is SelectableTagModel mdl) {
                if (mdl.IsSelected && !_model.SelectedTags.ContainsKey(mdl.TagName)) {
                    _model.SelectedTags.AddAll(new SelectedTagModel[] {
                        new SelectedTagModel() {
                            SelectableTag = mdl
                        }
                    });
                }
            }
        }

        private void SelectedTag_TagClick(object sender, RoutedEventArgs e) {
            if (_model != null) {
                tagInput.FocusInput();
                if (e.OriginalSource is Tag tagBtn
                    && tagBtn.DataContext is SelectedTagModel mdl) {
                    if (mdl.SelectableTag != null) {
                        // de-select that tag
                        mdl.SelectableTag.IsSelected = false;
                    }
                    _model.SelectedTags.RemoveAll(new string[] { mdl.Key });
                }
            }
        }
    }
}