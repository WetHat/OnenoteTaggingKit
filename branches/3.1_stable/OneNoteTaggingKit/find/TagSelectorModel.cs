using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Contact for view models supporting the <see cref="TagSelector"/> control.
    /// </summary>
    public interface ITagSelectorModel
    {
        /// <summary>
        /// Get the checked state
        /// </summary>
        bool IsChecked { get; set; }

        /// <summary>
        /// Get the visibility of the glyph showing that a tag is used for filtering
        /// </summary>
        Visibility FilterIndicatorVisibility { get; }

        string PageCountTooltip { get; }

        /// <summary>
        /// Get the number of pages having a particular tag 
        /// </summary>
        int PageCount { get; }

        /// <summary>
        /// Get the number of pages having a particular tag after
        /// application of a filter
        /// </summary>
        int FilteredPageCount { get; }
        /// <summary>
        /// Get the name of a tag
        /// </summary>
        string TagName { get; }
        /// <summary>
        /// Get the visibility of a tag in the UI.
        /// </summary>
        Visibility Visibility { get; }
    }

    /// <summary>
    /// View model to support the <see cref="TagSelector"/> control.
    /// </summary>
    /// Implements the <see cref="INotifyPropertyChanged"/> interface to update the UI after property changes.
    /// <remarks>
    /// </remarks>
    [ComVisible(false)]
    public class TagSelectorModel : DependencyObject, ISortableKeyedItem<TagModelKey, string>, ITagSelectorModel, IHighlightableTagDataContext, INotifyPropertyChanged
    {
        internal static readonly PropertyChangedEventArgs FILTER_INDICATOR_VISIBILITY = new PropertyChangedEventArgs("FilterIndicatorVisibility");
        internal static readonly PropertyChangedEventArgs FILTERED_PAGE_COUNT = new PropertyChangedEventArgs("FilteredPageCount");
        internal static readonly PropertyChangedEventArgs PAGE_COUNT_TOOLTIP = new PropertyChangedEventArgs("PageCountTooltip");
        
        internal static readonly PropertyChangedEventArgs IS_CHECKED = new PropertyChangedEventArgs("IsChecked");
        internal static readonly PropertyChangedEventArgs VISIBILITY = new PropertyChangedEventArgs("Visibility");
        internal static readonly PropertyChangedEventArgs HIT_HIGHLIGHTED_TAGNAME = new PropertyChangedEventArgs("HitHighlightedTagName");

        private TagPageSet _tag;
        private TagModelKey _key;
        private IEnumerable<TextFragment> _highlightedTagName;
        bool _isFiltered;

        /// <summary>
        /// Create a new view model instance from a tag.
        /// </summary>
        /// <param name="tag">tag object</param>
        internal TagSelectorModel(TagPageSet tag)
        {
            _tag = tag;
            _key = new TagModelKey(tag.TagName);
            tag.PropertyChanged += OnTagPropertyChanged;
            _highlightedTagName = new TextSplitter().SplitText(tag.TagName);
        }

        /// <summary>
        /// Create a new view model instance for a tag and an event handler.
        /// </summary>
        /// <param name="tag">tag object</param>
        /// <param name="propHandler">listener for property changes</param>
        internal TagSelectorModel(TagPageSet tag, PropertyChangedEventHandler propHandler)
            : this(tag)
        {
            PropertyChanged += propHandler;
        }

        private void OnTagPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == TagPageSet.FILTERED_PAGES)
            {
                Dispatcher.Invoke(() =>
                {
                    firePropertyChanged(FILTERED_PAGE_COUNT);
                    firePropertyChanged(PAGE_COUNT_TOOLTIP);
                    firePropertyChanged(VISIBILITY);
                });
            }
        }

        private void firePropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, args);
            }
        }

        /// <summary>
        /// Get the tag name with highlights
        /// </summary>
        public IEnumerable<TextFragment> HitHighlightedTagName { get { return _highlightedTagName; } }

        /// <summary>
        /// Get tag object.
        /// </summary>
        internal TagPageSet Tag
        {
            get
            {
                return _tag;
            }
        }

        #region ITagSelectorModel

        /// <summary>
        /// Get the number of pages having this tag
        /// </summary>
        public int PageCount
        {
            get
            {
                return _tag.Pages.Count;
            }
        }

        /// <summary>
        /// Get the number of pages after application of an intersection filter
        /// </summary>
        public int FilteredPageCount
        {
            get { return _tag.FilteredPages.Count; }
        }

        private bool _isChecked = false;
        /// <summary>
        /// Get or set a tags selectes state
        /// </summary>
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    firePropertyChanged(IS_CHECKED);
                    firePropertyChanged(FILTER_INDICATOR_VISIBILITY);
                    firePropertyChanged(VISIBILITY);
                }
            }
        }

        /// <summary>
        /// Get the tag's name
        /// </summary>
        public string TagName
        {
            get
            {
                return _tag.TagName;
            }
        }

        /// <summary>
        /// Get the visibility of the tag in the UI.
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                return IsChecked || (FilteredPageCount > 0 && (HasHighlights || !_isFiltered)) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Get the visibility of te filter indicator
        /// </summary>
        public Visibility FilterIndicatorVisibility
        {
            get { return IsChecked ? Visibility.Visible : Visibility.Collapsed; }
        }

        /// <summary>
        /// Get the tooltip of the page count
        /// </summary>
        public string PageCountTooltip
        {
            get { return String.Format(Properties.Resources.TagSelector_PageCount_Tooltip, FilteredPageCount,PageCount,TagName); }
        }

        #endregion ITagSelectorModel

        #region ISortableKeyedItem<TagModelKey,string>
        /// <summary>
        /// Get the view model's sort key
        /// </summary>
        public TagModelKey SortKey
        {
            get { return _key; }
        }

        /// <summary>
        /// Get the view model's unique key
        /// </summary>
        public string Key
        {
            get { return TagName; }
        }
        #endregion ISortableKeyedItem<TagModelKey,string>

        #region INotifyPropertyChanged
        /// <summary>
        /// Event to notify listeners about changes to properties in this class
        /// </summary>
        /// <remarks>
        /// Currently notification for following properties are emitted:
        /// <list type="bullet">
        /// <item>PageCount</item>
        /// <item>IsChecked</item>
        /// <item>Visibility</item>
        /// </list>
        /// </remarks>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion INotifyPropertyChanged

        #region IHighlightableTagDataContext
        /// <summary>
        /// Set the object which is used to generate the highlighted text.
        /// </summary>
        public TextSplitter Highlighter
        {
            set
            {
                _highlightedTagName = value.SplitText(TagName);
                _isFiltered = value.SplitPattern != null;
                HasHighlights = (from f in _highlightedTagName where f.IsMatch select f).FirstOrDefault().IsMatch;
                firePropertyChanged(HIT_HIGHLIGHTED_TAGNAME);
                firePropertyChanged(VISIBILITY);
            }
        }
        /// <summary>
        /// Check if the tag name has highlights.
        /// </summary>
        public bool HasHighlights { get; private set; }

        #endregion IHighlightableTagDataContext

    }
}
