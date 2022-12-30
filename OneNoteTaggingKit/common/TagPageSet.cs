// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Collections.Generic;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// The set of pages which have a particular tag in their &lt;one:Meta
    /// name="TaggingKit.PageTags" ...&gt; meta element.
    /// </summary>
    /// <remarks>Instances of this class can be collected in hashtables.</remarks>
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

        /// <summary>
        /// Associate a page with this page.
        /// </summary>
        /// <param name="pg">Page having this tag.</param>
        /// <returns></returns>
        internal bool AddPage(PageNode pg) => Pages.Add(pg);
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

        #region IKeyedItem
        /// <summary>
        /// Get the tag's sortable key
        /// </summary>
        public string Key => Tag.Key;

        #endregion IKeyedItem
    }
}