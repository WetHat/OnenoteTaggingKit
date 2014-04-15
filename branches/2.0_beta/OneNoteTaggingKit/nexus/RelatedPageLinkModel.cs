using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.nexus
{
    public interface IRelatedPageLinkModel
    {
       string PageTitle { get; }

       IEnumerable<Tuple<string,bool>> HighlightedTags { get; }
    }

    public class RelatedPageLinkSortKey : IComparable<RelatedPageLinkSortKey>
    {
        string _pageTitle;
        int _matchingTags;
        string _pageID;

        internal RelatedPageLinkSortKey(string pageTitle, string pageID, int matchingTagCount)
        {
            _pageTitle = pageTitle;
            _pageID = pageID;
            _matchingTags = matchingTagCount;
        }

        #region IComparable<RelatedPageLinkSortKey>
        public int CompareTo(RelatedPageLinkSortKey other)
        {
            int result = _matchingTags.CompareTo(other._matchingTags);
            if (result == 0)
            {
                result = string.Compare(_pageTitle, other._pageTitle, true);
            }
            if (result == 0)
            {
                result = _pageID.CompareTo(other._pageID);
            }
            return result;
        }
        #endregion IComparable<RelatedPageLinkSortKey>
    }

    public class RelatedPageLinkModel : IRelatedPageLinkModel, ISortableKeyedItem<RelatedPageLinkSortKey, string>
    {
        TaggedPage _page;
        RelatedPageLinkSortKey _sortKey;

        IList<Tuple<string, bool>> _highlightedTags;

        internal RelatedPageLinkModel(TaggedPage page, IDictionary<string,TagPageSet> pageTags)
        {
            _page = page;
            IEnumerable<string> sortedTags = from t in page.Tags
                                             orderby t.Key ascending
                                             select t.TagName;

            _highlightedTags = new List<Tuple<string, bool>>(page.Tags.Count);
            int matches = 0;
            foreach (var tagname in sortedTags)
            {
                bool match = pageTags.ContainsKey(tagname);
                _highlightedTags.Add(new Tuple<string,bool>(tagname,match));
                if (match)
                {
                    matches++;
                }
            }

            _sortKey = new RelatedPageLinkSortKey(PageTitle, Key, matches);
        }

        #region IRelatedPageLinkModel
        public string PageTitle
        {
            get { return _page.Title; }
        }

        public IEnumerable<Tuple<string, bool>> HighlightedTags
        {
            get { return _highlightedTags; }
        }

        #endregion IRelatedPageLinkModel

        #region  ISortableKeyedItem<RelatedPageLinkSortKey, string>
        public RelatedPageLinkSortKey SortKey
        {
            get { return _sortKey; }
        }

        public string Key
        {
            get { return _page.ID; }
        }

        #endregion  ISortableKeyedItem<RelatedPageLinkSortKey, string>

    }
}
