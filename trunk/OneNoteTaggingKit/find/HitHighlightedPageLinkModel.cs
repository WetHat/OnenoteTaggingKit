// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
// Author: WetHat | (C) Copyright 2013 - 2016 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.find
{
    /// <summary>
    /// Contract for view models supporting the <see cref="HitHighlightedPageLink"/> control.
    /// </summary>
    public interface IHitHighlightedPageLinkModel
    {
        /// <summary>
        /// Get a the hit highlighted page title.
        /// </summary>
        IList<TextFragment> HighlightedTitle { get; }

        string MarkerSymbol { get; }
        Brush MarkerColor { get; }
    }

    /// <summary>
    /// Sortable key to support the ranked display of <see cref="HitHighlightedPageLink"/> controls.
    /// </summary>
    public class HitHighlightedPageLinkKey : IComparable<HitHighlightedPageLinkKey>, IEquatable<HitHighlightedPageLinkKey>
    {
        private int _hits;

        private string _title;

        internal HitHighlightedPageLinkKey(string pageTitle, string pageId)
        {
            _title = pageTitle.ToLower();
            PageID = pageId;
        }

        /// <summary>
        /// Get the unique id of the OneNote page.
        /// </summary>
        public string PageID { get; private set; }

        /// <summary>
        /// Get number of hits of the query string against the page title
        /// </summary>
        internal int HitCount
        {
            set
            {
                _hits = value;
            }
        }

        #region IComparable<HitHighlightedPageLinkKey>

        /// <summary>
        /// compare this key with another key.
        /// </summary>
        /// <param name="other">the other key to compare with</param>
        /// <returns><list type="bullet">
        /// <item>a negative number, if this instance of the key comes before the other key</item>
        /// <item>0, if both keys are identical</item>
        /// <item>a positive number, if this instance of the key comes after the other key</item>
        /// </list>
        /// </returns>
        /// <remarks>ordering takes into account the number of matches of the query against the page title</remarks>
        public int CompareTo(HitHighlightedPageLinkKey other)
        {
            int retval = 0;
            if (_hits < other._hits)
            {
                retval = 1;
            }
            else if (_hits > other._hits)
            {
                retval = -1;
            }

            if (retval == 0)
            {
                retval = _title.CompareTo(other._title);
            }

            if (retval == 0)
            {
                retval = PageID.CompareTo(other.PageID);
            }

            return retval;
        }

        #endregion IComparable<HitHighlightedPageLinkKey>

        #region IEquatable<HitHighlightedPageLinkKey>

        /// <summary>
        /// Check keys for equality
        /// </summary>
        /// <param name="other">the other key to chack against</param>
        /// <returns>true if both keys are equal; false if they are not</returns>
        public bool Equals(HitHighlightedPageLinkKey other)
        {
            return PageID.Equals(other.PageID);
        }

        #endregion IEquatable<HitHighlightedPageLinkKey>
    }

    /// <summary>
    /// View model to support the <see cref="HitHighlightedPageLink"/> control.
    /// </summary>
    /// <remarks>The view model describes a link to a OneNote page returned from a search operation.
    /// <para>The search query is used to generate a hit highlighted rendering of a link to a OneNote page</para>
    /// </remarks>
    public class HitHighlightedPageLinkModel : HitHighlightedPageLinkKey, ISortableKeyedItem<HitHighlightedPageLinkKey, string>, IHitHighlightedPageLinkModel, INotifyPropertyChanged
    {
        private IList<TextFragment> _highlights;
        private TaggedPage _page;
        private OneNoteProxy _onenote;

        private static readonly PropertyChangedEventArgs MARKER_SYMBOL = new PropertyChangedEventArgs("MarkerSymbol");
        private static readonly PropertyChangedEventArgs MARKER_COLOR = new PropertyChangedEventArgs("MarkerColor");

        #region INotifyPropertyChanged

        /// <summary>
        /// Event to notify registered handlers about property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged

        /// <summary>
        /// create a new instance of the view model.
        /// </summary>
        /// <param name="tp">a OneNote page object</param>
        /// <param name="highlighter">object to generate a highlight description of the link title</param>
        /// <param name="onenote">OneNote application object proxy</param>
        internal HitHighlightedPageLinkModel(TaggedPage tp, TextSplitter highlighter, OneNoteProxy onenote)
            : base(tp.Title, tp.ID)
        {
            IsSelected = false;
            _page = tp;
            _highlights = highlighter.SplitText(_page.Title);

            HitCount = _highlights.Count((f) => f.IsMatch);
            _onenote = onenote;
        }

        protected void fireNotifyPropertyChanged(PropertyChangedEventArgs propArgs)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, propArgs);
            }
        }

        /// <summary>
        /// Get the path to the page in the OneNote hierarchy
        /// </summary>
        public IEnumerable<HierarchyElement> Path
        {
            get
            {
                return _page.Path;
            }
        }

        internal string PageLink
        {
            get
            {
                return _onenote.GetHyperlinkToObject(_page.ID, pageObjectID: null);
            }
        }

        #region IHitHighlightedPageLinkModel

        public IList<TextFragment> HighlightedTitle
        {
            get { return _highlights; }
        }

        private bool _isSelected;

        private string _markerSymbol;

        public string MarkerSymbol
        {
            get
            {
                return _markerSymbol;
            }
            private set
            {
                if (!value.Equals(_markerSymbol))
                {
                    _markerSymbol = value;
                    fireNotifyPropertyChanged(MARKER_SYMBOL);
                }
            }
        }

        private Brush _markerColor;

        public Brush MarkerColor
        {
            get
            {
                return _markerColor;
            }
            private set
            {
                if (!value.Equals(_markerColor))
                {
                    _markerColor = value;
                    fireNotifyPropertyChanged(MARKER_COLOR);
                }
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;

                if (_isSelected)
                {
                    MarkerSymbol = "✔";
                    MarkerColor = Brushes.MediumSeaGreen;
                }
                else
                {
                    MarkerSymbol = "❱";
                    MarkerColor = Brushes.DodgerBlue;
                }
            }
        }

        #endregion IHitHighlightedPageLinkModel

        internal string LinkTitle { get { return _page.Title; } }

        #region ISortableKeyedItem<HitHighlightedPageLinkKey,string>

        /// <summary>
        /// Get the unique key of the OneNote page
        /// </summary>
        public string Key
        {
            get { return PageID; }
        }

        /// <summary>
        /// Get the sorting key of the page.
        /// </summary>
        /// <remarks>Sort order is determined by the page title and the number of matches
        /// of the search query for this particular page.</remarks>
        public HitHighlightedPageLinkKey SortKey
        {
            get { return this; }
        }

        #endregion ISortableKeyedItem<HitHighlightedPageLinkKey,string>
    }
}