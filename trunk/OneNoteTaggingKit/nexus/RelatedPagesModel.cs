using Microsoft.Office.Interop.OneNote;
using System.Threading;
using System.Xml.Linq;
using System.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using System.ComponentModel;
using System.Threading.Tasks;

namespace WetHatLab.OneNote.TaggingKit.nexus
{
    public interface IRelatedPagesModel
    {
        /// <summary>
        /// Get the title of the current page
        /// </summary>
        string CurrentPageTitle {get;}
    }

    public class RelatedPagesModel : WindowViewModelBase, IRelatedPagesModel
    {
        private static readonly PropertyChangedEventArgs PAGE_TITLE = new PropertyChangedEventArgs("CurrentPageTitle");

        /// <summary>
        /// ID of the page currently on display
        /// </summary>
        private string _currentPageID = string.Empty;

        private AggregatedPageCollection _taggedPagesCollection;

        /// <summary>
        /// Page currently on display
        /// </summary>
        private TaggedPage _currentPage;

        Timer _pageTimer;

        internal RelatedPagesModel(Application app, XMLSchema schema) : base(app,schema)
        {
            _taggedPagesCollection = new AggregatedPageCollection(OneNoteApp, OneNotePageSchema);
        }

        #region IRelatedPagesModel
        public string CurrentPageTitle
        {
            get
            {
                return _currentPage.Title;
            }
        }
        #endregion IRelatedPagesModel

        internal void TrackCurrentPage()
        {
            _pageTimer = new Timer(TrackCurrentPage, null, 0, 1000);
        }

        internal Task LoadTaggedPagesAsyc()
        {
            return Task.Run(() => _taggedPagesCollection.FindPages(string.Empty));
        }

        private void TrackCurrentPage(object state)
        {
            if (!_currentPageID.Equals(CurrentPageID))
            { // pull in new page
                _currentPageID = CurrentPageID;
                string strXml;
                OneNoteApp.GetHierarchy(_currentPageID, HierarchyScope.hsSelf, out strXml, OneNotePageSchema);

                XDocument result = XDocument.Parse(strXml);
                XNamespace one = result.Root.GetNamespaceOfPrefix("one");

                XElement pg = result.Descendants(one.GetName("Page")).FirstOrDefault();
                if (pg != null)
                {
                    _currentPage = new TaggedPage(pg);

                    fireNotifyPropertyChanged(Dispatcher, PAGE_TITLE);
                }
            }
        }

        public override void Dispose()
        {
            if (_pageTimer != null)
            {
                _pageTimer.Dispose();
                _pageTimer = null;
            }
            base.Dispose();
        }
    }
}
