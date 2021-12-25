using System.Runtime.InteropServices;
using System.Windows;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// A basic data context implementation for showing tags in list views.
    /// </summary>
    /// <remarks>
    ///     Can be used as-is for simple tag representations
    ///     or can be subclassed to add additional functionality such as
    ///     highlighting.
    /// </remarks>
    [ComVisible(false)]
    public class TagModel : ViewModelBase, ISortableKeyedItem<TagModelKey, string>
    {
        string _tagName = string.Empty;
        /// <summary>
        /// Get or set the name of a page tag represented by this model.
        /// </summary>
        /// <remarks>
        ///     Setter should be called only once in construction context.
        /// </remarks>
        public virtual string TagName {
            get => _tagName;
            set {
                _tagName = value;
                SortKey = new TagModelKey(value);
                RaisePropertyChanged();
            }
        }

        Visibility _tagVisibility = Visibility.Visible;
        /// <summary>
        /// The visibility of the tag.
        /// </summary>
        /// <remarks>This value is typically computed by
        /// <see cref="ComputeTagVisibility"/></remarks>
        public Visibility TagVisibility {
            get => _tagVisibility;
            protected set {
                if (_tagVisibility != value) {
                    _tagVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        string _tagIndicator = string.Empty;
        /// <summary>
        /// Get/set the tag indicator.
        /// </summary>
        /// <remarks>
        ///     The indicator string is displayed after the tag name and is
        ///     expected to be composed of characters from the 'Segoe UI Symbol'
        ///     font family. Indicator strings are intended to provide meta
        ///     information for the tag.
        /// </remarks>
        public string TagIndicator {
            get => _tagIndicator;
            set {
                if (!_tagIndicator.Equals(value)) {
                    _tagIndicator = value;
                    RaisePropertyChanged();
                }
            }
        }
        /// <summary>
        /// Determine if the tag is an imported or a managed page tag.
        /// </summary>
        public bool IsImported {
            get {
                return TagName.EndsWith(Properties.Settings.Default.ImportOneNoteTagMarker)
                       || TagName.EndsWith(Properties.Settings.Default.ImportHashtagMarker);
            }
        }

        #region ISortableKeyedItem<TagModelKey, string>
        /// <summary>
        /// Get a key for this tag which is suitable for sorting the tag models-
        /// </summary>
        /// <remarks>The sort key does not need to be unique,</remarks>
        public TagModelKey SortKey { get; private set; }

        /// <summary>
        /// Get the unique key of this tag.
        /// </summary>
        public string Key => TagName;

        #endregion ISortableKeyedItem<TagModelKey, string>
    }
}
