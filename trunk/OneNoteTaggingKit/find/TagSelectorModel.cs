using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
        /// Get the number of pages having a particular tag 
        /// </summary>
        int PageCount { get; }
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
    public class TagSelectorModel : DependencyObject, ISortableKeyedItem<TagModelKey, string>, ITagSelectorModel, IFilterableTagDataContext,INotifyPropertyChanged
    {
        internal static readonly PropertyChangedEventArgs PAGE_COUNT = new PropertyChangedEventArgs("PageCount");
        internal static readonly PropertyChangedEventArgs IS_CHECKED = new PropertyChangedEventArgs("IsChecked");
        internal static readonly PropertyChangedEventArgs VISIBILITY = new PropertyChangedEventArgs("Visibility");
        internal static readonly PropertyChangedEventArgs HIT_HIGHLIGHTED_TAGNAME = new PropertyChangedEventArgs("HitHighlightedTagName");

        private TagPageSet _tag;
        private TagModelKey _key;
        private bool _isFiltered = false;
        private IEnumerable<TextFragment> _highlightedTagName;

        /// <summary>
        /// create a new view model instance from a tag.
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
        /// create a new view model instance for a tag and an event handler.
        /// </summary>
        /// <param name="tag">tag object</param>
        /// <param name="propHandler">listerner for property changes</param>
        internal TagSelectorModel(TagPageSet tag,PropertyChangedEventHandler propHandler) : this(tag)
        {
            PropertyChanged += propHandler;
        }

        private void OnTagPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("PageCount"))
            {
                Dispatcher.Invoke(new Action(() => {
                    firePropertyChanged(PAGE_COUNT);
                    firePropertyChanged(VISIBILITY);                   
                }));
            }
        }

        private void firePropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, args);
            }
        }

        public IEnumerable<TextFragment> HitHighlightedTagName { get { return _highlightedTagName; } }

        #region ITagSelectorModel

        public TagPageSet Tag
        {
            get
            {
                return _tag;
            }
        }
        /// <summary>
        /// Number of pages having this tag
        /// </summary>
        public int PageCount
        {
            get
            {
                return _tag.Pages.Count;
            }
        }

        private bool _isChecked = false;
        /// <summary>
        /// get or set a tags selectes state
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
                return IsChecked || PageCount > 0  ? Visibility.Visible : Visibility.Collapsed;
            }
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
        /// Event to inform listerners about changes to certain properties
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

        #region IFilterableTagDataContext
        public TextSplitter Highlighter
        {
            set
            {
                _highlightedTagName = value.SplitText(TagName);
                HasHighlights = (from f in _highlightedTagName where f.IsMatch select f).FirstOrDefault().IsMatch;
                firePropertyChanged(HIT_HIGHLIGHTED_TAGNAME);
            }
        }
        public bool HasHighlights {get; private set;}

        #endregion IFilterableTagDataContext
    }
}
