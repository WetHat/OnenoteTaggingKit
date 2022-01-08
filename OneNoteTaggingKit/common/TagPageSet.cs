// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// The set of pages which have a specified tag in their &lt;one:Meta
    /// name="TaggingKit.PageTags" ...&gt; meta element.
    /// </summary>
    /// <remarks>A filter can be applied to constrain the number of tagged pages <see cref="IntersectWith" /></remarks>
    public class TagPageSet : ObservableObject, IKeyedItem<string>
    {
        /// <summary>
        /// Utility function to get base name and type of a page tag.
        /// </summary>
        /// <param name="tagname">Tag name with type postfix</param>
        /// <returns>Tag basename and type.</returns>
        internal static Tuple<string,string> ParseTagName(string tagname) {
            string basename;

            if (tagname.EndsWith(Properties.Settings.Default.ImportOneNoteTagMarker)) {
                basename = tagname.Substring(0, tagname.Length - Properties.Settings.Default.ImportOneNoteTagMarker.Length);
            } else if (tagname.EndsWith(Properties.Settings.Default.ImportHashtagMarker)) {
                basename = tagname.Substring(0, tagname.Length - Properties.Settings.Default.ImportHashtagMarker.Length);
            } else {
                basename = tagname;
            }

            return new Tuple<string, string>(
                basename,
                basename.Length == tagname.Length ? string.Empty : tagname.Substring(basename.Length));
        }
        /// <summary>
        /// Get name of the tag.
        /// </summary>
        /// <value>Name of the tag without type annotation.</value>
        public string TagName { get; }

        string _tagType = string.Empty;
        /// <summary>
        /// The tag's type.
        /// </summary>
        /// <value>A marker emoji for imported tags; Empty string otherwise</value>
        public string TagType {
            get => _tagType;
            set {
                if (_tagType != value
                    && (Properties.Settings.Default.ImportOneNoteTagMarker.Equals(value)
                        && (!Properties.Settings.Default.ImportOneNoteTagMarker.Equals(_tagType)
                             || (!_tagType.Equals(value) && !string.IsNullOrWhiteSpace(value))))) {
                    _tagType = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Determine if a filter has been applied to the pages with this
        /// tag.
        /// </summary>
        public bool IsFiltered => FilteredPageCount != Pages.Count;

        /// <summary>
        /// Initialize a new instance of a page tag.
        /// </summary>
        /// <param name="parsedName">
        ///     The parsed tag name consisting of base name
        ///     and type as returned by  <see cref="ParseTagName(string)"/>
        /// </param>
        public TagPageSet(Tuple<string, string> parsedName) {
            TagName = parsedName.Item1;
            TagType = parsedName.Item2;
        }

        /// <summary>
        /// Create a new instance object representing pages having a specific tag.
        /// </summary>
        /// <param name="tagName">Name of tag with type annotation.</param>
        public TagPageSet(string tagName) : this(ParseTagName(tagName)) {
        }

        ISet<TaggedPage> _pages = new HashSet<TaggedPage>();
        /// <summary>
        /// Get the set of pages having the tag represented by this object.
        /// </summary>
        public ISet<TaggedPage> Pages { get => _pages; }

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

        ISet<TaggedPage> _filteredPages = null;
        /// <summary>
        /// Get the set of tags after filtering.
        /// </summary>
        /// <value>
        /// If no filter is applied returns the set of all pages
        /// associated wth this tag otherwise returnes the set of
        /// pages with the filter applied.
        /// </value>
        internal ISet<TaggedPage> FilteredPages => _filteredPages ?? _pages;

        /// <summary>
        /// Associate a page with this page.
        /// </summary>
        /// <param name="pg">Page having this tag.</param>
        /// <returns></returns>
        internal bool AddPage(TaggedPage pg) => Pages.Add(pg);

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
        internal void IntersectWith(IEnumerable<TaggedPage> filter)
        {
            _filteredPages = new HashSet<TaggedPage>(filter);
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
        public override int GetHashCode() {
            return TagName.GetHashCode();
        }

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