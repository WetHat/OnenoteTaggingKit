using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    public interface IHitHighlightedTagButtonModel: INotifyPropertyChanged
    {
        Visibility Visibility { get; }
        string TagName { get; }
        Hit Hit { get; }
        Thickness Margin { get; }
    }

    public struct Hit: IEquatable<Hit>
    {
        public int Index;
        public int Length;

        #region IEquatable<Hit>
        public bool Equals(Hit other)
        {
            return Index == other.Index && Length == other.Length;
        }
        #endregion IEquatable<Hit>
    }

    public class HitHighlightedTagButtonModel : IHitHighlightedTagButtonModel, ISortableKeyedItem<TagModelKey,string>
    {
        public static readonly PropertyChangedEventArgs HIT_Property = new PropertyChangedEventArgs("Hit");
        public static readonly PropertyChangedEventArgs VISIBILITY_Property = new PropertyChangedEventArgs("Visibility");
        public static readonly PropertyChangedEventArgs MARGIN_Property = new PropertyChangedEventArgs("Margin");

        string _tagName;
        Hit _hit ;
        TagModelKey _sortkey;


        internal HitHighlightedTagButtonModel(string tagName)
        {
            _tagName = tagName;
            _sortkey = new TagModelKey(tagName);
        }

        #region IHitHighlightedTagButtonModel

        public string TagName
        {
            get { return _tagName; }
        }

        public Visibility Visibility
        {
            get
            {
                return _hit.Index >= 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        public Hit Hit
        {
            get
            {
                return _hit;
            }
        }

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
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion INotifyPropertyChanged

        #region ISortableKeyedItem<TagModelKey,string>
        public TagModelKey SortKey
        {
            get { return _sortkey; }
        }

        public string Key
        {
            get { return _tagName; }
        }
        #endregion
    }
}
