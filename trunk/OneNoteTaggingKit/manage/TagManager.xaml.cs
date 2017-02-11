// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
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
                /// create new tag if it does not already exist
                string newName = rt_mdl.LocalName;

                if (!_model.SuggestedTags.ContainsKey(newName))
                {
                    _model.SuggestedTags.AddAll(new RemovableTagModel[] { new RemovableTagModel() { Tag = new TagPageSet(newName) } });
                }

                if (rt_mdl.Tag != null)
                {
                    string[] toAdd = new string[] { newName };
                    // remove the old tag and add new tag to the pages
                    foreach (var tp in rt_mdl.Tag.Pages)
                    {
                        _model.OneNoteApp.TaggingService.Add(new TaggingJob(tp.ID, toAdd, TagOperation.UNITE));
                        _model.OneNoteApp.TaggingService.Add(new TaggingJob(tp.ID, toRemove, TagOperation.SUBTRACT));
                    }
                    suggestedTags.Notification = rt_mdl.Tag.Pages.Count == 0 ? Properties.Resources.TagEditor_Popup_NothingTagged : string.Format(Properties.Resources.TagEditor_Popup_TaggingInProgress, rt_mdl.Tag.Pages.Count);
                    TraceLogger.Log(TraceCategory.Info(), "{0} page(s) enqueued for background tagging; Operation UNITE {1} SUBTRACT {2}", rt_mdl.Tag.Pages.Count, toAdd[0], toRemove[0]);
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
            _model.SuggestedTags.AddAll(from t in tagInput.Tags where !_model.SuggestedTags.ContainsKey(t) select new RemovableTagModel() { Tag = new TagPageSet(t) });
            suggestedTags.Highlighter = null;
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
            switch (itm.Tag.ToString())
            {
                case "Copy":
                    Clipboard.SetData(DataFormats.Text, _model.TagList);
                    tagInput.FocusInput();
                    break;

                case "Refresh":
                    if (pBar.Visibility == System.Windows.Visibility.Hidden)
                    {
                        pBar.Visibility = System.Windows.Visibility.Visible;
                        await _model.LoadSuggestedTagsAsync();
                        if (tagInput.Tags != null)
                        {
                            suggestedTags.Highlighter = new TextSplitter(tagInput.Tags);
                        }
                        pBar.Visibility = System.Windows.Visibility.Hidden;
                    }
                    break;
            }
        }

        private void TagInputBox_Input(object sender, TagInputEventArgs e)
        {
            if (e.TagInputComplete)
            {
                NewTagButton_Click(sender, e);
            }
            else
            {
                suggestedTags.Highlighter = new TextSplitter(tagInput.Tags);
                e.Handled = true;
            }
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