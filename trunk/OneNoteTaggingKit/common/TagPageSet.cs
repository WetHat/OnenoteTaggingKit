using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// The set of pages which have a specified tag in their &lt;one:Meta name="TaggingKit.PageTags" ...&gt; meta element.
    /// </summary>
    /// <remarks>A filter can be applied to constrain the number of tagged pages
    /// <see cref="IntersectWith"/>
    /// </remarks>
    public class TagPageSet : IKeyedItem<string>, INotifyPropertyChanged
    {
        internal static readonly PropertyChangedEventArgs PAGES = new PropertyChangedEventArgs("Pages");
        internal static readonly PropertyChangedEventArgs FILTERED_PAGES = new PropertyChangedEventArgs("FilteredPages");

        private readonly HashSet<TaggedPage> _pages = new HashSet<TaggedPage>();

        private HashSet<TaggedPage> _filteredPages;

        /// <summary>
        /// Get name of the tag.
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
                return  _pages;
            }
        }

        internal ISet<TaggedPage> FilteredPages
        {
            get
            {
                return _filteredPages ?? _pages;
            }
        }

        internal bool AddPage(TaggedPage pg)
        {
            bool added = _pages.Add(pg);

            if (added)
            {
                firePropertyChanged(PAGES);
            }
            return added;
        }

        internal bool RemovePage(TaggedPage pg)
        {
            bool removed = _pages.Remove(pg);

            if (removed)
            {
                firePropertyChanged(PAGES);
            }
            return removed;
        }

        internal void ClearFilter()
        {
            if (_filteredPages != null)
            {
                _filteredPages = null;
                firePropertyChanged(FILTERED_PAGES);
            }
        }
        /// <summary>
        /// Apply an intersection filter to constrain the number of pages.
        /// </summary>
        /// <param name="filter"></param>
        internal void IntersectWith(IEnumerable<TaggedPage> filter)
        {
            int countBefore = FilteredPages.Count;
            _filteredPages = new HashSet<TaggedPage>(_pages);
            _filteredPages.IntersectWith(filter);

            if (countBefore != FilteredPages.Count)
            {
                firePropertyChanged(FILTERED_PAGES);
            }
        }

        /// <summary>
        /// Determine if two tags a are equal
        /// </summary>
        /// <param name="obj">the other tag for equality test</param>
        /// <returns>true if both tags are equal; false otherwise</returns>
        public override bool Equals(object obj)
        {
            TagPageSet other = obj as TagPageSet;

            if (other != null)
            {
                return TagName.Equals(other.TagName);
            }
            return false;
        }

        /// <summary>
        /// Get the tag's hashcode.
        /// </summary>
        /// <returns>hashcode</returns>
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
        /// <summary>
        /// Event to notify listeners about changes of properties.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
