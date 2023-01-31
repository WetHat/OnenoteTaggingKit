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
        void TagInputBox_Input(object sender, TagInputEventArgs e) {
            ViewModel.RefinementTagModels.Highlighter = tagInput.IsEmpty ? new TextSplitter() : new TextSplitter(e.Tags);
            ViewModel.UpdateRefinementTagsPanelHeader();
            if (e.TagInputComplete && !tagInput.IsEmpty) {
                // select all tags with exact full matches
                SelectAllMatchingTags();
            } else if (e.Action == TagInputEventArgs.TaggingAction.Clear) {
                ClearSelectionButton_Click(sender, e);
            }
        }
        private  void ClearSelectionButton_Click(object sender, RoutedEventArgs e) {
            ViewModel.ClearFilter();
            e.Handled = true;
        }
        private async void SelectMatchingTagsButton_Click(object sender, RoutedEventArgs e) {
            SelectAllMatchingTags();
        }
        private void TagFilterPanel_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ViewModel = DataContext as TagFilterPanelModel;
        }
        #endregion Event Handlers

        void SelectAllMatchingTags() {
            if (!tagInput.IsEmpty) {
                if (tagInput.IsPreset) {
                    ViewModel.ResetFilter(from mdl in ViewModel.RefinementTagModels.Values
                                          where mdl.IsFullMatch
                                          select mdl);
                } else {
                    ViewModel.AddAllTagsToFilter(from mdl in ViewModel.RefinementTagModels.Values
                                                    where mdl.IsFullMatch && !mdl.IsSelected
                                                    select mdl);
                }
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
            }
        }

        /// <summary>
        /// Initialze a new user control instance.
        /// </summary>
        public TagFilterPanel() {
            InitializeComponent();
            DataContextChanged += TagFilterPanel_DataContextChanged;
        }
    }
}
