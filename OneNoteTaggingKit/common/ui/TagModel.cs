﻿using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// Interface to be used by designer model.
    /// </summary>
    public interface ITagModel
    {
        /// <summary>
        /// Get the basename of a tag.
        /// </summary>
        string TagName { get; }

        /// <summary>
        /// Get the tag type prefix marker.
        /// </summary>
        string TagTypePrefix { get; }

        /// <summary>
        /// Get the tag type postfix parker.
        /// </summary>
        string TagTypePostfix { get; }
        /// <summary>
        /// Get the tag indicator.
        /// </summary>
        /// <remarks>
        ///     The indicator provides additional meta information
        ///     such as use counts. The tag indicator is typically displayed
        ///     as a postfix.
        /// </remarks>
        string TagIndicator { get; }

        /// <summary>
        /// Get the tag indicator foreground color.
        /// </summary>
        Brush TagIndicatorColor { get; }
    }

    /// <summary>
    /// A basic data context implementation for showing tags in list views.
    /// </summary>
    /// <remarks>
    ///     Can be used as-is for simple tag representations
    ///     or can be subclassed to add additional functionality such as
    ///     highlighting.
    /// </remarks>
    [ComVisible(false)]
    public class TagModel : ObservableObject, ISortableKeyedItem<string, string>, ITagModel {
        PageTag _pagetag;
        /// <summary>
        /// Get or set the page tag represented by this model.
        /// </summary>
        /// <remarks>
        ///     Setter should be called only once in construction context.
        /// </remarks>
        public virtual PageTag Tag {
            get => _pagetag;
            set {
                _pagetag = value;
                if (value.IsRTL) {
                    TagTypePrefix = string.Empty;
                    TagTypePostfix = value.TagMarker;
                } else {
                    TagTypePrefix = value.TagMarker;
                    TagTypePostfix = string.Empty;
                }
            }
        }

        /// <summary>
        /// Get or set the name of a page tag represented by this model.
        /// </summary>
        /// <remarks>
        ///     Setter should be called only once in construction context.
        /// </remarks>
        public string TagName => Tag.BaseName;

        Visibility _tagVisibility = Visibility.Visible;
        /// <summary>
        /// The visibility of the tag.
        /// </summary>
        public Visibility TagVisibility {
            get => _tagVisibility;
            protected set {
                if (_tagVisibility != value) {
                    _tagVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        Brush _tagIndicatorColor = Brushes.Red;
        /// <summary>
        /// Get/set the forground color of the tag indicator
        /// </summary>
        public Brush TagIndicatorColor {
            get => _tagIndicatorColor;
            set {
                if (_tagIndicatorColor != value) {
                    _tagIndicatorColor = value;
                    RaisePropertyChanged();
                }
            }
        }

        string _tagTypePrefix = string.Empty;
        /// <summary>
        /// Get the tag prefix marker.
        /// </summary>
        public string TagTypePrefix {
            get => _tagTypePrefix;
            protected set {
                if (!_tagTypePrefix.Equals(value)) {
                    _tagTypePrefix = value;
                    RaisePropertyChanged();
                }
            }
        }

        string _tagTypePostfix = string.Empty;
        /// <summary>
        /// Get the tag postfix marker.
        /// </summary>
        public string TagTypePostfix {
            get => _tagTypePostfix;
            protected set {
                if (!_tagTypePostfix.Equals(value)) {
                    _tagTypePostfix = value;
                    RaisePropertyChanged();
                }
            }
        }
        string _tagIndicator = string.Empty;
        /// <summary>
        /// Get/set the tag indicator (postfix string).
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
        #region ISortableKeyedItem<TagModelKey, string>
        /// <summary>
        /// Get a key for this tag which is suitable for sorting the tag models-
        /// </summary>
        /// <remarks>The sort key does not need to be unique,</remarks>
        public string SortKey { get => Tag.SortKey; }

        /// <summary>
        /// Get the unique key of this tag.
        /// </summary>
        public string Key { get => Tag.Key; }

        #endregion ISortableKeyedItem<TagModelKey, string>


        /// <summary>
        /// Create a new view model for tags.
        /// </summary>
        public TagModel() {
            PropertyChanged += TagModelPropertyChanged;
        }

        /// <summary>
        /// Property change handler listening to changes to instances of this
        /// class.
        /// </summary>
        /// <remarks>Derived classes can override this handler to deal with
        /// property dependencies.</remarks>
        /// <param name="sender">The object which raised a property
        /// change event</param>
        /// <param name="e">Event details</param>
        protected virtual void TagModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
        }

        /// <summary>
        ///     Get the hash code for this model,.
        /// </summary>
        /// <returns>
        ///     The hash code for this model.
        /// </returns>
        public override int GetHashCode() => Tag.Key.GetHashCode();

        /// <summary>
        /// Determine if this instance is equal to another object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) {
            if (this == obj) {
                return true;
            }

            return (obj is TagModel other && Key.Equals(other.Key));
        }
    }
}
