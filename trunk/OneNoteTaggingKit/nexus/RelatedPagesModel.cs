using Microsoft.Office.Interop.OneNote;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.nexus
{
    public interface IRelatedPagesModel
    {
        /// <summary>
        /// Get the title of the current page
        /// </summary>
        string CurrentPageTitle {get;}

        IEnumerable<RelatedPageLinkModel> RelatedPages {get;}
    }

    [ComVisible(false)]
    public class RelatedPagesModel : WindowViewModelBase, IRelatedPagesModel
    {
        private static readonly PropertyChangedEventArgs PAGE_TITLE = new PropertyChangedEventArgs("CurrentPageTitle");
        private static readonly PropertyChangedEventArgs RELATED_PAGES = new PropertyChangedEventArgs("RelatedPages");

        /// <summary>
        /// ID of the page currently on display
        /// </summary>
        private string _currentPageID = string.Empty;

        private TagsAndPages _taggedPagesCollection;

        /// <summary>
        /// Page currently on display
        /// </summary>
        private TaggedPage _currentPage;

        Timer _pageTimer;

        internal RelatedPagesModel(Application app) : base(app)
        {
            _taggedPagesCollection = new TagsAndPages(OneNoteApp, CurrentSchema);
        }

        #region IRelatedPagesModel
        public string CurrentPageTitle
        {
            get
            {
                return _currentPage.Title;
            }
        }
        public IEnumerable<RelatedPageLinkModel> RelatedPages
        {
            get
            {
                foreach (string tagname in _currentPage.TagNames)
                {
                    TagPageSet t;
                    if (_taggedPagesCollection.Tags.TryGetValue(tagname,out t))
                    {
                        foreach (TaggedPage p in t.FilteredPages)
                        {
                            if (!p.ID.Equals(_currentPage.ID))
                            {
                                yield return new RelatedPageLinkModel(p, t);
                            }
                        }
                    }
                }
            }
        }
        #endregion IRelatedPagesModel

        internal void TrackCurrentPage()
        {
            _pageTimer = new Timer(TrackCurrentPage, null, 0, 1000);
        }

        internal Task LoadTaggedPagesAsyc()
        {
            return Task.Run(() => _taggedPagesCollection.FindTaggedPages(string.Empty));
        }


        private void TrackCurrentPage(object state)
        {
            if (!_currentPageID.Equals(CurrentPageID))
            { // pull in new page
                _currentPageID = CurrentPageID;
                string strXml;
                OneNoteApp.GetHierarchy(_currentPageID, HierarchyScope.hsSelf, out strXml, CurrentSchema);

                XDocument result = XDocument.Parse(strXml);
                XNamespace one = result.Root.GetNamespaceOfPrefix("one");

                XElement pg = result.Descendants(one.GetName("Page")).FirstOrDefault();
                if (pg != null)
                {
                    _currentPage = new TaggedPage(pg);

                    fireNotifyPropertyChanged(Dispatcher, PAGE_TITLE);
                    fireNotifyPropertyChanged(Dispatcher, RELATED_PAGES);
                }

                // build the list of pages
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
