using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    ///  A basic implementation of a view model for tags which can be filtered.
    /// </summary>
    /// <remarks>
    ///     Tag visibility is dermined by:
    ///     <list type="bullet">
    ///         <item>Selection status. Selected tags are collapsed.</item>
    ///         <item>Highlighting patterns defined by the
    ///               <see cref="Highlighter"/> property.</item>
    ///     </list>
    /// </remarks>
    public class FilterableTagModel : TagModel {

        /// <summary>
        /// Get/set the name of the tag.
        /// </summary>
        /// <remarks>
        /// Updates the <see cref="HighlightedTagName"/> property when set.
        /// </remarks>
        public override string TagName {
            get => base.TagName;
            set {
                base.TagName = value;
                HighlightedTagName = Highlighter.SplitText(value);
            }
        }

        IList<TextFragment> _highlightedTagName = new TextFragment[]{};
        /// <summary>
        /// Get/set the list of tagname highlights.
        /// </summary>
        public IList<TextFragment> HighlightedTagName {
            get => _highlightedTagName;
            set {
                _highlightedTagName = value;
                HasHighlights = value.IsHighlighted();
                TagVisibility = ComputeTagVisibility();
                RaisePropertyChanged();
            }
        }

        bool _isSelected = false;
        /// <summary>
        /// Get/set the tag selection flag.
        /// </summary>
        /// <remarks>Selected tags are collapsed.</remarks>
        public bool IsSelected {
            get => _isSelected;
            set {
                _isSelected = value;
                TagVisibility = ComputeTagVisibility();
                TagIndicator = ComputeTagIndicator();
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
            private set {
                if (_tagVisibility != value) {
                    _tagVisibility = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Compute the visibility based on changes to the <see cref="IsSelected"/>
        /// and/or <see cref="HighlightedTagName"/> property.
        /// </summary>
        protected virtual Visibility ComputeTagVisibility() {
            return IsSelected || (Highlighter.SplitPattern != null && !HasHighlights) ?
                Visibility.Collapsed : Visibility.Visible;
        }

        string _tagIndicator = string.Empty;
        /// <summary>
        /// Get/set the tag indicator.
        /// </summary>
        /// <remarks>
        ///     The indicator string is displayed after the tag name and is
        ///     expected to be composed of characters from the 'Segoe UI Symbol'
        ///     font family. Indicator strings are supposed to provide meta
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
        /// Compute the tag indicator when the <see cref="IsSelected"/>
        /// property changes.
        /// </summary>
        /// <returns>The new tag indicator string using 'Segoe UI Symbol'
        /// font characters</returns>
        protected string ComputeTagIndicator() {
            return string.Empty;
        }

        static readonly TextSplitter sDefaultSplitter = new TextSplitter();

        /// <summary>
        /// Determine if the tag's name has any highlights.
        /// </summary>
        /// <value>true if highlights are present; false otherwise</value>
        public bool HasHighlights { get; private set; } = false;

        TextSplitter _highlighter = sDefaultSplitter;
        /// <summary>
        /// Set/get the highlighter object which is used to mark portions
        /// of the tag name which match one or more strings.
        /// </summary>
        /// <remarks>
        ///     Setting this property has a side effect on the property
        ///     <see cref="HighlightedTagName"/>.
        ///     Tags which do not match the highlighting pattern are collapsed.
        /// </remarks>
        public TextSplitter Highlighter {
            get => _highlighter;
            set {
                _highlighter = value;
                // apply the highlighting rules to the tag name
                HighlightedTagName = _highlighter.SplitText(TagName);
            }
        }

        /// <summary>
        /// Determine if the highlghting pattern fully matches the tag name.
        /// </summary>
        public bool IsFullMatch {
            get {
                var htn = HighlightedTagName;
                 return htn.Count > 0
                       && htn[0].IsMatch
                       && (htn.Count == 1
                           || (htn.Count == 2 && IsImported));
            }
        }
        /// <summary>
        /// Create a new instance of a data context for selectable tags.
        /// </summary>
        public FilterableTagModel() {
        }
    }
}