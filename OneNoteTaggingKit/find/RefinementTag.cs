using System.Collections.Generic;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    ///     A decorator for OneNote page tags used to filter sets of OneNote pages.
    /// </summary>
    public class RefinementTag : ObservableObject, IKeyedItem<string>
    {
        /// <summary>
        ///     Get the tag appearing on a set of OneNote pages.
        /// </summary>
        public TagPageSet Tag { get; private set; }

        /// <summary>
        ///     Get the set of pages having the tag represented by this object.
        /// </summary>
        public ISet<PageNode> Pages { get => Tag.Pages; }

        /// <summary>
        ///     Get name of the page tag.
        /// </summary>
        /// <value>
        ///     Basename of the tag without type annotation.
        /// </value>
        public string TagName { get => Tag.TagName; }

        /// <summary>
        ///     Initialize a refinement tag with a tag used on one or more OneNote pages.
        /// </summary>
        /// <param name="tag">
        ///     A tag ued on OneNote pages.
        /// </param>
        public RefinementTag (TagPageSet tag) {
            Tag = tag;
        }

        int _filteredPageCount = -1;
        /// <summary>
        ///     Observable number of pages having this tag after filter application.
        /// </summary>
        public int FilteredPageCount {
            get {
                return _filteredPageCount < 0 ? Pages.Count : _filteredPageCount;
            }
            set {
                if (_filteredPageCount != value) {
                    _filteredPageCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Determine how many pages having this tag match the filter.
        /// </summary>
        /// <param name="filter">A set of OneNote pages.</param>
        internal void IntersectWith(IEnumerable<PageNode> filter) {
            var filterset = new HashSet<PageNode>(filter);
            filterset.IntersectWith(Pages);
            FilteredPageCount = filterset.Count;
        }

        /// <summary>
        ///     Clear the tag filter.
        /// </summary>
        public void ClearFilter() {
            FilteredPageCount = -1;
        }

        /// <summary>
        ///     Determine if two tags a are equal based on their tagname.
        /// </summary>
        /// <param name="obj">the other tag for equality test</param>
        /// <returns>
        ///     true if both tags are equal; false otherwise.
        ///     </returns>
        public override bool Equals(object obj) {
            TagPageSet other = obj as TagPageSet;

            if (other != null) {
                return Tag.TagName.Equals(other.TagName);
            }
            return false;
        }

        /// <summary>
        /// Get the tag's hashcode.
        /// </summary>
        /// <returns>hashcode</returns>
        public override int GetHashCode() => Tag.GetHashCode();

        #region IKeyedItem<string>
        /// <summary>
        /// Get the unique key of the tag.
        /// </summary>
        public string Key => Tag.Key;
        #endregion IKeyedItem<string>
    }
}
