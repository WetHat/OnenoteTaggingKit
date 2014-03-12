using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using WetHatLab.OneNote.TaggingKit.common;

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
                
                DataContext = _model;
                _model.Tags.CollectionChanged += HandleTagCollectionChanges;
            }
        }

        #endregion IOneNotePageWindow<TagSearchModel>

        private void HandleTagCollectionChanges(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        TagSelector btn = createTagSelectorButton((TagSelectorModel)e.NewItems[i]);
                        tagsPanel.Children.Insert(i + e.NewStartingIndex, btn);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    for (int i = 0; i < e.OldItems.Count; i++)
                    {
                        tagsPanel.Children.RemoveAt(e.OldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    tagsPanel.Children.Clear();
                    break;
            }
        }

        private TagSelector createTagSelectorButton(TagSelectorModel mdl)
        {
            TagSelector s = new TagSelector()
            {
                DataContext = mdl,
                Margin = new Thickness(3,3,0,0)
            };
  
            return s;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            if (_model != null)
            {
                _model.Dispose();
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            HitHighlightedPageLink l = sender as HitHighlightedPageLink;
            if (l != null)
            {
                HitHighlightedPageLinkModel model = l.DataContext as HitHighlightedPageLinkModel;
                _model.NavigateTo(model.PageID);
                e.Handled = true;
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

        private void ScopeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pBar.Visibility = System.Windows.Visibility.Visible;
            _model.FindPagesAsync(searchComboBox.Text, () => pBar.Visibility = System.Windows.Visibility.Hidden);
            e.Handled = true;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = searchComboBox.Text;
            pBar.Visibility = System.Windows.Visibility.Visible;
            _model.FindPagesAsync(query, () => pBar.Visibility = System.Windows.Visibility.Hidden);
            searchComboBox.SelectedValue = query;
            e.Handled = true;
        }

        private void SearchComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                string query = searchComboBox.Text;
                pBar.Visibility = System.Windows.Visibility.Visible;
                _model.FindPagesAsync(query, () => pBar.Visibility = System.Windows.Visibility.Hidden);
                searchComboBox.SelectedValue = query;
            }
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            searchComboBox.Focus();
            Keyboard.Focus(searchComboBox);
        }
    }
}
