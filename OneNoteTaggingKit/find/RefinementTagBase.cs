using System.Collections.Generic;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    ///     Aa abstract decorator base class for OneNote page tags used to
    ///     filter sets of OneNote pages base on the filter rules implemented
    ///     by subclasses.
    /// </summary>
    public abstract class RefinementTagBase : ObservableObject, IKeyedItem<string>
    {
        /// <summary>
        ///     Get the set of pages having the tag represented by this object.
        /// </summary>
        public ISet<PageNode> Pages => TagWithPages.Pages;

        /// <summary>
        /// Get the tag with ists OneNote pages this refinement tag is based on.
        /// </summary>
        public TagPageSet TagWithPages { get; private set; }

        /// <summary>
        /// Get the page tag this refinement  tag is based on..
        /// </summary>
        public PageTag Tag => TagWithPages.Tag;

        /// <summary>
        ///     Initialize a refinement tag with a tag used on one or more OneNote pages.
        /// </summary>
        /// <param name="tag">
        ///     A tag ued on OneNote pages.
        /// </param>
        public RefinementTagBase (TagPageSet tag) {
            TagWithPages = tag;
        }

        /// <summary>
        /// Predicate to dermine if a filter is applied to this tag.
        /// </summary>
        public bool IsFiltered => _filteredPageCount >= 0;

        int _filteredPageCountDelta = 0;
        /// <summary>
        ///     Get the number of pages removed by applying a filter.
        /// </summary>
        public int FilteredPageCountDelta {
            get => _filteredPageCountDelta;
            protected set {
                if (_filteredPageCountDelta != value) {
                    _filteredPageCountDelta = value;
                    // only fire events if filters are applied
                    if (IsFiltered) {
                        RaisePropertyChanged();
                    }
                }
            }
        }

        int _filteredPageCount = int.MinValue;
        /// <summary>
        ///     Observable number of pages having this tag after filter application.
        /// </summary>
        public int FilteredPageCount {
            get {
                return _filteredPageCount < 0 ? Pages.Count : _filteredPageCount;
            }
            protected set {
                if (_filteredPageCount != value) {
                    _filteredPageCount = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Filter a collection of pages acording to the tag filter rule inplmenmeted
        ///     by the subclass.
        /// </summary>
        /// <param name="pages">Collection of pages to filter-</param>
        /// <returns>
        ///     Collection of pages satisfying the filter condition implemented by
        ///     subclasses.
        /// </returns>
        public abstract IEnumerable<PageNode> FilterPages(IEnumerable<PageNode> pages);

        /// <summary>
        ///     Compute the effect this tag has on a collection of pages.
        /// </summary>
        /// <remarks>
        ///     This method sets the properties
        ///     <see cref="FilteredPageCount"/> and
        ///     <see cref="FilteredPageCountDelta"/>.
        /// </remarks>
        public abstract void FilterEffect(IEnumerable<PageNode> pages);

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
                return TagWithPages.TagName.Equals(other.TagName);
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
