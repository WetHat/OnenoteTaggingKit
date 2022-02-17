// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;
using WetHatLab.OneNote.TaggingKit.Tagger;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    /// Interaction logic for TagManager.xaml user control
    /// </summary>
    /// <remarks>Implements the tag management dialog logic</remarks>
    [ComVisible(false)]
    public partial class TagManager : Window, IOneNotePageWindow<TagManagerModel>
    {
        private TagManagerModel _model;

        /// <summary>
        /// Create a new instance of a tag management dialog.
        /// </summary>
        public TagManager()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Remove or rename tag from suggestions when user control is tapped.
        /// </summary>
        /// <param name="sender">user control emitting this event</param>
        /// <param name="e">     event details</param>
        private void Tag_Action(object sender, RoutedEventArgs e)
        {
            var rt = sender as RemovableTag;
            var rt_mdl = rt.DataContext as RemovableTagModel;
            string[] toRemove = new string[] { rt_mdl.Key };
            if ("DeleteTag".Equals(rt.Tag))
            {
                _model.SuggestedTags.RemoveAll(toRemove);
                // schedule all pages with this tag for tag removal
                if (rt_mdl.Tag != null)
                {
                    foreach (var tp in rt_mdl.Tag.Pages)
                    {
                        _model.OneNoteApp.TaggingService.Add(new TaggingJob(tp.ID, toRemove, TagOperation.SUBTRACT));
                    }
                    suggestedTags.Notification = rt_mdl.Tag.Pages.Count == 0 ? Properties.Resources.TagEditor_Popup_NothingTagged : string.Format(Properties.Resources.TagEditor_Popup_TaggingInProgress, rt_mdl.Tag.Pages.Count);
                    TraceLogger.Log(TraceCategory.Info(), "{0} page(s) enqueued for background tagging; Operation SUBTRACT {1}", rt_mdl.Tag.Pages.Count, toRemove[0]);
                }
                else
                {
                    suggestedTags.Notification = Properties.Resources.TagEditor_Popup_NothingTagged;
                }
            }
            else if ("RenameTag".Equals(rt.Tag))
            {
                _model.SuggestedTags.RemoveAll(toRemove);

                string[] newTagNames = (from tn in TaggedPage.ParseTaglist(rt_mdl.LocalName)
                                        select TagFormatter.Format(tn)).ToArray();

                // create new tag models unless they already exist
                List<RemovableTagModel> newTagModels = new List<RemovableTagModel>();
                foreach (var newName in newTagNames)
                {
                    RemovableTagModel tagmodel;
                    if (!_model.SuggestedTags.TryGetValue(newName, out tagmodel))
                    {
                        // renamed to non-existing tag
                        tagmodel = new RemovableTagModel() { Tag = new TagPageSet(newName) };
                        newTagModels.Add(tagmodel);
                    }
                    else if (tagmodel.Tag == null && rt_mdl.Tag != null)
                    { // renamed to existing tag which is not used anywhere
                        tagmodel.Tag = new TagPageSet(newName);
                    }

                    if (rt_mdl.Tag != null)
                    {
                        // copy the pages into the new tag and update the tag count
                        foreach (var pg in rt_mdl.Tag.Pages)
                        {
                            tagmodel.Tag.Pages.Add(pg);
                        }
                        tagmodel.UseCount = tagmodel.Tag.Pages.Count;
                    }
                }
                _model.SuggestedTags.AddAll(newTagModels);

                if (rt_mdl.Tag != null)
                {
                    // remove the old tag and add new tag to the pages
                    foreach (var tp in rt_mdl.Tag.Pages)
                    {
                        _model.OneNoteApp.TaggingService.Add(new TaggingJob(tp.ID, toRemove, TagOperation.SUBTRACT));
                        _model.OneNoteApp.TaggingService.Add(new TaggingJob(tp.ID, newTagNames, TagOperation.UNITE));
                    }
                    suggestedTags.Notification = rt_mdl.Tag.Pages.Count == 0 ? Properties.Resources.TagEditor_Popup_NothingTagged : string.Format(Properties.Resources.TagEditor_Popup_TaggingInProgress, rt_mdl.Tag.Pages.Count);
                    TraceLogger.Log(TraceCategory.Info(), "{0} page(s) enqueued for background tagging; Operation UNITE {1} SUBTRACT {2}", rt_mdl.Tag.Pages.Count, string.Join(",", newTagNames), toRemove[0]);
                }
                else
                {
                    suggestedTags.Notification = Properties.Resources.TagEditor_Popup_NothingTagged;
                }
            }
            TraceLogger.Flush();
            _model.SaveChanges();
        }

        /// <summary>
        /// Add a new tag to the list of suggestions when the button is pressed
        /// </summary>
        /// <param name="sender">control emitting the event</param>
        /// <param name="e">     event details</param>
        private void NewTagButton_Click(object sender, RoutedEventArgs e)
        {
            _model.SuggestedTags.AddAll(from t in tagInput.Tags where !_model.SuggestedTags.ContainsKey(t) select new RemovableTagModel() { Tag = new TagPageSet(TagFormatter.Format(t)) });
            tagInput.Clear();
            _model.SaveChanges();
            e.Handled = true;
        }

        #region IOneNotePageDialog<TagManagerModel>

        /// <summary>
        /// Get or set the dialog's view model.
        /// </summary>
        /// <remarks>
        /// As soon as the view model is defined a background collection of tags is started
        /// </remarks>
        public TagManagerModel ViewModel
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
                DataContext = _model;
            }
        }

        #endregion IOneNotePageDialog<TagManagerModel>

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            Trace.Flush();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;

            string navigateUri = hl.NavigateUri.ToString();

            Process.Start(new ProcessStartInfo(navigateUri));

            e.Handled = true;
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var itm = sender as MenuItem;
            if (pBar.Visibility == System.Windows.Visibility.Hidden) {
                pBar.Visibility = System.Windows.Visibility.Visible;
                switch (itm.Tag.ToString()) {
                    case "Copy":
                        Clipboard.SetData(DataFormats.UnicodeText, _model.TagList);
                        tagInput.FocusInput();
                        break;
                    case "Refresh":
                        await _model.LoadSuggestedTagsAsync();
                        if (tagInput.Tags != null) {
                            _model.SuggestedTags.Highlighter = new TextSplitter(tagInput.Tags);
                        }
                        pBar.Visibility = Visibility.Hidden;

                        Properties.Settings.Default.Save();
                        break;
                    case "SortByName":
                       _model.SortByTagName();
                        byName.IsChecked = true;
                        byUsage.IsChecked = false;

                        break;
                    case "SortByUsage":
                        _model.SortByUsage();
                        byName.IsChecked = false;
                        byUsage.IsChecked = true;
                        break;
                }
                TraceLogger.Flush();
                pBar.Visibility = Visibility.Hidden;
            }
        }

        private void TagInputBox_Input(object sender, TagInputEventArgs e)
        {
            _model.SuggestedTags.Highlighter = new TextSplitter(tagInput.Tags);
            if (e.TagInputComplete && !tagInput.IsEmpty)
            {
                NewTagButton_Click(sender, e);
            }
            e.Handled = true;
        }

        private void Hyperlink_RequestLogNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            TraceLogger.Flush();
            Hyperlink hl = (Hyperlink)sender;

            string path = hl.NavigateUri.LocalPath;

            Process.Start(new ProcessStartInfo("notepad.exe", path));

            e.Handled = true;
        }

        private async void TabItem_Selected(object sender, RoutedEventArgs e)
        {
            if (pBar.Visibility == System.Windows.Visibility.Visible)
            {
                await _model.LoadSuggestedTagsAsync();
                pBar.Visibility = System.Windows.Visibility.Hidden;
            }
        }
    }
}