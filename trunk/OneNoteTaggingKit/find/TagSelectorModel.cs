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
    public interface ITagSelectorModel
    {
        bool IsChecked { get; set; }
        int PageCount { get; }
        string TagName { get; }
        Visibility Visibility { get; }
    }

    public class TagSelectorModel : DependencyObject, ISortableKeyedItem<TagModelKey>, ITagSelectorModel, INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs PAGE_COUNT = new PropertyChangedEventArgs("PageCount");
        private static readonly PropertyChangedEventArgs IS_CHECKED = new PropertyChangedEventArgs("IsChecked");
        private static readonly PropertyChangedEventArgs VISIBILITY = new PropertyChangedEventArgs("Visibility");

        private TagPageSet _tag;
        private TagModelKey _key;

        internal TagSelectorModel(TagPageSet tag)
        {
            _tag = tag;
            _key = new TagModelKey(tag.TagName);
            tag.PropertyChanged += OnTagPropertyChanged;
        }

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
                    Visibility = PageCount > 0 ? Visibility.Visible : Visibility.Collapsed;                    
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
                }
            }
        }

        /// <summary>
        /// Get or set the tag's name
        /// </summary>
        public string TagName
        {
            get
            {
                return _tag.TagName;
            }
        }

        Visibility _visibility;
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

        #endregion ITagSelectorModel

        #region ISortableKeyedItem<TagModelKey>
        public TagModelKey Key
        {
            get { return _key; }
        }
        #endregion ISortableKeyedItem<TagModelKey>

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion INotifyPropertyChanged

    }
}
