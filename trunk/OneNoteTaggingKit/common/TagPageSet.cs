using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// The set of pages which have a specified tag in the &lt;one:Meta name="TaggingKit.PageTags" ...&gt; element
    /// </summary>
    public class TagPageSet : IKeyedItem<string>, INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs PAGE_COUNT = new PropertyChangedEventArgs("PageCount");

        private HashSet<TaggedPage> _pages = new HashSet<TaggedPage>();

        private HashSet<TaggedPage> _filteredPages;

        /// <summary>
        /// get name of the tag.
        /// </summary>
        public string TagName { get; private set; }

        private void firePropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, args);
            }
        }

        internal TagPageSet(string tagName) 
        {
            TagName = tagName;
        }

        internal ISet<TaggedPage> Pages
        {
            get
            {
                return _filteredPages != null ?_filteredPages : _pages;
            }
        }

        internal int PageCount
        {
            get
            {
                return Pages.Count;
            }
        }

        internal bool AddPage(TaggedPage pg)
        {
            bool added = _pages.Add(pg);

            if (added)
            {
                firePropertyChanged(PAGE_COUNT);
            }
            return added;
        }

        internal bool RemovePage(TaggedPage pg)
        {
            bool removed = _pages.Remove(pg);

            if (removed)
            {
                firePropertyChanged(PAGE_COUNT);
            }
            return removed;
        }

        internal void ClearFilter()
        {
            if (_filteredPages != null)
            {
                _filteredPages = null;
                firePropertyChanged(PAGE_COUNT);
            }
        }
        internal void IntersectWith(IEnumerable<TaggedPage> filter)
        {
            int countBefore = PageCount;
            _filteredPages = new HashSet<TaggedPage>(_pages);
            _filteredPages.IntersectWith(filter);

            if (countBefore != PageCount)
            {
                firePropertyChanged(PAGE_COUNT);
            }
        }

        public override bool Equals(object obj)
        {
            TagPageSet other = obj as TagPageSet;

            if (other != null)
            {
                return TagName.Equals(other.TagName);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return TagName.GetHashCode();
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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
