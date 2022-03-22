// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.edit;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Interaction logic for TagSearch.xaml
    /// </summary>
    [ComVisible(false)]
    public partial class FindTaggedPages : System.Windows.Window, IOneNotePageWindow<FindTaggedPagesModel>
    {
        #region TagPanelHeaderProperty
        /// <summary>
        /// Backing store for observable property <see cref="TagPanelHeader"/>
        /// </summary>
        public static readonly DependencyProperty TagPanelHeaderProperty = DependencyProperty.Register(
            nameof(TagPanelHeader),
            typeof(string),
            typeof(FindTaggedPages),
            new FrameworkPropertyMetadata("Refinement Tags"));

        /// <summary>
        /// Get/set the tag panel header text.
        /// </summary>
        public string TagPanelHeader {
            get => GetValue(TagPanelHeaderProperty) as string;
            set => SetValue(TagPanelHeaderProperty, value);
        }
        void UpdateTagPanelHeader() {
            TagPanelHeader = _model.SelectedRefinementTags.Count == 0
                ? string.Format("{0} ({1})",
                                Properties.Resources.TagSearch_Tags_GroupBox_Title,
                                _model.PageTagsSource.Count)
                : string.Format("{0} (Ո)",
                                Properties.Resources.TagSearch_Tags_GroupBox_Title);
        }
        #endregion PageCountProperty
        #region PagePanelHeaderProperty
        /// <summary>
        /// Backing store for observable property <see cref="PagePanelHeader"/>
        /// </summary>
        public static readonly DependencyProperty PagePanelHeaderProperty = DependencyProperty.Register(
            nameof(PagePanelHeader),
            typeof(string),
            typeof(FindTaggedPages),
            new FrameworkPropertyMetadata("Pages"));

        /// <summary>
        /// Get/set the tag panel header text.
        /// </summary>
        public string PagePanelHeader {
            get => GetValue(PagePanelHeaderProperty) as string;
            set => SetValue(PagePanelHeaderProperty, value);
        }
        void UpdatePagePanelHeader() {
            PagePanelHeader = string.Format("{0} ({1})",
                                                        Properties.Resources.TagSearch_Pages_GroupBox_Title,
                                                        _model.FilteredPages.Count);
        }
        #endregion PagePanelHeaderProperty
        #region RefinementTagsPanelHeaderProperty
        /// <summary>
        /// Backing store for observable property <see cref="RefinementTagsPanelHeader"/>
        /// </summary>
        public static readonly DependencyProperty RefinementTagsPanelHeaderProperty = DependencyProperty.Register(
            nameof(RefinementTagsPanelHeader),
            typeof(string),
            typeof(FindTaggedPages),
            new FrameworkPropertyMetadata("Filter tags"));

        /// <summary>
        /// Get/set the tag panel header text.
        /// </summary>
        public string RefinementTagsPanelHeader {
            get => GetValue(RefinementTagsPanelHeaderProperty) as string;
            set => SetValue(RefinementTagsPanelHeaderProperty, value);
        }

        void UpdateRefinementTagsPanelHeader() {
            RefinementTagsPanelHeader = string.Format("{0} ({1})",
                                                       Properties.Resources.TagSearch_SelectedTags_GroupBox_Title,
                                                       _model.SelectedRefinementTags.Count);
        }
        #endregion RefinementTagsPanelHeaderProperty
        /// <summary>
        /// Create a new instance of the find tags window
        /// </summary>
        public FindTaggedPages() {
            InitializeComponent();
            pBarCopy.Visibility = System.Windows.Visibility.Hidden;
        }
        #region IOneNotePageWindow<FindTaggedPagesModel>

        private FindTaggedPagesModel _model;
        /// <summary>
        /// get or set the view model backing this UI
        /// </summary>
        public FindTaggedPagesModel ViewModel {
            get {
                return _model;
            }
            set {
                _model = value;
                DataContext = _model;
                _model.FilteredPages.CollectionChanged += FilteredPages_CollectionChanged;
                UpdatePagePanelHeader();
                _model.PageTagsSource.CollectionChanged += TagSource_CollectionChanged;
                UpdateTagPanelHeader();
                _model.DependencyPropertyChanged += _model_DependencyPropertyChanged;
                _model.SelectedRefinementTags.CollectionChanged += SelectedRefinementTags_CollectionChanged;
                UpdateRefinementTagsPanelHeader();
            }
        }

        private void SelectedRefinementTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateRefinementTagsPanelHeader();
            UpdateTagPanelHeader();
        }

        #endregion IOneNotePageWindow<FindTaggedPagesModel>

        private void TagSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            Dispatcher.Invoke(() => UpdateTagPanelHeader());
        }
        private void FilteredPages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        Dispatcher.Invoke(() => UpdatePagePanelHeader());
        }

        #region UI events

        private async void Page_MenuItem_Click(object sender, RoutedEventArgs e) {
            MenuItem item = sender as MenuItem;
            if (item != null) {
                switch (item.Tag.ToString()) {
                    case "Refresh":
                        string query = searchComboBox.Text;

                        try {
                            pBar.Visibility = System.Windows.Visibility.Visible;
                            await _model.FindPagesAsync(query, scopeSelect.SelectedScope);
                            searchComboBox.SelectedValue = query;
                            pBar.Visibility = System.Windows.Visibility.Hidden;
                        } catch (System.Exception ex) {
                            TraceLogger.Log(TraceCategory.Error(), "search for '{0}' failed: {1}", query, ex);
                            TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_Find, ex);
                        }
                        break;

                    case "ClearSelection":
                        foundPagesList.UnselectAll();
                        break;

                    case "SelectAll":
                        foundPagesList.SelectAll();
                        break;

                    case "SaveSearch":
                        var onenote = ViewModel.OneNoteApp;
                        string currentPageID = onenote.CurrentPageID;
                        if (!string.IsNullOrEmpty(currentPageID)) {
                            var currentPage = onenote.GetHierarchy(currentPageID, HierarchyScope.hsSelf);
                            XAttribute recycleBinAtt = currentPage.Root.Attribute("isInRecycleBin");
                            if (recycleBinAtt != null && "true".Equals(recycleBinAtt.Value)) {
                                // TODO localize
                                MessageBox.Show("Pages cannot be created in the recycle bin!", Properties.Resources.TagEditor_WarningMessageBox_Title, MessageBoxButton.OK);
                            } else {
                                pBarCopy.Visibility = Visibility.Visible;
                                var newPageID = onenote.CreateNewPage(onenote.CurrentSectionID);
                                var pg = new PageBuilder.OneNotePage(onenote, newPageID);
                                SearchScope scope = scopeSelect.SelectedScope;
                                string searchstring = searchComboBox.Text;
                                var tagset = new PageTagSet(from rt in ViewModel.SelectedRefinementTags.Values
                                                            select rt.Tag);
                                var pages = (from p in ViewModel.FilteredPages.Values
                                             orderby p.Page.Name
                                             select p.Page).ToList();
                                await Task.Run(() => {
                                    // TODO localize
                                    pg.Tags = new PageTagSet("Saved Search", TagFormat.AsEntered);
                                    pg.SavedSearches.Add(searchstring, tagset.ToString(), scope, pages);
                                    pg.Update();
                                });
                                pBarCopy.Visibility = Visibility.Hidden;
                                onenote.NavigateTo(pg.PageID);
                            }
                        }

                        break;
                    case "TagSelection":
                        var pagesToTag = from mp in _model.FilteredPages
                                         where mp.IsSelected && !mp.IsInRecycleBin
                                         select mp.PageID;
                        if (pagesToTag.Count() == 0) {
                            MessageBox.Show(Properties.Resources.TagSearch_NoPagesSelectedWarning, Properties.Resources.TagEditor_WarningMessageBox_Title, MessageBoxButton.OK);
                        } else {
                            AddInDialogManager.ShowDialog<TagEditor, TagEditorModel>(() =>
                            {
                                var mdl = new TagEditorModel(_model.OneNoteApp);
                                // configure the model
                                mdl.Scope = TaggingScope.SelectedNotes;
                                mdl.PagesToTag = pagesToTag;
                                return mdl;
                            });
                        }
                        break;

                    case "MarkSelection":
                        var marker = new PageTagSet("-✩-", TagFormat.AsEntered);
                        int pagesTagged = 0;
                        foreach (var mdl in _model.FilteredPages.Where((p) => p.IsSelected && !p.IsInRecycleBin)) {
                            _model.OneNoteApp.TaggingService.Add(new Tagger.TaggingJob(mdl.PageID, marker, Tagger.TagOperation.UNITE));
                            pagesTagged++;
                        }
                        tagsPanel.Notification = pagesTagged == 0 ? Properties.Resources.TagEditor_Popup_NothingTagged : string.Format(Properties.Resources.TagEditor_Popup_TaggingInProgress, pagesTagged);
                        break;

                    case "CopyLinks":
                        pBarCopy.Visibility = System.Windows.Visibility.Visible;
                        string clip = await Task<string>.Run(() =>
                       {
                           string header =
   @"Version:0.9
StartHTML:{0:D6}
EndHTML:{1:D6}
StartFragment:{2:D6}
EndFragment:{3:D6}
StartSelection:{4:D6}
EndSelection:{5:D6}";
                           string htmlpre =
   @"<HTML>
<BODY>
<!--StartFragment-->";
                           StringBuilder links = new StringBuilder();

                           foreach (var mdl in _model.FilteredPages.Where(p => !p.IsInRecycleBin)) {
                               string linkTitle = mdl.LinkTitle;
                               try {
                                   if (links.Length > 0) {
                                       links.Append("<br />");
                                   }
                                   links.Append(@"<a href=""");
                                   links.Append(mdl.GetHyperlink(_model.OneNoteApp));
                                   links.Append(@""">");
                                   links.Append(linkTitle);
                                   links.Append("</a>");
                               } catch (Exception ex) {
                                   TraceLogger.Log(TraceCategory.Error(), "Link to page '{0}' could not be created: {1}", linkTitle, ex);
                                   TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_CopyLink, ex);
                               }
                           }

                           string htmlpost =
   @"<!--EndFragment-->
</BODY>
</HTML>";
                           string strLinks = links.ToString();
                           return string.Format(header,
                               header.Length,
                               header.Length + htmlpre.Length + strLinks.Length + htmlpost.Length,
                               header.Length + htmlpre.Length,
                               header.Length + htmlpre.Length + strLinks.Length,
                               header.Length + htmlpre.Length,
                               header.Length + htmlpre.Length + strLinks.Length)
                               + htmlpre + strLinks + htmlpost;
                       });
                        Clipboard.SetText(clip, TextDataFormat.Html);
                        pBarCopy.Visibility = System.Windows.Visibility.Hidden;
                        break;
                }

                e.Handled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            // save the scope Properties.Settings.Default.DefaultScope = (int)scopeSelect.SelectedScope;
            Properties.Settings.Default.Save();
            if (_model != null) {
                _model.Dispose();
            }

            Trace.Flush();
        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e) {
            try {
                HitHighlightedPageLink l = sender as HitHighlightedPageLink;
                if (l != null) {
                    HitHighlightedPageLinkModel model = l.DataContext as HitHighlightedPageLinkModel;
                    _model.NavigateTo(model.PageID);
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                        int ndx = foundPagesList.SelectedItems.IndexOf(model);
                        if (ndx >= 0) {
                            foundPagesList.SelectedItems.RemoveAt(ndx);
                        } else {
                            foundPagesList.SelectedItems.Add(model);
                        }
                    } else {
                        // select the link
                        foundPagesList.SelectedItem = model;
                    }

                    e.Handled = true;
                }
            } catch (System.Exception ex) {
                TraceLogger.Log(TraceCategory.Error(), "Navigation to OneNote page failed: {0}", ex);
                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_PageNavigation, ex);
            }
        }

        private async void ClearSelectionButton_Click(object sender, RoutedEventArgs e) {
            pBar.Visibility = System.Windows.Visibility.Visible;
            await _model.ClearTagFilterAsync();
            foreach (var t in _model.SelectedRefinementTags.Values) {
                t.SelectableTag.IsSelected = false;
            }
            _model.SelectedRefinementTags.Clear();
            pBar.Visibility = System.Windows.Visibility.Hidden;
            e.Handled = true;
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e) {
            string query = searchComboBox.Text;

            try {
                pBar.Visibility = Visibility.Visible;
                await _model.FindPagesAsync(query, scopeSelect.SelectedScope);
                searchComboBox.SelectedValue = query;
                pBar.Visibility = Visibility.Hidden;
            } catch (Exception ex) {
                TraceLogger.Log(TraceCategory.Error(), "search for '{0}' failed: {1}", query, ex);
                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_Find, ex);
            }
            e.Handled = true;
        }

        private async void SearchComboBox_KeyUp(object sender, KeyEventArgs e) {
            string query;
            switch (e.Key) {
                case Key.Enter:
                    query = searchComboBox.Text;
                    pBar.Visibility = Visibility.Visible;
                    await _model.FindPagesAsync(query, scopeSelect.SelectedScope);
                    pBar.Visibility = Visibility.Hidden;
                    searchComboBox.SelectedValue = query;
                    break;
                case Key.Escape:
                    searchComboBox.Text = string.Empty;
                    break;
            }
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            searchComboBox.Focus();
            Keyboard.Focus(searchComboBox);
        }

        private async void TagInputBox_Input(object sender, TagInputEventArgs e) {
            _model.PageTagsSource.Highlighter = tagInput.IsEmpty ? new TextSplitter() : new TextSplitter(e.Tags);
            if (e.TagInputComplete && !tagInput.IsEmpty) {
                // select all tags with exact full matches
                await SelectAllMatchingTagsAsync();
            } else if (e.Action == TagInputEventArgs.TaggingAction.Clear) {
                ClearSelectionButton_Click(sender, e);
            }
        }

        private async void ScopeSelector_ScopeChanged(object sender, ScopeChangedEventArgs e) {
            try {
                pBar.Visibility = Visibility.Visible;
                string query = searchComboBox.Text;
                // using ContinueWith until I've discovered how to do implement async
                // events properly
                await _model.FindPagesAsync(query, scopeSelect.SelectedScope);
                Dispatcher.Invoke(() => {
                    pBar.Visibility = Visibility.Hidden;
                    searchComboBox.SelectedValue = query;
                });

            } catch (System.Exception ex) {
                TraceLogger.Log(TraceCategory.Error(), "Changing search scope failed: {0}", ex);
                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_ScopeChange, ex);
            }
            e.Handled = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
            // Stop tracking current page
            ViewModel.EndTracking();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) {
            // start tracking current page
            ViewModel.BeginTracking();
        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            foreach (HitHighlightedPageLinkModel pl in e.RemovedItems) {
                pl.IsSelected = false;
            }
            foreach (HitHighlightedPageLinkModel pl in e.AddedItems) {
                pl.IsSelected = true;
            }
        }

        private void Tag_TagClick(object sender, RoutedEventArgs e) {
            if (_model != null) {
                tagInput.FocusInput();
                if (e.OriginalSource is Tag tagBtn
                    && tagBtn.DataContext is SelectedTagModel mdl) {
                    if (mdl.SelectableTag != null) {
                        // de-select that tag
                        mdl.SelectableTag.IsSelected = false;
                    }
                    _model.SelectedRefinementTags.RemoveAll(new string[] { mdl.Key });
                }
            }
        }

        private void SelectableTag_TagSelected(object sender, RoutedEventArgs e) {
            var btn = sender as SelectableTag;
            if (btn.DataContext is RefinementTagModel mdl) {
                _model.AddTagToFilter(mdl);
            }
        }
        #endregion UI events
        private void _model_DependencyPropertyChanged(object sender, DependencyPropertyChangedEventArgs e) {
           if (e.Property == FindTaggedPagesModel.CurrentPageTagsProperty) {
                // update query if necessary
                if (scopeSelect.SelectedScope != ViewModel.CurrentScope) { // rerun the query for the current scope
                    try {
                        pBar.Visibility = Visibility.Visible;
                        string query = searchComboBox.Text;
                        _model.FindPagesAsync(query, scopeSelect.SelectedScope).Wait();
                        tagInput.TagNames = ViewModel.CurrentPageTags;
                        pBar.Visibility = Visibility.Hidden;
                        searchComboBox.SelectedValue = query;
                    } catch (Exception ex) {
                        TraceLogger.Log(TraceCategory.Error(), "Changing search scope failed: {0}", ex);
                        TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_ScopeChange, ex);
                    }
                } else {
                    tagInput.TagNames = ViewModel.CurrentPageTags;
                }
            }
        }

        async Task SelectAllMatchingTagsAsync() {
            if (!tagInput.IsEmpty) {
                if (tagInput.IsPreset) {
                    await _model.ClearTagFilterAsync();
                    var failedTags = await _model.AddAllFullyMatchingTagsAsync(tagInput.TagNames);
                    if (failedTags.Count > 0) {
                        string scopeName;
                        switch (scopeSelect.SelectedScope) {
                            case SearchScope.Section:
                                scopeName = Properties.Resources.TagSearch_Scope_Section_Label;
                                break;
                            case SearchScope.SectionGroup:
                                scopeName = Properties.Resources.TagSearch_Scope_SectionGroup_Label;
                                break;
                            case SearchScope.Notebook:
                                scopeName = Properties.Resources.TagSearch_Scope_Notebook_Label;
                                break;
                            case SearchScope.AllNotebooks:
                                scopeName = Properties.Resources.TagSearch_Scope_AllNotebooks_Label;
                                break;
                            default:
                                scopeName = "Unknown Scope";
                                break;

                        }
                        selectedTags.Notification = string.Format(Properties.Resources.TagSearch_TagsIgnored,
                                                                  string.Join(",",from s in failedTags select s),
                                                                  scopeName);
                    }
                } else {
                    await _model.AddAllFullyHighlightedTagsAsync();
                }
            }
        }
        private async void SelectMatchingTagsButton_Click(object sender, RoutedEventArgs e) {
           await SelectAllMatchingTagsAsync();
        }
    }
}