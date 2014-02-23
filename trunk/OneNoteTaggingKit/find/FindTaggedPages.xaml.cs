using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Interaction logic for TagSearch.xaml
    /// </summary>
    public partial class FindTaggedPages : Window, IOneNotePageWindow<FindTaggedPagesModel>
    {
        private FindTaggedPagesModel _model;

        private bool _isClearTagFilterInProgress = false;

        /// <summary>
        /// Create a new instance of the find tags window
        /// </summary>
        public FindTaggedPages()
        {
            InitializeComponent();
        }

        private void UpdateTagsHideProgress()
        {
            pBar.Visibility = System.Windows.Visibility.Hidden;
            UpdateTags();
        }

        private void UpdateTags()
        {
            foreach (TagSelector s in tagsPanel.Children)
            {
                s.UpdateTag();
            }
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
                _model.TagCollectionChanged += HandleTagCollectionChanges;
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
                        TagSelector btn = createTagSelectorButton((TagPageSet)e.NewItems[i]);
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

        private TagSelector createTagSelectorButton(TagPageSet tps)
        {
            TagSelector s = new TagSelector()
            {
                PageTag = tps
            };
            s.Margin = new Thickness(3,3,0,0);
            s.Checked += TagChecked;
            s.UnChecked += TagUnChecked;
            return s;
        }

        private void TagUnChecked(object sender, RoutedEventArgs e)
        {
            if (!_isClearTagFilterInProgress)
            {
                TagSelector selector = sender as TagSelector;
                if (selector != null)
                {
                    selector.IsChecked = false;
                    _model.RemoveTagFromFilterAsync(selector.PageTag, UpdateTags);
                }
            }
        }

        private void TagChecked(object sender, RoutedEventArgs e)
        {
            if (!_isClearTagFilterInProgress)
            {
                TagSelector selector = sender as TagSelector;
                if (selector != null)
                {
                    selector.IsChecked = true;
                    _model.AddTagToFilterAsync(selector.PageTag, UpdateTags);
                }
            }
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
           _model.NavigateTo((string)((Hyperlink)sender).Tag);
        }

        private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // turn off (un)checked events
                _isClearTagFilterInProgress = true;
                foreach (TagSelector s in tagsPanel.Children)
                {
                    s.IsChecked = false;
                }
            }
            finally
            {
                _isClearTagFilterInProgress = false;
            }
            _model.ClearTagFilterAsync(UpdateTags);
        }

        private void ScopeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pBar.Visibility = System.Windows.Visibility.Visible;
            _model.FindPagesAsync(searchComboBox.Text, UpdateTagsHideProgress);
            e.Handled = true;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string query = searchComboBox.Text;
            pBar.Visibility = System.Windows.Visibility.Visible;
            _model.FindPagesAsync(query, UpdateTagsHideProgress);
            searchComboBox.SelectedValue = query;
            e.Handled = true;
        }

        private void SearchComboBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                string query = searchComboBox.Text;
                pBar.Visibility = System.Windows.Visibility.Visible;
                _model.FindPagesAsync(query, UpdateTagsHideProgress);
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
