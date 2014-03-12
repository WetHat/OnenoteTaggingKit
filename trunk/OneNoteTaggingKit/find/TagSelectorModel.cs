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
    }

    public class TagSelectorModel : DependencyObject, ISortableKeyedItem<TagModelKey>, ITagSelectorModel, INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs PAGE_COUNT = new PropertyChangedEventArgs("PageCount");
        private static readonly PropertyChangedEventArgs IS_CHECKED = new PropertyChangedEventArgs("IsChecked");

        private TagPageSet _tag;
        private TagModelKey _key;

        private bool _isChecked = false;


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
                Dispatcher.Invoke(new Action(() => firePropertyChanged(PAGE_COUNT)));
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
