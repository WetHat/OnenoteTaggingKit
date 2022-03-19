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
        private ObservableTagList<SelectedTagModel> _pageTags = new ObservableTagList<SelectedTagModel>();
        private KnownTagsSource<SelectableTagModel> _suggestedTags = new KnownTagsSource<SelectableTagModel>();

        /// <summary>
        /// Create a new instance of the view model
        /// </summary>
        public TagEditorDesignerModel() {
            _suggestedTags.AddAll(new SelectableTagModel[] {
                new SelectableTagModel() { Tag = new PageTag("Suggested Tag 1", PageTagType.Unknown)},
                new SelectableTagModel() { Tag= new PageTag("Suggested Tag 2",PageTagType.Unknown)}
            });

            _pageTags.AddAll(new SelectedTagModel[] {
                new SelectedTagModel() {
                    Tag = new PageTag("tag 1",PageTagType.Unknown)
                },
                new SelectedTagModel() {
                    Tag = new PageTag("tag 2",PageTagType.Unknown)
                }
            }) ;
        }

        /// <summary>
        /// get the collection of page tags.
        /// </summary>
        public ObservableTagList<SelectedTagModel> SelectedTags => _pageTags;

        /// <summary>
        /// Get the index of the selected tagging scope.
        /// </summary>
        public int ScopeIndex => (int)TaggingScope.CurrentNote;

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

        public KnownTagsSource<SelectableTagModel> TagSuggestions => _suggestedTags;

        public TagsAndPages ContextTagCollection => new TagsAndPages(null);
    }
}