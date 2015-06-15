using System.Runtime.InteropServices;
using System.Windows;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.nexus
{
    /// <summary>
    /// Interaction logic for RelatedPages.xaml
    /// </summary>
    [ComVisible(false)]
    public partial class RelatedPages : Window, IOneNotePageWindow<RelatedPagesModel>
    {
        private RelatedPagesModel _model;
        public RelatedPages()
        {
            InitializeComponent();
        }

        #region IOneNotePageWindow<RelatedPagesModel>
        public RelatedPagesModel ViewModel
        {
            get
            {
                return _model;  
            }
            set
            {
                _model = value;
                DataContext = _model;
                _model.TrackCurrentPage();
                _model.LoadTaggedPagesAsyc();
            }
        }
        #endregion IOneNotePageWindow<RelatedPagesModel>

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_model != null)
            {
                _model.Dispose();
                _model = null;
            }
        }

    }
}
