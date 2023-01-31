using System.Collections.Generic;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    ///     Aa abstract decorator base class for OneNote page tags used to
    ///     filter sets of OneNote pages base on specific rules implemented
    ///     by subclasses.
    /// </summary>
    /// <remarks>
    ///     Instances of this class support hashing.
    /// </remarks>
    public abstract class RefinementTagBase : ObservableObject, IKeyedItem<string>
    {
        /// <summary>
        ///     Get the set of pages having the page tag of this instance.
        /// </summary>
        public ISet<PageNode> Pages => TagWithPages.Pages;

        /// <summary>
        ///     Get the tag and its OneNote pages this refinement tag is based on.
        /// </summary>
        public TagPageSet TagWithPages { get; set; }

        /// <summary>
        /// Get the page tag this refinement  tag is based on..
        /// </summary>
        public PageTag Tag => TagWithPages.Tag;

        /// <summary>
        ///     Initialize a refinement tag with a tag used on one or more
        ///     OneNote pages.
        /// </summary>
        /// <param name="tag">
        ///     A tag with its OneNote pages.
        /// </param>
        public RefinementTagBase (TagPageSet tag) {
            TagWithPages = tag;
        }

        int _filteredPageCountDelta = 0;
        /// <summary>
        ///     Get the number of pages removed or added by applying the filter
        ///     rule implemented by the concrete subclass.
        /// </summary>
        public int FilteredPageCountDelta {
            get => _filteredPageCountDelta;
            protected set {
                if (_filteredPageCountDelta != value) {
                    _filteredPageCountDelta = value;
                    // only fire events if filters are applied
                    if (_filteredPageCount >= 0) {
                        RaisePropertyChanged();
                    }
                }
            }
        }

        int _filteredPageCount = int.MinValue;
        /// <summary>
        ///     Number of pages having this tag after filter application.
        /// </summary>
        /// <remarks>
        ///     This property raises change events.
        /// </remarks>
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
        ///     Filter a collection of pages according to the tag filter rule
        ///     inplmenmeted by the concrete subclass.
        /// </summary>
        /// <param name="pages">Collection of pages to filter-</param>
        /// <returns>
        ///     Collection of pages satisfying the filter condition implemented by
        ///     the concrete subclass.
        /// </returns>
        public abstract IEnumerable<PageNode> FilterPages(IEnumerable<PageNode> pages);

        /// <summary>
        ///     Compute the effect this tag has on a collection of pages.
        /// </summary>
        /// <remarks>
        ///     This method sets the observable properties
        ///     <see cref="FilteredPageCount"/> and
        ///     <see cref="FilteredPageCountDelta"/>.
        /// </remarks>
        public abstract void FilterEffect(IEnumerable<PageNode> pages);

        /// <summary>
        ///     Get the tag's hashcode.
        /// </summary>
        /// <returns>hashcode</returns>
        public override int GetHashCode() => Tag.GetHashCode();

        #region IKeyedItem<string>
        /// <summary>
        ///     Get the unique key of the tag.
        /// </summary>
        public string Key => Tag.Key;
        #endregion IKeyedItem<string>
    }
}
