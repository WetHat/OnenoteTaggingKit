using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// Contract for view models for the <see cref="HitHighlightedTagButton"/> control.
    /// </summary>
    public interface IHitHighlightedTagButtonModel: INotifyPropertyChanged
    {
        /// <summary>
        /// Get the current control visibility.
        /// </summary>
        Visibility Visibility { get; }
        /// <summary>
        /// Get the name of the tag the control should display
        /// </summary>
        string TagName { get; }
        /// <summary>
        /// Get the description of a substring match
        /// </summary>
        Hit Hit { get; }

        /// <summary>
        /// get the current margin of the control.
        /// </summary>
        Thickness Margin { get; }
    }

    /// <summary>
    /// Descriptor for a substring match.
    /// </summary>
    public struct Hit: IEquatable<Hit>
    {
        /// <summary>
        /// startindey of match
        /// </summary>
        public int Index;
        /// <summary>
        /// length of match
        /// </summary>
        public int Length;

        #region IEquatable<Hit>
        /// <summary>
        /// compare two instances of class <see cref="Hit"/> for equality
        /// </summary>
        /// <param name="other">other instance of this class to compare against</param>
        /// <returns>true if this instance is identcal with the other instance</returns>
        public bool Equals(Hit other)
        {
            return Index == other.Index && Length == other.Length;
        }
        #endregion IEquatable<Hit>
    }

    /// <summary>
    /// View model for the <see cref="HitHighlightedTagButton"/> control.
    /// </summary>
    public class HitHighlightedTagButtonModel : IHitHighlightedTagButtonModel, ISortableKeyedItem<TagModelKey,string>
    {
        /// <summary>
        /// predefined event descriptor for <see cref=">PropertyChanged"/> event fired for the <see cref="Hit"/> property
        /// </summary>
        internal static readonly PropertyChangedEventArgs HIT_Property = new PropertyChangedEventArgs("Hit");
        /// <summary>
        /// predefined event descriptor for <see cref=">PropertyChanged"/> event fired for the <see cref="Visibility"/> property
        /// </summary>
        internal static readonly PropertyChangedEventArgs VISIBILITY_Property = new PropertyChangedEventArgs("Visibility");
        /// <summary>
        /// predefined event descriptor for <see cref=">PropertyChanged"/> event fired for the <see cref="Margin"/> property
        /// </summary>
        internal static readonly PropertyChangedEventArgs MARGIN_Property = new PropertyChangedEventArgs("Margin");

        string _tagName;
        Hit _hit ;
        TagModelKey _sortkey;


        internal HitHighlightedTagButtonModel(string tagName)
        {
            _tagName = tagName;
            _sortkey = new TagModelKey(tagName);
        }

        #region IHitHighlightedTagButtonModel

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
                return _hit.Index >= 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }
        /// <summary>
        /// Get the hit higlighting data of the  <see cref="HitHighlightedTagButton"/> control.
        /// </summary>
        public Hit Hit
        {
            get
            {
                return _hit;
            }
        }

        /// <summary>
        /// Get the margin the associated <see cref="HitHighlightedTagButton"/> control uses.
        /// </summary>
        public Thickness Margin
        {
            get
            {
                return Visibility == System.Windows.Visibility.Visible ? new Thickness(0, 5, 5, 0) : new Thickness(0,0,0,0);
            }
        }
        #endregion IHitHighlightedTagButtonModel

        private void firePropertyChange(PropertyChangedEventArgs args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, args);
            }
        }

        /// <summary>
        /// Set a filter string which is used to determine the appearance of the <see cref="HitHighlightedTagButton"/>
        /// control.
        /// </summary>
        /// <remarks>
        /// Setting this property has a side effect on two other properties: <see cref="Hit"/> and <see cref="Margin"/>.
        /// The appropriate <see cref="PropertyChanged"/> events are fired as necessary.
        /// </remarks>
        internal IEnumerable<string> Filter
        {
            set
            {
                Visibility visBefore = Visibility;
                Hit hitBefore = _hit;

                if (value == null)
                {
                    _hit = new Hit
                        {
                            Index = 0,
                            Length = 0
                        };
                }
                else
                {
                    _hit = new Hit {
                        Index = -1,
                        Length = 0
                    };
                    foreach (string s in value)
                    {
                        int index = TagName.IndexOf(s, 0, StringComparison.CurrentCultureIgnoreCase);
                        if (index >= 0)
                        {
                            _hit.Index = index;
                            _hit.Length = s.Length;
                            break;
                        }
                    }
                }
                if (!hitBefore.Equals(_hit))
                {
                    firePropertyChange(HIT_Property);
                }
                if (visBefore != Visibility)
                {
                    firePropertyChange(VISIBILITY_Property);
                    firePropertyChange(MARGIN_Property);
                }
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
