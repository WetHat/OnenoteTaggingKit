﻿// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
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
        private KnownTagsSource<FilterableTagModel> _suggestedTags = new KnownTagsSource<FilterableTagModel>();

        /// <summary>
        /// Create a new instance of the view model
        /// </summary>
        public TagEditorDesignerModel() {
            _suggestedTags.AddAll(new FilterableTagModel[] {
                new FilterableTagModel() { TagName="Suggested Tag 1"},
                new FilterableTagModel() { TagName="Suggested Tag 2"}
            });

            _pageTags.AddAll(new SelectedTagModel[] {
                new SelectedTagModel() {
                    TagName = "tag 1"
                },
                new SelectedTagModel() {
                    TagName = "tag 2"
                }
            });
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

        public KnownTagsSource<FilterableTagModel> TagSuggestions => _suggestedTags;

        public TagsAndPages ContextTagCollection => new TagsAndPages(null);
    }
}