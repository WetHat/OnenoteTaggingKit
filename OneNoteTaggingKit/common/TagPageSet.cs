// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Collections.Generic;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// The set of pages which have a particular tag in their &lt;one:Meta
    /// name="TaggingKit.PageTags" ...&gt; meta element.
    /// </summary>
    /// <remarks>A filter can be applied to constrain the number of tagged pages <see cref="IntersectWith" /></remarks>
    public class TagPageSet : ObservableObject, IKeyedItem<string> {

        /// <summary>
        /// Set / set the page tag common to all pages in this set.
        /// </summary>
        public PageTag Tag { get; set; }

        /// <summary>
        /// Get name of the page tag.
        /// </summary>
        /// <value>Basename of the tag without type annotation.</value>
        public string TagName { get => Tag.BaseName; }

        /// <summary>
        /// Get the tag's type marker.
        /// </summary>
        /// <value>A marker string</value>
        public string TagType {
            get => Tag.TagMarker;
        }

        /// <summary>
        /// Initialize a new instance of a set of pages having the given page
        /// tag.
        /// </summary>
        /// <param name="tag">Page tag.</param>
        public TagPageSet(PageTag tag) {
            Tag = tag;
        }

        ISet<PageNode> _pages = new HashSet<PageNode>();
        /// <summary>
        /// Get the set of pages having the tag represented by this object.
        /// </summary>
        public ISet<PageNode> Pages { get => _pages; }

        int _filteredPageCount = 0;
        /// <summary>
        /// Observable property of the number of pages after filter application.
        /// </summary>
        public int FilteredPageCount {
            get {
               return _filteredPages == null ? Pages.Count :  _filteredPageCount;
            }
            set {
                if (_filteredPageCount != value) {
                    _filteredPageCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        ISet<PageNode> _filteredPages = null;
        /// <summary>
        /// Get the set of tags after filtering.
        /// </summary>
        /// <value>
        /// If no filter is applied returns the set of all pages
        /// associated wth this tag otherwise returnes the set of
        /// pages with the filter applied.
        /// </value>
        internal ISet<PageNode> FilteredPages => _filteredPages ?? _pages;

        /// <summary>
        /// Associate a page with this page.
        /// </summary>
        /// <param name="pg">Page having this tag.</param>
        /// <returns></returns>
        internal bool AddPage(PageNode pg) => Pages.Add(pg);

        /// <summary>
        /// Clear the tag filter.
        /// </summary>
        public void ClearFilter()
        {
            _filteredPages = null;
            FilteredPageCount = Pages.Count;
        }

        /// <summary>
        /// Apply an intersection filter to constrain the number of pages.
        /// </summary>
        /// <param name="filter"></param>
        internal void IntersectWith(IEnumerable<PageNode> filter)
        {
            _filteredPages = new HashSet<PageNode>(filter);
            FilteredPages.IntersectWith(Pages);
            FilteredPageCount = FilteredPages.Count;
        }

        #region IKeyedItem
        /// <summary>
        /// Determine if two tags a are equal based on their tagname.
        /// </summary>
        /// <param name="obj">the other tag for equality test</param>
        /// <returns>true if both tags are equal; false otherwise</returns>
        public override bool Equals(object obj) {
            TagPageSet other = obj as TagPageSet;

            if (other != null) {
                return TagName.Equals(other.TagName);
            }
            return false;
        }

        /// <summary>
        /// Get the tag's hashcode.
        /// </summary>
        /// <returns>hashcode</returns>
        public override int GetHashCode() => Tag.GetHashCode();


        /// <summary>
        /// Get the tag's sortable key
        /// </summary>
        public string Key => Tag.Key;

        #endregion IKeyedItem
    }
}