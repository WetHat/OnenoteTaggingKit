using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Dialog to save a search resule to a OneNote page.
    /// </summary>
    public partial class SaveSearch : Window, IOneNotePageWindow<SaveSearchModel>
    {
        public SaveSearch() {
            InitializeComponent();
        }

        #region  IOneNotePageWindow<SaveSearchModel>
        SaveSearchModel _model;
        public SaveSearchModel ViewModel {
            get => _model;
            set {
                _model = value;
                DataContext = _model;
            }
        }
        #endregion  IOneNotePageWindow<SaveSearchModel>

        #region Events
        private void chooseSection_Click(object sender, RoutedEventArgs e) {
            string sectionid = ViewModel.OneNoteApp.SectionChooser(self,
                                                                   "Pick a section where to save the search result:");
        }

        private void self_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (_model != null) {
                _model.Dispose();
                _model = null;
            }
            Trace.Flush();
        }

        private void self_Loaded(object sender, RoutedEventArgs e) {
            pageTitle.Focus();
            Keyboard.Focus(pageTitle);
        }

        private void saveSearchBtn_Click(object sender, RoutedEventArgs e) {
            // TODO Create a new 'Saved Search' OneNote page in the selected
            // section in the background
            self.Close();
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e) {
            self.Close();
        }
        #endregion Events
    }
}
