using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// View model for the <see cref="HitHighlightedTagButton"/> control.
    /// </summary>
    public class HitHighlightedTagButtonModel : IFilterableTagDataContext, INotifyPropertyChanged, ISortableKeyedItem<TagModelKey, string>
    {
        /// <summary>
        /// predefined event descriptor for <see cref=">PropertyChanged"/> event fired for the <see cref="HitHighlightedTagName"/> property
        /// </summary>
        internal static readonly PropertyChangedEventArgs HITHIGHLIGHTED_TAGNAME_Property = new PropertyChangedEventArgs("HitHighlightedTagName");
        /// <summary>
        /// predefined event descriptor for <see cref=">PropertyChanged"/> event fired for the <see cref="Visibility"/> property
        /// </summary>
        internal static readonly PropertyChangedEventArgs VISIBILITY_Property = new PropertyChangedEventArgs("Visibility");
        
        string _tagName;
        IEnumerable<TextFragment> _hithighlightedTagname ;
        bool _isFiltered = false;
        TagModelKey _sortkey;

        internal HitHighlightedTagButtonModel(string tagName)
        {
            _tagName = tagName;
            _sortkey = new TagModelKey(tagName);
            TextSplitter splitter = new TextSplitter();
            _hithighlightedTagname = splitter.SplitText(tagName);
        }
        
        /// <summary>
        /// Get the name of the tag the associated <see cref="HitHighlightedTagButton"/> control displays
        /// </summary>
        public string TagName
        {
            get { return _tagName; }
        }

        /// <summary>
        /// Get the visibilty the associated <see cref="HitHighlightedTagButton"/> control has. 
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                return !_isFiltered || _hithighlightedTagname.FirstOrDefault((f) => f.IsMatch).IsMatch ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        public IEnumerable<TextFragment> HitHighlightedTagName
        {
            get { return _hithighlightedTagname; }
        }

        #region IFilterableTagDataContext
        /// <summary>
        /// Set a filter string which is used to determine the appearance of the <see cref="HitHighlightedTagButton"/>
        /// control.
        /// </summary>
        /// <remarks>
        /// Setting this property has a side effect on two other properties: <see cref="Hit"/> and <see cref="Margin"/>.
        /// The appropriate <see cref="PropertyChanged"/> events are fired as necessary.
        /// </remarks>
        public TextSplitter Highlighter
        {
            set
            {
                Visibility visBefore = Visibility;
                _isFiltered = value.SplitPattern != null;
                _hithighlightedTagname = value.SplitText(TagName);
                HasHighlights = (from f in _hithighlightedTagname where f.IsMatch select f).FirstOrDefault().IsMatch;
                if (Visibility == System.Windows.Visibility.Visible)
                {
                    firePropertyChange(HITHIGHLIGHTED_TAGNAME_Property);
                }
                if (visBefore != Visibility)
                {
                    firePropertyChange(VISIBILITY_Property);
                }
            }
        }

        public bool HasHighlights { get; private set; }

        #endregion

        private void firePropertyChange(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, args);
            }
        }

        #region INotifyPropertyChanged
        /// <summary>
        /// Event fired for view model properties bound to the <see cref="HitHighlightedTagButton"/> control. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion INotifyPropertyChanged

        #region ISortableKeyedItem<TagModelKey,string>

        /// <summary>
        /// Get the view model's sort key
        /// </summary>
        public TagModelKey SortKey
        {
            get { return _sortkey; }
        }

        /// <summary>
        /// Get the view models key used for hashing
        /// </summary>
        public string Key
        {
            get { return _tagName; }
        }
        #endregion

    }
}
