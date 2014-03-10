using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// A simple view model for a tag consisting of just a name (key)
    /// </summary>
    public class SimpleTagButtonModel : IKeyedItem
    {
        private string _tag;

        /// <summary>
        /// Create a new instance of a <see cref="SimpleTag"/> object
        /// </summary>
        /// <param name="tag"></param>
        public SimpleTagButtonModel(string tag)
        {
            _tag = tag;
        }

        /// <summary>
        /// Get the name of this tag
        /// </summary>
        public string TagName
        {
            get { return _tag; }
        }

        #region IKeyedItem
        /// <summary>
        /// Get the key (name) of this tag
        /// </summary>
        public string Key
        {
            get { return _tag; }
        }
        #endregion IKeyedItem
    }
}
