using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Interaction logic for TagSearch.xaml
    /// </summary>
    public partial class FindTaggedPages : Window, IOneNotePageWindow<FindTaggedPagesModel>
    {
        private FindTaggedPagesModel _model;

        /// <summary>
        /// Create a new instance of the find tags window
        /// </summary>
        public FindTaggedPages()
        {
            InitializeComponent();
        }

        #region IOneNotePageWindow<TagSearchModel>

        /// <summary>
        /// get or set the view model backing this UI
        /// </summary>
        public FindTaggedPagesModel ViewModel
        {
            get
            {
                return _model;        
            }
            set
            {
                _model = value;
                _model.PropertyChanged += _model_PropertyChanged;
                DataContext = _model;
            }
        }

        #endregion IOneNotePageWindow<TagSearchModel>

        #region UI events
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            if (_model != null)
            {
                _model.Dispose();
            }

            Trace.Flush();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HitHighlightedPageLink l = sender as HitHighlightedPageLink;
                if (l != null)
                {
                    HitHighlightedPageLinkModel model = l.DataContext as HitHighlightedPageLinkModel;
                    _model.NavigateTo(model.PageID);
                    e.Handled = true;
                }
            }
            catch (System.Exception ex)
            {
                TraceLogger.Log(TraceCategory.Error(), "Navigation to OneNote page failed: {0}", ex);
                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_PageNavigation, ex);
            }
        }

        private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            pBar.Visibility = System.Windows.Visibility.Visible;
            _model.ClearTagFilterAsync(() => { pBar.Visibility = System.Windows.Visibility.Hidden;
                                               foreach (var t in _model.Tags.Values)
                                               {
                                                   t.IsChecked = false;
                                               }
                                             });
            e.Handled = true;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = searchComboBox.Text;
                
            try
            {
                pBar.Visibility = System.Windows.Visibility.Visible;
                _model.FindPagesAsync(query, scopeSelect.SelectedScope,() => pBar.Visibility = System.Windows.Visibility.Hidden);
                searchComboBox.SelectedValue = query;
            }
            catch (System.Exception ex)
            {
                TraceLogger.Log(TraceCategory.Error(), "search for '{0}' failed: {1}", query, ex);
                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_Find, ex);
            }
            e.Handled = true;
        }

        private void SearchComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                string query = searchComboBox.Text;
                pBar.Visibility = System.Windows.Visibility.Visible;
                _model.FindPagesAsync(query, scopeSelect.SelectedScope,() => pBar.Visibility = System.Windows.Visibility.Hidden);
                searchComboBox.SelectedValue = query;
            }
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            searchComboBox.Focus();
            Keyboard.Focus(searchComboBox);
        }

        private void TagInputBox_Input(object sender, TagInputEventArgs e)
        {
            if (!e.TagInputComplete)
            {
                tagsPanel.Highlighter = new TextSplitter(tagInput.Tags);
            }
        }
        
        private void ScopeSelector_ScopeChanged(object sender, ScopeChangedEventArgs e)
        {
            try
            {
                pBar.Visibility = System.Windows.Visibility.Visible;
                string query = searchComboBox.Text;
                _model.FindPagesAsync(query, scopeSelect.SelectedScope, () => pBar.Visibility = System.Windows.Visibility.Hidden);
                searchComboBox.SelectedValue = query;
            }
            catch (System.Exception ex)
            {
                TraceLogger.Log(TraceCategory.Error(), "Changing search scope failed: {0}", ex);
                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_ScopeChange, ex);
            }
            e.Handled = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Stop tracking current page
            ViewModel.EndTracking();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // start tracking current page
            ViewModel.BeginTracking();
        }

        #endregion UI events

        private void _model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == FindTaggedPagesModel.CURRENT_TAGS)
            {
                // update query if necessary
                string thisScopID;
                switch (scopeSelect.SelectedScope)
                {
                    case SearchScope.Notebook:
                        thisScopID = ViewModel.CurrentNotebookID;
                        break;
                    case SearchScope.SectionGroup:
                        thisScopID = ViewModel.CurrentSectionGroupID;
                        break;
                    case SearchScope.Section:
                        thisScopID = ViewModel.CurrentSectionID;
                        break;
                    default:
                        thisScopID = string.Empty;
                        break;
                }
                if (!thisScopID.Equals(ViewModel.LastScopeID))
                { // rerun the query for the current scope
                    try
                    {
                        pBar.Visibility = System.Windows.Visibility.Visible;
                        string query = searchComboBox.Text;
                        _model.FindPagesAsync(query, scopeSelect.SelectedScope, () =>
                        {
                            tagInput.Tags = ViewModel.CurrentTags;
                            pBar.Visibility = System.Windows.Visibility.Hidden;
                        });
                        searchComboBox.SelectedValue = query;
                    }
                    catch (System.Exception ex)
                    {
                        TraceLogger.Log(TraceCategory.Error(), "Changing search scope failed: {0}", ex);
                        TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_ScopeChange, ex);
                    }
                }
                else
                {
                    tagInput.Tags = ViewModel.CurrentTags;
                }
            }
        }
    }
}
