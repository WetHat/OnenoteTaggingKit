using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    ///  A basic implementation of a view model for tags which can be filtered
    ///  based on a pattern.
    /// </summary>
    public class FilterableTagModel : TagModel {
        IList<TextFragment> _highlightedTagName = new TextFragment[]{};
        /// <summary>
        /// Get/set the list of tagname highlights.
        /// </summary>
        public IList<TextFragment> HighlightedTagName {
            get => _highlightedTagName;
            private set {
                _highlightedTagName = value;
                HasHighlights = value.IsHighlighted();
                UpdateTagVisibility();
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Compute the visibility based on changes to the
        /// <see cref="HighlightedTagName"/> property.
        /// </summary>
        protected virtual void UpdateTagVisibility() {
            TagVisibility = Highlighter.SplitPattern != null && !HasHighlights ?
                Visibility.Collapsed : Visibility.Visible;
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
                return htn.Count == 1 && htn[0].IsMatch;
            }
        }
        /// <summary>
        /// Handle property changes in this instance.
        /// </summary>
        /// <param name="sender">Object which raised the change event</param>
        /// <param name="e">Event details</param>
        protected override void TagModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.TagModelPropertyChanged(sender, e);
            switch (e.PropertyName) {
                case nameof(FilterableTagModel.TagName):
                    HighlightedTagName = Highlighter.SplitText(TagName);
                    break;
            }
        }
    }
}