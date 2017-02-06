// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

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
        /// remove tag from suggestions when user control is tapped.
        /// </summary>
        /// <param name="sender">user control emitting this event</param>
        /// <param name="e">event details</param>
        private void TagButton_Click(object sender, RoutedEventArgs e)
        {
            RemovableTag btn = sender as RemovableTag;
            _model.SuggestedTags.RemoveAll(new string[] { ((RemovableTagModel)btn.DataContext).Key });
            _model.SaveChanges();
        }

        /// <summary>
        /// Add a new tag to the list of suggestions when the button is pressed
        /// </summary>
        /// <param name="sender">control emitting the event</param>
        /// <param name="e">event details</param>
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
        /// <remarks>As soon as the view model is defined a background collection of tags is started</remarks>
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

        private void Copy_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, _model.TagList);
            tagInput.FocusInput();
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