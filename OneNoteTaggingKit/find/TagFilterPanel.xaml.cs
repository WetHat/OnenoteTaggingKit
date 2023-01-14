using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    ///     interaction logic for the user control.
    /// </summary>
    [ComVisible(false)]
    public partial class TagFilterPanel : UserControl
    {
        #region NotificationProperty
        /// <summary>
        /// Dependecy property definition for <see cref="Notification"/>
        /// </summary>
        public static readonly DependencyProperty NotificationProperty = DependencyProperty.Register(
            nameof(Notification),
            typeof(string),
            typeof(TagFilterPanel));

        /// <summary>
        /// Get or set the popup notification which is displayed for a few second
        /// over this list control.
        /// </summary>
        public string Notification {
            get => GetValue(NotificationProperty) as string;
            set => SetValue(NotificationProperty, value);
        }
        #endregion NotificationProperty

        #region Event Handlers
        private void SelectedTag_TagClick(object sender, RoutedEventArgs e) {
            if (e.OriginalSource is Tag tagBtn
                && tagBtn.DataContext is SelectedTagModel sel) {
                sel.SelectableTag.IsSelected = false;
            }
            tagInput.FocusInput();
        }
        void SelectableTag_TagSelected(object sender, RoutedEventArgs e) {
            if (sender is SelectableTag btn
                && btn.DataContext is RefinementTagModel mdl) {
                mdl.IsSelected = true;
            }
        }
        async void TagInputBox_Input(object sender, TagInputEventArgs e) {
            pBar.Visibility = Visibility.Visible;
            ViewModel.RefinementTagModels.Highlighter = tagInput.IsEmpty ? new TextSplitter() : new TextSplitter(e.Tags);
            if (e.TagInputComplete && !tagInput.IsEmpty) {
                // select all tags with exact full matches
                await SelectAllMatchingTagsAsync();
            } else if (e.Action == TagInputEventArgs.TaggingAction.Clear) {
                ClearSelectionButton_Click(sender, e);
            }
            pBar.Visibility = Visibility.Hidden;
        }
        private async void ClearSelectionButton_Click(object sender, RoutedEventArgs e) {
            pBar.Visibility = Visibility.Visible;
            await ViewModel.ClearFilterAsync();
            pBar.Visibility = Visibility.Hidden;
            e.Handled = true;
        }
        private async void SelectMatchingTagsButton_Click(object sender, RoutedEventArgs e) {
            pBar.Visibility = Visibility.Visible;
            await SelectAllMatchingTagsAsync();
            pBar.Visibility = Visibility.Hidden;
        }
        private void TagFilterPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ViewModel = DataContext as TagFilterPanelModel;
        }
        #endregion Event Handlers

        async Task SelectAllMatchingTagsAsync() {
            if (!tagInput.IsEmpty) {
                pBar.Visibility = Visibility.Visible;
                if (tagInput.IsPreset) {
                    await ViewModel.ResetFilterAsync(from mdl in ViewModel.RefinementTagModels.Values
                                                     where mdl.IsFullMatch
                                                     select mdl);
                } else {
                    await ViewModel.AddAllTagsToFilterAsync(from mdl in ViewModel.RefinementTagModels.Values
                                                            where mdl.IsFullMatch && !mdl.IsSelected
                                                            select mdl);
                }
                pBar.Visibility = Visibility.Hidden;
            }
        }

        private TagFilterPanelModel _model;
        /// <summary>
        /// get or set the view model backing this UI
        /// </summary>
        private TagFilterPanelModel ViewModel {
            get => _model;
            set {
                _model = value;
                DataContext = _model;
            }
        }

        /// <summary>
        /// Initialze a new user control instance.
        /// </summary>
        public TagFilterPanel() {
            InitializeComponent();
            pBar.Visibility = Visibility.Hidden;
            DataContextChanged += TagFilterPanel_DataContextChanged;
        }
    }
}
