// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.Tagger;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// Contract used by the tag editor view model
    /// </summary>
    /// <seealso cref="WetHatLab.OneNote.TaggingKit.edit.TagEditor" />
    internal interface ITagEditorModel
    {
        /// <summary>
        /// Get the collection of tags on current OneNote page.
        /// </summary>
        ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> PageTags { get; }

        /// <summary>
        /// Get the enumeration of tagging scopes
        /// </summary>
        IEnumerable<TaggingScopeDescriptor> TaggingScopes { get; }
    }

    /// <summary>
    /// Classification of a range of OneNote pages.
    /// </summary>
    public enum TaggingScope
    {
        /// <summary>
        /// The OneNote page currently viewed
        /// </summary>
        CurrentNote = 0,

        /// <summary>
        /// The range of OneNote pages currently selected
        /// </summary>
        SelectedNotes,

        /// <summary>
        /// The OneNote section to which the currently viewed page belongs to.
        /// </summary>
        CurrentSection
    }

    /// <summary>
    /// Descriptor for a range of pages the tags will be applied to.
    /// </summary>
    public class TaggingScopeDescriptor
    {
        internal TaggingScopeDescriptor(TaggingScope scope, string label)
        {
            Scope = scope;
            Label = label;
        }

        /// <summary>
        /// Get the scope classification.
        /// </summary>
        public TaggingScope Scope { get; private set; }

        /// <summary>
        /// Get the scope UI label.
        /// </summary>
        public string Label { get; private set; }
    }

    /// <summary>
    /// View Model to support the tag editor dialog.
    /// </summary>
    /// <remarks>
    /// Maintains data models for:
    /// <list type="bullet">
    /// <item>Tags selection</item>
    /// <item>suggested tags</item>
    /// </list>
    /// </remarks>
    [ComVisible(false)]
    public class TagEditorModel : WindowViewModelBase, ITagEditorModel
    {
        private static readonly PropertyChangedEventArgs PAGE_TITLE = new PropertyChangedEventArgs("PageTitle");

        private SuggestedTagsSource<HitHighlightedTagButtonModel> _suggestionSource;

        private ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> _pageTags = new ObservableSortedList<TagModelKey, string, SimpleTagButtonModel>();

        private TaggingScopeDescriptor[] _taggingScopes;

        public SuggestedTagsSource<HitHighlightedTagButtonModel> TagSuggestions
        {
            get
            {
                return _suggestionSource;
            }
            internal set
            {
                _suggestionSource = value;
            }
        }

        /// <summary>
        /// Collection of tags found in a OneNote hierarchy context (section, section
        /// group, notebook)
        /// </summary>
        public TagsAndPages ContextTagCollection
        {
            get
            {
                return new TagsAndPages(OneNoteApp);
            }
        }

        internal TagEditorModel(OneNoteProxy onenote) : base(onenote)
        {
            _taggingScopes = new TaggingScopeDescriptor[] {
               new TaggingScopeDescriptor(TaggingScope.CurrentNote,Properties.Resources.TagEditor_ComboBox_Scope_CurrentNote),
               new TaggingScopeDescriptor(TaggingScope.SelectedNotes,Properties.Resources.TagEditor_ComboBox_Scope_SelectedNotes),
               new TaggingScopeDescriptor(TaggingScope.CurrentSection,Properties.Resources.TagEditor_ComboBox_Scope_CurrentSection),
            };
        }

        #region ITagEditorModel

        /// <summary>
        /// Get the collection of tags on the current page.
        /// </summary>
        public ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> PageTags
        {
            get { return _pageTags; }
        }

        /// <summary>
        /// Get a collection of scopes available for tagging
        /// </summary>
        public IEnumerable<TaggingScopeDescriptor> TaggingScopes
        {
            get { return _taggingScopes; }
        }

        #endregion ITagEditorModel

        internal int EnqueuePagesForTagging(TagOperation op, TaggingScope scope)
        {
            // bring suggestions up-to-date with new tags that may have been entered
            TagSuggestions.AddAll(from t in _pageTags where !TagSuggestions.ContainsKey(t.Key) select new HitHighlightedTagButtonModel() { TagName = t.TagName });
            TagSuggestions.Save();

            TagsAndPages tc = new TagsAndPages(OneNoteApp);

            // covert scope to context
            TagContext ctx;

            switch (scope)
            {
                default:
                case TaggingScope.CurrentNote:
                    ctx = TagContext.CurrentNote;
                    break;

                case TaggingScope.SelectedNotes:
                    ctx = TagContext.SelectedNotes;
                    break;

                case TaggingScope.CurrentSection:
                    ctx = TagContext.CurrentSection;
                    break;
            }
            tc.LoadPageTags(ctx);
            string[] pageTags = (from t in _pageTags.Values select t.TagName).ToArray();
            int enqueuedPages = 0;
            foreach (string pageID in (from p in tc.Pages select p.Key))
            {
                OneNoteApp.TaggingService.Add(new TaggingJob(pageID, pageTags, op));
                enqueuedPages++;
            }
            TraceLogger.Log(TraceCategory.Info(), "{0} page(s) enqueued for tagging with '{1}' using {2}", enqueuedPages, string.Join(";", pageTags), op);
            TraceLogger.Flush();
            return enqueuedPages;
        }
    }
}