// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Collections.Generic;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// View model implementation for the UI designer
    /// </summary>
    public class TagEditorDesignerModel : ITagEditorModel
    {
        private ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> _pageTags = new ObservableSortedList<TagModelKey, string, SimpleTagButtonModel>();
        private ObservableSortedList<TagModelKey, string, HitHighlightedTagButtonModel> _suggestedTags = new ObservableSortedList<TagModelKey, string, HitHighlightedTagButtonModel>();

        /// <summary>
        /// Create a new instance of the view model
        /// </summary>
        public TagEditorDesignerModel() {
            _suggestedTags.AddAll(new HitHighlightedTagButtonModel[] {
                new HitHighlightedTagButtonModel() { TagName="Suggested Tag 1"},
                new HitHighlightedTagButtonModel() { TagName="Suggested Tag 2"}
            });

            _pageTags.AddAll(new SimpleTagButtonModel[] { new SimpleTagButtonModel("tag 1"), new SimpleTagButtonModel("tag 2") });
        }

        /// <summary>
        /// get the collection of page tags.
        /// </summary>
        public ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> PageTags {
            get { return _pageTags; }
        }

        /// <summary>
        /// Get the index of the selected tagging scope.
        /// </summary>
        public int ScopeIndex {
            get {
                return (int)TaggingScope.CurrentNote;
            }
        }

        /// <summary>
        /// Determine if the tagging scope can be changed.
        /// </summary>
        public bool ScopesEnabled {
            get {
                return true;
            }
        }

        /// <summary>
        /// Get the design time collection of scopes
        /// </summary>
        public IEnumerable<TaggingScopeDescriptor> TaggingScopes {
            get { return new TaggingScopeDescriptor[] { new TaggingScopeDescriptor(TaggingScope.CurrentNote, "Current Note") }; }
        }
    }
}