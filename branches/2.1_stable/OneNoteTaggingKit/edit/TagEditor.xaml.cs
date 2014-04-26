using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    internal delegate HitHighlightedTagButton TagButtonFactory(object dataContext);

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
                _model.TagSuggestions = new SuggestedTagsSource<HitHighlightedTagButtonModel>();
                DataContext = _model;
            }
        }
        #endregion

        private void OnSuggestedTagClick(object sender, RoutedEventArgs e)
        {
            tagInput.FocusInput();
            HitHighlightedTagButton btn = sender as HitHighlightedTagButton;
            if (btn != null)
            {
                HitHighlightedTagButtonModel mdl = btn.DataContext as HitHighlightedTagButtonModel;
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
                tagInput.FocusInput();
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
            tagInput.FocusInput();
            if (_model != null)
            {
                _model.TagSuggestions.LoadSuggestedTagsAsync().ContinueWith((x) => { pBar.Visibility = System.Windows.Visibility.Hidden; }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }

        private void editTags_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            Trace.Flush();
        }

        private void ClearTagsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_model != null)
            {
                _model.PageTags.Clear();
                tagInput.FocusInput();
            }
            e.Handled = true;
        }

        private void TagInputBox_Input(object sender, TagInputEventArgs e)
        {
            pagesTaggedPopup.IsOpen = false;
            progressPopup.IsOpen = false;
            try
            {
                if (tagInput.IsEmpty)
                {
                    suggestedTags.Highlighter = new TextSplitter();
                }
                else
                {
                    IEnumerable<string> tags = tagInput.Tags;
                    if (e.TagInputComplete)
                    {
                        _model.PageTags.AddAll(from t in tags where !_model.PageTags.ContainsKey(t) select new SimpleTagButtonModel(t));
                        tagInput.Clear();
                    }
                    suggestedTags.Highlighter = new TextSplitter(tagInput.Tags);           
                }
            }
            catch (Exception ex)
            {
                TraceLogger.Log(TraceCategory.Error(), "Processing Tag input failed with {0}", ex);
                TraceLogger.ShowGenericMessageBox(Properties.Resources.TagEditor_Input_Error, ex);
            }
            e.Handled = true;
        }

        private async Task ApplyPageTagsAsync(TagOperation op)
        {
            tagInput.FocusInput();
            try
            { 
                TraceLogger.Log(TraceCategory.Info(), "Applying tags to page");
                TaggingScope scope = ((TaggingScopeDescriptor)taggingScope.SelectedItem).Scope;
                
                Task<int> saveTask = _model.SavePageTagsAsync(op, scope);
                progressPopup.IsOpen = true;
     
                taggingScope.SelectedIndex = 0;
                tagInput.Clear();
                suggestedTags.Highlighter = new TextSplitter();
                progressPopup.IsOpen = false;
                pagesTaggedPopup.IsOpen = true;

                int pagesTagged = await saveTask;
                pagesTaggedText.Text = pagesTagged == 0 ? Properties.Resources.TagEditor_Popup_NothingTagged : string.Format(Properties.Resources.TagEditor_Popup_PagesTagged, pagesTagged); 
            }
            catch (Exception xe)
            {
                TraceLogger.Log(TraceCategory.Error(), "Applying tags to page failed: {0}", xe);
                TraceLogger.ShowGenericMessageBox(Properties.Resources.TagEditor_TagUpdate_Error, xe);
            }
        }

        private void handlePopupPointerAction(object sender, RoutedEventArgs e)
        {
            Popup p = sender as Popup;
            if (p != null)
            {
                p.IsOpen = false;
            }
            e.Handled = true;
        }
    }
}
