﻿// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
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
                        _model.PageTags.RemoveAll(new string[] { mdl.TagName });
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
                _model.PageTags.Clear();
                tagInput.FocusInput();
            }
            e.Handled = true;
        }

        private void TagInputBox_Input(object sender, TagInputEventArgs e) {
            suggestedTags.Notification = String.Empty;
            try {
                if (e.TagInputComplete) {
                    _model.PageTags.AddAll(from t in e.Tags where !_model.PageTags.ContainsKey(t) select new SimpleTagButtonModel(TagFormatter.Format(t)));
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
                    }
                }
                if (sender is TagInputBox) {
                    if (e.TagInputComplete) {
                        suggestedTags.Highlighter = new TextSplitter();
                        tagInput.Clear();
                    } else {
                        suggestedTags.Highlighter = new TextSplitter(e.Tags);
                    }
                    var ctrl = sender as UserControl;
                    if (ctrl != null) {
                        tagInput.FocusInput();
                    }
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
                    // reset the scope to avoid unindended tagging of pages
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
    }
}