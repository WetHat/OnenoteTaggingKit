using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    /// View model backing a <see cref="RemovableTag"/> user control.
    /// </summary>
    /// <remarks>Provides properties to enable/disable a tag for removal and to adjust the presentation of the corresponding UI element.</remarks>
    public class RemovableTagModel : ISortableKeyedItem<string>
    {
        private TagPageSet _tag;

        internal RemovableTagModel(TagPageSet tag)
        {
            _tag = tag;
        }

        /// <summary>
        /// Get the tag name
        /// </summary>
        public string TagName
        {
            get { return _tag.Key; }
        }

        /// <summary>
        /// Check whether the tag can be removed
        /// </summary>
        public bool CanRemove
        {
            get { return UseCount == 0; }
        }

        /// <summary>
        /// Get the number of uses of this tag 
        /// </summary>
        public int UseCount
        {
            get { return _tag.Pages.Count; }
        }

        /// <summary>
        /// Get the visibility of the <i>remove</i> marker
        /// </summary>
        public System.Windows.Visibility RemoveMarkerVisibility
        {
            get { return CanRemove ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
        }

        #region IKeyedItem
        /// <summary>
        /// Get the name (key) of this tag
        /// </summary>
        public string Key
        {
            get { return _tag.Key; }
        }
        #endregion IKeyedItem
    }
}
