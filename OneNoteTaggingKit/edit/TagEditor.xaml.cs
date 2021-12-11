// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.Tagger;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    internal delegate HitHighlightedTagButton TagButtonFactory(object dataContext);

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
                _model.TagSuggestions = new SuggestedTagsSource<HitHighlightedTagButtonModel>();
                DataContext = _model;
            }
        }

        #endregion IOneNotePageDialog<TagEditorModel>

        private void RemovePageTagButton_Click(object sender, RoutedEventArgs e) {
            if (_model != null) {
                tagInput.FocusInput();
                SimpleTagButton tagBtn = e.OriginalSource as SimpleTagButton;
                if (tagBtn != null) {
                    SimpleTagButtonModel mdl = tagBtn.DataContext as SimpleTagButtonModel;
                    if (mdl != null) {
                        var names = new string[] { mdl.TagName };
                        _model.PageTags.RemoveAll(names);
                        // suggest that tag again
                        HitHighlightedTagButtonModel tbm;
                        if (_model.TagSuggestions.TryGetValue(mdl.TagName, out tbm)) {
                            tbm.IsSuggested = true;
                        }
                    }
                }
            }
        }

        private void AddTagsToPageButton_Click(object sender, RoutedEventArgs e) {
            if (_model != null && _model.PageTags.Count > 0) {
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
            if (_model != null && _model.PageTags.Count > 0) {
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
                await _model.TagSuggestions.LoadSuggestedTagsAsync().ContinueWith((x) => { pBar.Visibility = System.Windows.Visibility.Hidden; }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void editTags_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Properties.Settings.Default.Save();
            Trace.Flush();
        }

        private void ClearTagsButton_Click(object sender, RoutedEventArgs e) {
            if (_model != null) {
                // suggest all tags again
                foreach (var mdl in from ts in _model.TagSuggestions
                                    where !ts.IsSuggested
                                    select ts) {
                    mdl.IsSuggested = true;
                }
                _model.PageTags.Clear();
                tagInput.FocusInput();
            }
            e.Handled = true;
        }

        private void TagInputBox_Input(object sender, TagInputEventArgs e) {
            suggestedTags.Notification = String.Empty;
            try {
                if (e.TagInputComplete) {
                    selectMatchingTags();
                    // create new tags
                     _model.PageTags.AddAll(from t in e.Tags
                                            where !_model.PageTags.ContainsSortKey(new TagModelKey(t))
                                            select new SimpleTagButtonModel(TagFormatter.Format(t)));
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
                            suggestedTags.Highlighter = new TextSplitter();
                            ClearTagsButton_Click(e.Source, e);
                            break;
                        default:
                            if (tagInput.IsEmpty) {
                                suggestedTags.Highlighter = new TextSplitter();
                            }
                            break;
                    }
                } else {
                    suggestedTags.Highlighter = new TextSplitter(e.Tags);
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
                suggestedTags.Highlighter = new TextSplitter();
                suggestedTags.Notification = pagesTagged == 0 ? Properties.Resources.TagEditor_Popup_NothingTagged : string.Format(Properties.Resources.TagEditor_Popup_TaggingInProgress, pagesTagged);
            } catch (Exception xe) {
                TraceLogger.Log(TraceCategory.Error(), "Applying tags to page failed: {0}", xe);
                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagEditor_TagUpdate_Error, xe);
            }
        }

        void selectMatchingTags() {
            SimpleTagButtonModel _select_tag(HitHighlightedTagButtonModel mdl) {
                mdl.IsSuggested = false;
                return new SimpleTagButtonModel(mdl.TagName);
            }
            _model.PageTags.AddAll(from t in _model.TagSuggestions.Values
                                   where t.IsFullMatch && t.IsSuggested
                                   select _select_tag(t));
        }
        private void SelectMatchingTagsButton_Click(object sender, RoutedEventArgs e) {
            selectMatchingTags();
        }
    }
}