using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    public struct TextFragment
    {
        public string Text { get; private set; }
        public Brush HighLightColor { get; private set; }

        public TextFragment(string text, Brush highlightColor): this()
        {
            Text = text;
            HighLightColor = highlightColor;
        }
    }

    /// <summary>
    /// Contact for view models supporting the <see cref="SelectableTag"/> control.
    /// </summary>
    public interface ISelectableTagModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Get the number of pages having a particular tag 
        /// </summary>
        int PageCount { get; }

        /// <summary>
        /// Get the visibility of a tag in the UI.
        /// </summary>
        Visibility Visibility { get; }

        /// <summary>
        /// Get the tag name with hit highlighting
        /// </summary>
        IEnumerable<TextFragment> HitHighlightedTagName { get; }
    }

    /// <summary>
    /// View model to support the <see cref="SelectableTag"/> control.
    /// </summary>
    /// Implements the <see cref="INotifyPropertyChanged"/> interface to update the UI after property changes.
    /// <remarks>
    /// </remarks>
    public class SelectableTagModel : DependencyObject, ISortableKeyedItem<TagModelKey, string>, ISelectableTagModel
    {
        private static readonly PropertyChangedEventArgs PAGE_COUNT = new PropertyChangedEventArgs("PageCount");
        private static readonly PropertyChangedEventArgs VISIBILITY = new PropertyChangedEventArgs("Visibility");
        private static readonly PropertyChangedEventArgs HIT_HIGHLIGHTED_TAGNAME = new PropertyChangedEventArgs("HitHighlightedTagName");

        private TagModelKey _key;
        private TagPageSet _tag;
        private IEnumerable<TextFragment> _highlightedTagName;

        /// <summary>
        /// create a new view model instance from a tag.
        /// </summary>
        /// <param name="tag">tag object</param>
        internal SelectableTagModel(TagPageSet tag)
        {
            _tag = tag;
            _key = new TagModelKey(tag.TagName);
            tag.PropertyChanged += OnTagPropertyChanged;
        }


        /// <summary>
        /// create a new view model instance for a tag and an event handler.
        /// </summary>
        /// <param name="tag">tag object</param>
        /// <param name="propHandler">listerner for property changes</param>
        internal SelectableTagModel(TagPageSet tag, PropertyChangedEventHandler propHandler)
            : this(tag)
        {
            PropertyChanged += propHandler;
        }

        private void firePropertyChanged(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, args);
            }
        }

        private void OnTagPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("PageCount"))
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    firePropertyChanged(PAGE_COUNT);
                    Visibility = PageCount > 0 ? Visibility.Visible : Visibility.Collapsed;
                }));
            }
        }
        #region ISelectableTagModel

        private bool _isChecked = false;

        Visibility _visibility;

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

        public string HighlightPattern
        {
            set
            {

            }
        }
        /// <summary>
        /// Get the visibility of the tag in the UI.
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                return _visibility;
            }
            private set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    firePropertyChanged(VISIBILITY);
                }
            }
        }

        public IEnumerable<TextFragment> HitHighlightedTagName { get { return _highlightedTagName; } }

        #endregion ISelectableTagModel

        #region ISortableKeyedItem<TagModelKey,string>
        /// <summary>
        /// Get the view model's unique key
        /// </summary>
        public string Key
        {
            get { return _tag.TagName; }
        }

        /// <summary>
        /// Get the view model's sort key
        /// </summary>
        public TagModelKey SortKey
        {
            get { return _key; }
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
    }
}
