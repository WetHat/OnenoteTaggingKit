using System;
using System.Collections.Generic;
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

namespace WetHatLab.OneNote.TaggingKit.nexus
{
    /// <summary>
    /// Interaction logic for RelatedPages.xaml
    /// </summary>
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
                _model = value; ;
            }
        }
        #endregion IOneNotePageWindow<RelatedPagesModel>
    }
}
