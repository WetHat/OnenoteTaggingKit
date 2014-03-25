using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using System.Linq;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Designer Suppoert for the <see cref="FindTaggedPages"/> window.
    /// </summary>
    public class FindTaggedPagesDesignerModel : ITagSearchModel
    {
        private List<TagSearchScope> _scopes;
        private TagSearchScope _selectedScope;
        private ObservableSortedList<HitHighlightedPageLinkKey,string, HitHighlightedPageLinkModel> _pages = new  ObservableSortedList<HitHighlightedPageLinkKey,string, HitHighlightedPageLinkModel>();
        private ObservableSortedList<TagModelKey, string, TagSelectorModel> _tags = new ObservableSortedList<TagModelKey, string, TagSelectorModel>();

        private string _strXml = "<?xml version=\"1.0\"?><one:Notebooks xmlns:one=\"http://schemas.microsoft.com/office/onenote/2013/onenote\"><one:Notebook name=\"My Notebook\" nickname=\"My Notebook\" ID=\"{415965A3-1D59-4A88-A52D-0DB4F457744F}{1}{B0}\" path=\"https://d.docs.live.net/3e9574def72409d3/Documents/OneNote Notebooks/My Notebook/\" lastModifiedTime=\"2014-03-04T08:09:18.000Z\" color=\"#8AA8E4\"><one:SectionGroup name=\"Inventar\" ID=\"{DB4E1AB9-7E4B-49A9-9E83-9E2161B856BC}{1}{B0}\" path=\"https://d.docs.live.net/3e9574def72409d3/Documents/OneNote Notebooks/My Notebook/Inventar/\" lastModifiedTime=\"2014-02-08T12:09:52.000Z\"><one:Section name=\"Hardware\" ID=\"{AEF7AC70-1CDC-07EC-3986-2749783EE0E6}{1}{B0}\" path=\"https://d.docs.live.net/3e9574def72409d3/Documents/OneNote Notebooks/My Notebook/Inventar/Hardware.one\" lastModifiedTime=\"2014-02-08T12:09:07.000Z\" color=\"#91BAAE\"><one:Page ID=\"{AEF7AC70-1CDC-07EC-3986-2749783EE0E6}{1}{E19573021772277977525420158707822091902171211}\" name=\"Cool Computer Names\" dateTime=\"2011-07-23T19:28:19.000Z\" lastModifiedTime=\"2013-11-30T08:08:05.000Z\" pageLevel=\"1\"/></one:Section></one:SectionGroup></one:Notebook><one:Notebook name=\"WetHat Lab Notes\" nickname=\"WetHat Lab Notes\" ID=\"{57CDF8C2-8864-41CF-9DED-42498F189B40}{1}{B0}\" path=\"https://d.docs.live.net/3e9574def72409d3/Documents/OneNote Notebooks/WetHat Lab Notes/\" lastModifiedTime=\"2014-03-04T17:34:49.000Z\" color=\"#ADE792\" isCurrentlyViewed=\"true\"><one:SectionGroup name=\"OneNote_RecycleBin\" ID=\"{5AB614F0-623C-4EA5-B1A0-D832BC9E372C}{1}{B0}\" path=\"https://d.docs.live.net/3e9574def72409d3/Documents/OneNote Notebooks/WetHat Lab Notes/OneNote_RecycleBin/\" lastModifiedTime=\"2014-03-04T10:48:38.000Z\" isRecycleBin=\"true\"><one:Section name=\"Deleted Pages\" ID=\"{42B40A97-31D1-0076-317C-E8DACFBDFA7B}{1}{B0}\" path=\"https://d.docs.live.net/3e9574def72409d3/Documents/OneNote Notebooks/WetHat Lab Notes/OneNote_RecycleBin/OneNote_DeletedPages.one\" lastModifiedTime=\"2014-03-04T10:48:38.000Z\" color=\"#E1E1E1\" isInRecycleBin=\"true\" isDeletedPages=\"true\"><one:Page ID=\"{42B40A97-31D1-0076-317C-E8DACFBDFA7B}{1}{E1947215228855425188431963526840848852112751}\" name=\"Manage Tags\" dateTime=\"2014-01-08T14:56:56.000Z\" lastModifiedTime=\"2014-01-08T18:45:50.000Z\" pageLevel=\"3\" isInRecycleBin=\"true\"/></one:Section></one:SectionGroup></one:Notebook></one:Notebooks>";
       
        public FindTaggedPagesDesignerModel()
        {
             _scopes = new List<TagSearchScope> {
                    new TagSearchScope()
                    {
                        Scope = SearchScope.AllNotebooks,
                        ScopeLabel = "All Notebooks"
                    },
                    new TagSearchScope()
                    {
                        Scope = SearchScope.Notebook,
                        ScopeLabel = "Notebooks"
                    }
                };

             _selectedScope = _scopes[0];

             TagCollection c = new TagCollection(null, Microsoft.Office.Interop.OneNote.XMLSchema.xs2013);
            c.parseOneNoteFindResult(_strXml);

            Regex pattern = new Regex("Cool",RegexOptions.IgnoreCase);

            _pages.AddAll(from TaggedPage tp in c.Pages.Values select new HitHighlightedPageLinkModel(tp,pattern));

            _tags.AddAll(from t in c.Tags.Values select new TagSelectorModel(t));
        }

        public IList<TagSearchScope> Scopes
        {
            get
            {
                return _scopes;
            }
        }

        public TagSearchScope SelectedScope
        {
            get
            {
                return _selectedScope;
            }
            set
            {
                _selectedScope = value;
            }
        }

        public ObservableSortedList<HitHighlightedPageLinkKey, string, HitHighlightedPageLinkModel> Pages
        {
            get { return _pages; }
        }

        public ObservableSortedList<TagModelKey, string, TagSelectorModel> Tags
        {
            get { return _tags;}
        }


        public int PageCount
        {
            get { return _pages.Count; }
        }


        public int TagCount
        {
            get { return _tags.Count; }
        }
    }
}
