using System.Globalization;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// A simple view model for a tag consisting of just a name (key)
    /// </summary>
    public class SimpleTagButtonModel : ISortableKeyedItem<TagModelKey,string>
    {
        private string _tag;

        private TagModelKey _sortKey;

        /// <summary>
        /// Create a new instance of a <see cref="SimpleTag"/> object
        /// </summary>
        /// <param name="tag">tag name</param>
        public SimpleTagButtonModel(string tag)
        {
            _tag = tag;
            _sortKey = new TagModelKey(tag);
        }

        /// <summary>
        /// Get the name of this tag
        /// </summary>
        public string TagName
        {
            get { return _tag; }
        }

        #region ISortableKeyedItem<TagModelKey,string>
        /// <summary>
        /// Get the key (name) of this tag
        /// </summary>
        public string Key
        {
            get { return _tag; }
        }
        public TagModelKey SortKey
        {
            get { return _sortKey; }
        }
        #endregion ISortableKeyedItem<TagModelKey,string>


        
    }
}
