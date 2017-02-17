// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using System;
using System.Text;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Interaction logic for TagSearch.xaml
    /// </summary>
    [ComVisible(false)]
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

        #region IOneNotePageWindow<FindTaggedPagesModel>

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

        #endregion IOneNotePageWindow<FindTaggedPagesModel>

        #region UI events

        private async void Page_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (item != null)
            {
                switch (item.Tag.ToString())
                {
                    case "Refresh":
                        string query = searchComboBox.Text;

                        try
                        {
                            pBar.Visibility = System.Windows.Visibility.Visible;
                            await _model.FindPagesAsync(query, scopeSelect.SelectedScope);
                            searchComboBox.SelectedValue = query;
                            pBar.Visibility = System.Windows.Visibility.Hidden;
                        }
                        catch (System.Exception ex)
                        {
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

                    case "CopyLinks":
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

                        foreach (var mdl in _model.Pages.Where((p) => p.IsSelected))
                        {
                            string pageTitle = mdl.LinkTitle;
                            try
                            {
                                if (links.Length > 0)
                                {
                                    links.Append("<br />");
                                }
                                links.Append(@"<a href=""");
                                links.Append(mdl.PageLink);
                                links.Append(@""">");
                                links.Append(mdl.LinkTitle);
                                links.Append("</a>");
                            }
                            catch (Exception ex)
                            {
                                TraceLogger.Log(TraceCategory.Error(), "Link to page '{0}' could not be created: {1}", pageTitle, ex);
                                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_CopyLink, ex);
                            }
                        }

                        string htmlpost =
@"<!--EndFragment-->
</BODY>
</HTML>";
                        string strLinks = links.ToString();
                        string clip = string.Format(header,
                            header.Length,
                            header.Length + htmlpre.Length + strLinks.Length + htmlpost.Length,
                            header.Length + htmlpre.Length,
                            header.Length + htmlpre.Length + strLinks.Length,
                            header.Length + htmlpre.Length,
                            header.Length + htmlpre.Length + strLinks.Length)
                            + htmlpre + strLinks + htmlpost;
                        Clipboard.SetText(clip, TextDataFormat.Html);
                        break;
                }

                e.Handled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // save the scope Properties.Settings.Default.DefaultScope = (int)scopeSelect.SelectedScope;
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
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        int ndx = foundPagesList.SelectedItems.IndexOf(model);
                        if (ndx >= 0)
                        {
                            foundPagesList.SelectedItems.RemoveAt(ndx);
                        }
                        else
                        {
                            foundPagesList.SelectedItems.Add(model);
                        }
                    }
                    else
                    {
                        // select the link
                        foundPagesList.SelectedItem = model;
                    }

                    e.Handled = true;
                }
            }
            catch (System.Exception ex)
            {
                TraceLogger.Log(TraceCategory.Error(), "Navigation to OneNote page failed: {0}", ex);
                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_PageNavigation, ex);
            }
        }

        private async void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            pBar.Visibility = System.Windows.Visibility.Visible;
            await _model.ClearTagFilterAsync();
            pBar.Visibility = System.Windows.Visibility.Hidden;
            foreach (var t in _model.Tags.Values)
            {
                t.IsChecked = false;
            }
            e.Handled = true;
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = searchComboBox.Text;

            try
            {
                pBar.Visibility = System.Windows.Visibility.Visible;
                await _model.FindPagesAsync(query, scopeSelect.SelectedScope);
                searchComboBox.SelectedValue = query;
                pBar.Visibility = System.Windows.Visibility.Hidden;
            }
            catch (System.Exception ex)
            {
                TraceLogger.Log(TraceCategory.Error(), "search for '{0}' failed: {1}", query, ex);
                TraceLogger.ShowGenericErrorBox(Properties.Resources.TagSearch_Error_Find, ex);
            }
            e.Handled = true;
        }

        private async void SearchComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                string query = searchComboBox.Text;
                pBar.Visibility = System.Windows.Visibility.Visible;
                await _model.FindPagesAsync(query, scopeSelect.SelectedScope);
                pBar.Visibility = System.Windows.Visibility.Hidden;
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
                // using ContinueWith until I've discovered how to do implement async
                // events properly
                _model.FindPagesAsync(query, scopeSelect.SelectedScope).ContinueWith(tsk => Dispatcher.Invoke(() =>
                {
                    pBar.Visibility = System.Windows.Visibility.Hidden;
                    searchComboBox.SelectedValue = query;
                }));
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
            if (e == FindTaggedPagesModel.PAGE_COUNT)
            {
                foundPagesList.UnselectAll();
            }
            else if (e == FindTaggedPagesModel.CURRENT_TAGS)
            {
                // update query if necessary
                string thisScopID;
                switch (scopeSelect.SelectedScope)
                {
                    case SearchScope.Notebook:
                        thisScopID = ViewModel.OneNoteApp.CurrentNotebookID;
                        break;

                    case SearchScope.SectionGroup:
                        thisScopID = ViewModel.OneNoteApp.CurrentSectionGroupID;
                        break;

                    case SearchScope.Section:
                        thisScopID = ViewModel.OneNoteApp.CurrentSectionID;
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
                        _model.FindPagesAsync(query, scopeSelect.SelectedScope).Wait();
                        tagInput.Tags = ViewModel.CurrentTags;
                        pBar.Visibility = System.Windows.Visibility.Hidden;
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

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (HitHighlightedPageLinkModel pl in e.RemovedItems)
            {
                pl.IsSelected = false;
            }
            foreach (HitHighlightedPageLinkModel pl in e.AddedItems)
            {
                pl.IsSelected = true;
            }
        }
    }
}