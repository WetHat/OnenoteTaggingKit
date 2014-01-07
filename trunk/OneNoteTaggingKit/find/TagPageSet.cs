using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// The set of pages which have a specified tag in the &lt;one:Meta name="TaggingKit.PageTags" ...&gt; element
    /// </summary>
    public class TagPageSet : IKeyedItem
    {
        private HashSet<TaggedPage> _pages = new HashSet<TaggedPage>();

        private HashSet<TaggedPage> _filtered;

        /// <summary>
        /// get name of the tag.
        /// </summary>
        public string TagName { get; private set; }

        internal TagPageSet(string tagName) 
        {
            TagName = tagName;
        }

        internal ISet<TaggedPage> Pages
        {
            get
            {
                return _filtered != null ?_filtered : _pages;
            }
        }

        internal bool AddPage(TaggedPage pg)
        {
            return _pages.Add(pg);
        }

        internal void IntersectWith(IEnumerable<TaggedPage> filter)
        {
            if (filter != null)
            {
                _filtered = new HashSet<TaggedPage>(_pages);
                _filtered.IntersectWith(filter);
            }
            else
            {
                _filtered = null;
            }
        }

        #region IKeyedItem

        /// <summary>
        /// Get the tag's sortable key
        /// </summary>
        public string Key
        {
            get { return TagName; }
        }
        #endregion IKeyedItem
    }
}
