using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// Base class for data context implementations for controls displaying a single tag (such as a button or label).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Data context implementations based on this class can be used in <see cref="SuggestedTagsSource{T}"/>
    /// collections.
    /// </para>
    /// <para>
    /// This class fires the <see cref="E:WetHatLab.OneNote.TaggingKit.common.PropertyChanged"/> events for changes to the <see cref="HighlightedTagName"/>
    /// property. Derived classes may fire additional <see cref="E:WetHatLab.OneNote.TaggingKit.common.PropertyChanged"/>
    /// events for their custom properties as appropriate.
    /// </para>
    /// <para>
    /// Derived classes typically add custom properties to support specific rendering of their associated
    /// controls. 
    /// </para>
    /// </remarks>
    public class SuggestedTagDataContext: IHighlightableTagDataContext,ISortableKeyedItem<TagModelKey,string>,INotifyPropertyChanged
    {
        bool _hasHighlights;
        IEnumerable<TextFragment> _highlightedTagName;
        string _tagName;
        TagModelKey _sortkey;

        ///<summary>
        /// predefined event descriptor for the <see cref="E:WetHatLab.OneNote.TaggingKit.common.PropertyChanged"/> event fired for changes to the <see cref="HighlightedTagName"/> property
        ///</summary>
        internal static readonly PropertyChangedEventArgs HIGHLIGHTED_TAGNAME = new PropertyChangedEventArgs("HighlightedTagName");

        /// <summary>
        /// Fire a <see cref="E:WetHatLab.OneNote.TaggingKit.common.PropertyChanged"/> event
        /// </summary>
        /// <param name="args">event details describing which property changed</param>
        internal protected void firePropertyChanged(PropertyChangedEventArgs args)
        {
          if (PropertyChanged != null)
          {
              PropertyChanged(this, args);
          }
        }

        internal SuggestedTagDataContext()
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
        /// Setting this property has a side effect on the property <see cref="HighlightedTagName"/>.
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

        /// <summary>
        /// Determine if the suggested tag's name has highlights.
        /// </summary>
        /// <value>true if highlights are present; false otherwise</value>
        public bool HasHighlights
        {
            get { return _hasHighlights; }
        }
        #endregion

        #region ISortableKeyedItem<TagModelKey,string>
        /// <summary>
        /// Get the sortable key of the data context.
        /// </summary>
        public TagModelKey SortKey
        {
            get { return _sortkey; }
        }
        /// <summary>
        /// Get the unique key of the data context
        /// </summary>
        public string Key
        {
            get { return _tagName; }
        }
        #endregion ISortableKeyedItem<TagModelKey,string>

        #region INotifyPropertyChanged
        /// <summary>
        /// Event to notify subscribers about property changes.
        /// </summary>
        /// <remarks>
        /// This class fires the event for the <see cref="HighlightedTagName"/>
        /// property. Derived classes may also fire additional
        /// events for their custom properties as appropriate.
        /// </remarks>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion INotifyPropertyChanged
    }

    /// <summary>
    /// The collection of suggested tags recorded in the add-in settings
    /// </summary>
    /// <typeparam name="T">data context type to use to represent a tag</typeparam>
    /// <remarks>
    /// <para>
    /// This class can be directly bound to the <see cref="HighlightedTagsPanel.TagSource"/> property and provides a
    /// convenient way to display and manage suggested tags.
    /// </para>
    /// <para>
    /// In combination with the <see cref="TagInputBox"/> control dynamic tag pattern hit highlighting
    /// can be implemented by binding a <see cref="TextSplitter"/> created from
    /// the <see cref="TagInputBox.Tags"/> property to the <see cref="HighlightedTagsPanel.Highlighter"/> property.
    /// </para>
    /// </remarks>
    public class SuggestedTagsSource<T> : ObservableSortedList<TagModelKey, string, T>, ITagSource where T : SuggestedTagDataContext,new()
    {
        /// <summary>
        /// create a new instance of a suggested tags collection.
        /// </summary>
        internal SuggestedTagsSource()
        {
        }

        /// <summary>
        /// Asynchronously load all tags used anywhere on OneNote pages.
        /// </summary>
        /// <returns>awaitable task object</returns>
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

        /// <summary>
        /// save the suggested tags to the add-in settings store.
        /// </summary>
        internal void Save()
        {
            Properties.Settings.Default.KnownTags = string.Join(",", from v in Values select v.TagName);
        }

        #region ITagSource
        /// <summary>
        /// Get the data context objects managed in this collection.
        /// </summary>
        public IEnumerable<IHighlightableTagDataContext> TagDataContextCollection
        {
            get { return Values; }
        }
        #endregion ITagSource
    }
}
