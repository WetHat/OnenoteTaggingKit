// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Interaction logic for TagSearch.xaml
    /// </summary>
    [ComVisible(false)]
    public partial class FindTaggedPages : System.Windows.Window, IOneNotePageWindow<FindTaggedPagesModel>
    {
        /// <summary>
        /// Backing store for observable property <see cref="SearchButtonBorderColor"/>
        /// </summary>
        #region SearchButtonBorderColor
        public static readonly DependencyProperty SearchButtonBorderColorProperty = DependencyProperty.Register(
            nameof(SearchButtonBorderColor),
            typeof(Brush),
            typeof(FindTaggedPages),
            new FrameworkPropertyMetadata(Brushes.Transparent)
            );

        /// <summary>
        /// Get or set the Search button border color.
        /// </summary>
        public Brush SearchButtonBorderColor {
            get => GetValue(SearchButtonBorderColorProperty) as Brush;
            set => SetValue(SearchButtonBorderColorProperty, value);
        }
        #endregion SearchButtonBorderColor
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
            var withalltags = ViewModel.WithAllTagsFilterModel.Filter.SelectedTags.Count > 0 ? "⋂" : string.Empty;
            var exceptwithtags = ViewModel.ExceptWithTagsFilterModel.Filter.SelectedTags.Count > 0 ? "⊄" : string.Empty;
            var query = string.IsNullOrWhiteSpace(_lastSearch) ? string.Empty : "🔍";
            var filtered = withalltags == string.Empty && exceptwithtags == string.Empty && query == string.Empty ? string.Empty : "  "; // <-
            // Properties.Resources.TagSearch_Pages_GroupBox_Title,
            PagePanelHeader = string.Format("{0}/{1} {2}{3}{4}{5}{6}",
                ViewModel.FilteredPages.Count,
                ViewModel.TagsAndPages.Pages.Count,
                "", // pages icon
                filtered,
                withalltags,
                exceptwithtags,
                query);
        }
        #endregion PagePanelHeaderProperty
        #region ProgressBarTextProperty
        /// <summary>
        ///     Backing store for observable property <see cref="PagePanelHeader"/>
        /// </summary>
        public static readonly DependencyProperty ProgressBarTextProperty = DependencyProperty.Register(
            nameof(ProgressBarText),
            typeof(string),
            typeof(FindTaggedPages),
            new FrameworkPropertyMetadata("..."));

        /// <summary>
        ///     Get/set the progress bar text text.
        /// </summary>
        public string ProgressBarText {
            get => GetValue(ProgressBarTextProperty) as string;
            set => SetValue(ProgressBarTextProperty, value);
        }
        #endregion ProgressBarTextProperty

        string _lastSearch = string.Empty;
        /// <summary>
        /// Create a new instance of the find tags window
        /// </summary>
        public FindTaggedPages() {
            InitializeComponent();
            pBar.Visibility = System.Windows.Visibility.Hidden;
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
                ViewModel.DependencyPropertyChanged += ViewModel_DependencyPropertyChanged;
            }
        }

        #endregion IOneNotePageWindow<FindTaggedPagesModel>

        void UpdateClearSelectionButton() {
            double totalTabWidth = 0.0;
            bool visible = false;
            foreach (var itm in filterTabs.Items) {
                if (itm is TabItem tab
                    && tab.Content is TagFilterPanel filter
                    && filter.DataContext is TagFilterPanelModel mdl) {
                    totalTabWidth += tab.ActualWidth;
                    visible |= !mdl.Filter.AutoUodateEnabled && mdl.Filter.SelectedTags.Count > 0;
                }
            }
            if (visible) {
                clearAllTagSelection.Margin = new Thickness(totalTabWidth + 5, 0, 0, 0);
                clearAllTagSelection.Visibility = Visibility.Visible;
            } else {
                clearAllTagSelection.Visibility = Visibility.Collapsed;
            }
        }
        private void ViewModel_DependencyPropertyChanged(object sender, DependencyPropertyChangedEventArgs e) {
            switch (e.Property.Name) {
                case nameof(FindTaggedPagesModel.WithAllTabLabel):
                case nameof(FindTaggedPagesModel.ExceptWithTabLabel):
                    UpdateClearSelectionButton();
                    break;
            }
        }

        private void FilteredPages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            Dispatcher.Invoke(() => UpdatePagePanelHeader());
        }
        private void TabItem_Selected(object sender, RoutedEventArgs e) {
            var activetab = sender as TabItem;
            foreach (var itm in filterTabs.Items) {
                if (itm is TabItem tab
                    && tab.Content is TagFilterPanel filter
                    && filter.DataContext is TagFilterPanelModel mdl) {
                    mdl.Filter.AutoUodateEnabled = activetab == tab;
                }
            }
            UpdateClearSelectionButton();
        }
        #region UI events

        private async void Page_MenuItem_Click(object sender, RoutedEventArgs e) {
            MenuItem item = sender as MenuItem;
            if (item != null) {
                switch (item.Tag.ToString()) {
                    case "Refresh":
                        string query = searchComboBox.Text;

                        try {
                            ProgressBarText = "Refreshing"; // TODO localize
                            pBar.Visibility = Visibility.Visible;
                            await ViewModel.FindPagesAsync(query, scopeSelect.SelectedScope);
                            searchComboBox.SelectedValue = query;
                            pBar.Visibility = Visibility.Hidden;
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
                                MessageBox.Show(Properties.Resources.TagSearch_Recyclebin_Error, Properties.Resources.TagEditor_WarningMessageBox_Title, MessageBoxButton.OK);
                            } else {
                                ProgressBarText = Properties.Resources.TagSearch_Links_ProgressBar_Title;
                                ProgressBarText = "Saving Search"; // TODO localize
                                pBar.Visibility = Visibility.Visible;
                                var newPageID = onenote.CreateNewPage(onenote.CurrentSectionID);
                                var pg = new PageBuilder.OneNotePage(onenote, newPageID, Properties.Resources.NewSavedSearchPage_Title);
                                SearchScope scope = scopeSelect.SelectedScope;
                                string searchstring = searchComboBox.Text;
                                var tagset = new PageTagSet(from sel in ViewModel.WithAllTagsFilterModel.Filter.SelectedTags.Values
                                                            select sel.Tag);
                                var pages = (from p in ViewModel.FilteredPages.Values
                                             where !p.IsInRecycleBin
                                             orderby p.Page.Name
                                             select p.Page).ToList();
                                await Task.Run(() => {
                                    pg.Tags = new PageTagSet(Properties.Resources.SavedSearchTagName, TagFormat.AsEntered);
                                    pg.SavedSearches.Add(searchstring, tagset, scope, pages);
                                    pg.Update();
                                });
                                pBar.Visibility = Visibility.Hidden;
                                onenote.NavigateTo(pg.PageID);
                            }
                        }

                        break;
                    case "TagSelection":
                        var pagesToTag = from mp in ViewModel.FilteredPages
                                         where mp.IsSelected && !mp.IsInRecycleBin
                                         select mp.PageID;
                        if (pagesToTag.Count() == 0) {
                            MessageBox.Show(Properties.Resources.TagSearch_NoPagesSelectedWarning, Properties.Resources.TagEditor_WarningMessageBox_Title, MessageBoxButton.OK);
                        } else {
                            AddInDialogManager.ShowDialog<TagEditor, TagEditorModel>(() =>
                            {
                                var mdl = new TagEditorModel(ViewModel.OneNoteApp);
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
                        foreach (var mdl in ViewModel.FilteredPages.Where((p) => p.IsSelected && !p.IsInRecycleBin)) {
                            ViewModel.OneNoteApp.TaggingService.Add(new Tagger.TaggingJob(mdl.PageID, marker, Tagger.TagOperation.UNITE));
                            pagesTagged++;
                        }
                        withAllFilter.Notification = pagesTagged == 0 ? Properties.Resources.TagEditor_Popup_NothingTagged : string.Format(Properties.Resources.TagEditor_Popup_TaggingInProgress, pagesTagged);
                        break;

                    case "CopyLinks":
                        ProgressBarText = Properties.Resources.TagSearch_Links_ProgressBar_Title;
                        pBar.Visibility = System.Windows.Visibility.Visible;
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

                           foreach (var mdl in ViewModel.FilteredPages.Where(p => !p.IsInRecycleBin && p.IsSelected)) {
                               string linkTitle = mdl.LinkTitle;
                               try {
                                   if (links.Length > 0) {
                                       links.Append("<br />");
                                   }
                                   links.Append(@"<a href=""");
                                   links.Append(mdl.GetHyperlink(ViewModel.OneNoteApp));
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
                        pBar.Visibility = Visibility.Hidden;
                        break;
                }

                e.Handled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (_model != null) {
                _model.Dispose();
                _model = null;
            }
        }
        private void Hyperlink_Click(object sender, RoutedEventArgs e) {
            try {
                HitHighlightedPageLink l = sender as HitHighlightedPageLink;
                if (l != null) {
                    HitHighlightedPageLinkModel model = l.DataContext as HitHighlightedPageLinkModel;
                    ViewModel.NavigateTo(model.PageID);
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

        private async void SearchButton_Click(object sender, RoutedEventArgs e) {
            try {
                pBar.Visibility = Visibility.Visible;
                ProgressBarText = "Searching"; // TODO localize
                _lastSearch = searchComboBox.Text;
                if (string.IsNullOrWhiteSpace(_lastSearch)) {
                    _lastSearch = string.Empty;
                }
                await ViewModel.FindPagesAsync(_lastSearch, scopeSelect.SelectedScope);
                SearchButtonBorderColor = Brushes.Transparent;
                pBar.Visibility = Visibility.Hidden;
                searchComboBox.SelectedValue = _lastSearch;
                UpdatePagePanelHeader();
            } catch (Exception ex) {
                TraceLogger.Log(TraceCategory.Error(), "search for '{0}' failed: {1}", searchComboBox.Text, ex);
                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_Find, ex);
            }
            e.Handled = true;
        }

        private void SearchComboBox_KeyUp(object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Enter:
                    SearchButton_Click(sender, e);
                    break;
                case Key.Escape:
                    searchComboBox.Text = string.Empty;
                    break;
            }
            SearchButtonBorderColor = _lastSearch.Equals(searchComboBox.Text)
                ? Brushes.Transparent
                : Brushes.Red;
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            searchComboBox.Focus();
            Keyboard.Focus(searchComboBox);
        }

        private async void ScopeSelector_ScopeChanged(object sender, ScopeChangedEventArgs e) {
            try {
                string scopename = "...";
                switch (scopeSelect.SelectedScope) {
                    case SearchScope.AllNotebooks:
                        scopename = Properties.Resources.TagSearch_Scope_AllNotebooks_Label;
                        break;
                    case SearchScope.Notebook:
                        scopename = Properties.Resources.TagSearch_Scope_Notebook_Label;
                        break;
                    case SearchScope.SectionGroup:
                        scopename = Properties.Resources.TagSearch_Scope_SectionGroup_Label;
                        break;
                    case SearchScope.Section:
                        scopename = Properties.Resources.TagSearch_Scope_Section_Label;
                        break;
                }
                ProgressBarText = string.Format("Searching {0}", scopename); // TODO localize
                pBar.Visibility = Visibility.Visible ;
                string query = searchComboBox.Text;
                // using ContinueWith until I've discovered how to do implement async
                // events properly
                await ViewModel.FindPagesAsync(query, scopeSelect.SelectedScope);
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
            foreach (var it in filterTabs.Items) {
                var tab = it as TabItem;
                tab.IsSelected = "WithAll".Equals(tab.Tag);
            }
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

        private void clearTagSelect_Click(object sender, RoutedEventArgs e) {
            foreach (var itm in filterTabs.Items) {
                if (itm is TabItem tab
                    && tab.Content is TagFilterPanel filter
                    && filter.DataContext is TagFilterPanelModel mdl) {
                    mdl.ClearFilter();
                }
            }
        }
        #endregion UI events
    }
}