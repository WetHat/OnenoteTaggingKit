using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.find
{
    public class TagFilterPanelDesignerModel : TagFilterPanelModel
    {
       static TagFilterBase sMakeFilter() {
            TagsAndPages tagsandpages = new TagsAndPages(null);
            var filter = new WithAllTagsFilter(tagsandpages);
            // populate data
            string strXml = "<?xml version=\"1.0\"?><one:Notebooks xmlns:one=\"http://schemas.microsoft.com/office/onenote/2013/onenote\"><one:Notebook name=\"My Notebook\" nickname=\"My Notebook\" ID=\"{415965A3-1D59-4A88-A52D-0DB4F457744F}{1}{B0}\" path=\"https://foo.com\" lastModifiedTime=\"2014-03-04T08:09:18.000Z\" color=\"#8AA8E4\"><one:SectionGroup name=\"Inventar\" ID=\"{DB4E1AB9-7E4B-49A9-9E83-9E2161B856BC}{1}{B0}\" path=\"https://d.docs.live.net/3e9574def72409d3/Documents/OneNote Notebooks/My Notebook/Inventar/\" lastModifiedTime=\"2014-02-08T12:09:52.000Z\"><one:Section name=\"Hardware\" ID=\"{AEF7AC70-1CDC-07EC-3986-2749783EE0E6}{1}{B0}\" path=\"https://d.docs.live.net/3e9574def72409d3/Documents/OneNote Notebooks/My Notebook/Inventar/Hardware.one\" lastModifiedTime=\"2014-02-08T12:09:07.000Z\" color=\"#91BAAE\"><one:Page ID=\"{AEF7AC70-1CDC-07EC-3986-2749783EE0E6}{1}{E19573021772277977525420158707822091902171211}\" name=\"Cool Computer Names\" dateTime=\"2011-07-23T19:28:19.000Z\" lastModifiedTime=\"2013-11-30T08:08:05.000Z\" pageLevel=\"1\"/></one:Section></one:SectionGroup></one:Notebook><one:Notebook name=\"WetHat Lab Notes\" nickname=\"WetHat Lab Notes\" ID=\"{57CDF8C2-8864-41CF-9DED-42498F189B40}{1}{B0}\" path=\"https://d.docs.live.net/3e9574def72409d3/Documents/OneNote Notebooks/WetHat Lab Notes/\" lastModifiedTime=\"2014-03-04T17:34:49.000Z\" color=\"#ADE792\" isCurrentlyViewed=\"true\"><one:SectionGroup name=\"OneNote_RecycleBin\" ID=\"{5AB614F0-623C-4EA5-B1A0-D832BC9E372C}{1}{B0}\" path=\"https://d.docs.live.net/3e9574def72409d3/Documents/OneNote Notebooks/WetHat Lab Notes/OneNote_RecycleBin/\" lastModifiedTime=\"2014-03-04T10:48:38.000Z\" isRecycleBin=\"true\"><one:Section name=\"Deleted FilteredPages\" ID=\"{42B40A97-31D1-0076-317C-E8DACFBDFA7B}{1}{B0}\" path=\"https://d.docs.live.net/3e9574def72409d3/Documents/OneNote Notebooks/WetHat Lab Notes/OneNote_RecycleBin/OneNote_DeletedPages.one\" lastModifiedTime=\"2014-03-04T10:48:38.000Z\" color=\"#E1E1E1\" isInRecycleBin=\"true\" isDeletedPages=\"true\"><one:Page ID=\"{42B40A97-31D1-0076-317C-E8DACFBDFA7B}{1}{E1947215228855425188431963526840848852112751}\" name=\"Manage Tags\" dateTime=\"2014-01-08T14:56:56.000Z\" lastModifiedTime=\"2014-01-08T18:45:50.000Z\" pageLevel=\"3\" isInRecycleBin=\"true\"/></one:Section></one:SectionGroup></one:Notebook></one:Notebooks>";
            tagsandpages.BuildTagSet(XDocument.Parse(strXml), false);
            return filter;
        }

        public TagFilterPanelDesignerModel() :base(null,sMakeFilter()) {
        }
    }
}
