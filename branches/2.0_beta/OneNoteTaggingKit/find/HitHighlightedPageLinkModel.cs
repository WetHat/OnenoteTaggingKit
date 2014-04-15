using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.find
{
    public class HitHighlightedPageLinkKey : IComparable<HitHighlightedPageLinkKey>, IEquatable<HitHighlightedPageLinkKey>
    {
        public string PageID { get; private set; }
        private string _title;
        private int _hits;

        internal int HitCount
        {
            set
            {
                _hits = value;
            }
        }
        internal HitHighlightedPageLinkKey(string pageTitle, string pageId)
        {
            _title = pageTitle.ToLower();
            PageID = pageId;
        }

        #region IComparable<HitHighlightedPageLinkKey>
        public int CompareTo(HitHighlightedPageLinkKey other)
        {
            int retval = 0;
            if (_hits < other._hits)
            {
                retval = 1;
            }
            else if (_hits > other._hits)
            {
                retval = -1;
            }

            if (retval == 0)
            {
               retval = _title.CompareTo(other._title); 
            }

            if (retval == 0)
            {
                retval = PageID.CompareTo(other.PageID);
            }

            return retval;
        }
        #endregion IComparable<HitHighlightedPageLinkKey>

        #region IEquatable<HitHighlightedPageLinkKey>
        public bool Equals(HitHighlightedPageLinkKey other)
        {
            return PageID.Equals(other.PageID);
        }
        #endregion IEquatable<HitHighlightedPageLinkKey>
    }

    public interface IHitHighlightedPageLinkModel
    {
        string PageTitle { get;}
        MatchCollection Matches { get;}
    }

    public class HitHighlightedPageLinkModel : HitHighlightedPageLinkKey, ISortableKeyedItem<HitHighlightedPageLinkKey,string>, IHitHighlightedPageLinkModel
    {
        private TaggedPage _page;

        private MatchCollection _matches;

        internal HitHighlightedPageLinkModel(TaggedPage tp, Regex pattern) : base(tp.Title,tp.ID)
        {
            _page = tp;
            if (pattern != null)
            {
                _matches = pattern.Matches(PageTitle);
            }
            HitCount = Matches != null ? Matches.Count : 0;
            
        }

        #region IHitHighlightedPageLinkModel
        public MatchCollection Matches
        {
            get
            {
                return _matches;
            }
        }
        public string PageTitle
        {
            get
            {
                return _page.Title;
            }
        }
        #endregion

        #region ISortableKeyedItem<HitHighlightedPageLinkKey,string>
        public string Key
        {
            get { return PageID; }
        }

        public HitHighlightedPageLinkKey SortKey
        {
            get { return this;  }
        }

        #endregion ISortableKeyedItem<HitHighlightedPageLinkKey,string>
    }
}
