using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.common
{
    public class SuggestedTagsDataContext: IHighlightableTagDataContext,ISortableKeyedItem<TagModelKey,string>,INotifyPropertyChanged
    {
        bool _hasHighlights;
        IEnumerable<TextFragment> _highlightedTagName;
        string _tagName;
        TagModelKey _sortkey;

        // <summary>
        /// predefined event descriptor for <see cref=">PropertyChanged"/> event fired for the <see cref="HighlightedTagName"/> property
        /// </summary>
        internal static readonly PropertyChangedEventArgs HIGHLIGHTED_TAGNAME = new PropertyChangedEventArgs("HighlightedTagName");

        internal protected void firePropertyChanged(PropertyChangedEventArgs args)
        {
          if (PropertyChanged != null)
          {
              PropertyChanged(this, args);
          }
        }

        internal SuggestedTagsDataContext()
        {   
        }

        internal string TagName
        {
            get
            {
                return _tagName;
            }
            set
            {
                _tagName = value;
                _highlightedTagName = new TextSplitter().SplitText(_tagName);
                _sortkey = new TagModelKey(_tagName);
                firePropertyChanged(HIGHLIGHTED_TAGNAME);
            }
        }

        internal IEnumerable<TextFragment> HighlightedTagName { get { return _highlightedTagName; } }

        #region IHighlightableTagDataContext
        /// <summary>
        /// Set a filter string which is used to determine the appearance of the <see cref="HitHighlightedTagButton"/>
        /// control.
        /// </summary>
        /// <remarks>
        /// Setting this property has a side effect on two other properties: <see cref="Hit"/> and <see cref="Margin"/>.
        /// The appropriate <see cref="E:PropertyChanged"/> events are fired as necessary.
        /// </remarks>
        public virtual TextSplitter Highlighter
        {
            set
            {
                bool before = _hasHighlights;
                _highlightedTagName = value.SplitText(TagName);
                _hasHighlights = _highlightedTagName.IsHighlighted();
                if (_hasHighlights || before != _hasHighlights)
                {
                  firePropertyChanged(HIGHLIGHTED_TAGNAME);
                }
            }
        }

        public bool HasHighlights
        {
            get { return _hasHighlights; }
        }
        #endregion

        #region ISortableKeyedItem<TagModelKey,string>
        public TagModelKey SortKey
        {
            get { return _sortkey; }
        }

        public string Key
        {
            get { return _tagName; }
        }
        #endregion ISortableKeyedItem<TagModelKey,string>

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion INotifyPropertyChanged
    }

    public class SuggestedTagsSource<T> : ObservableSortedList<TagModelKey, string, T>, ITagSource where T : SuggestedTagsDataContext,new()
    {
        internal SuggestedTagsSource()
        {
        }

        /// <summary>
        /// Asnchronously load all tags used anywhere on OneNote pages.
        /// </summary>
        /// <returns>task object</returns>
        internal async Task LoadSuggestedTagsAsync()
        {
            Clear();
            T[] mdls = await Task<T[]>.Run(() => LoadSuggestedTagsAction());
            AddAll(mdls);
        }

        private T[] LoadSuggestedTagsAction()
        {
            return (from string t in OneNotePageProxy.ParseTags(Properties.Settings.Default.KnownTags) select new T() { TagName = t }).ToArray();
        }

        internal void Save()
        {
            Properties.Settings.Default.KnownTags = string.Join(",", from v in Values select v.TagName);
        }

        #region ITagSource
        public IEnumerable<IHighlightableTagDataContext> TagDataContextCollection
        {
            get { return Values; }
        }
        #endregion ITagSource
    }
}
