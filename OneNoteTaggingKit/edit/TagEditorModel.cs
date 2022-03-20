// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;
using WetHatLab.OneNote.TaggingKit.Tagger;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// Contract used by the tag editor view model
    /// </summary>
    /// <remarks>
    /// Promarily used to implement the designer model.
    /// </remarks>
    /// <seealso cref="WetHatLab.OneNote.TaggingKit.edit.TagEditor" />
    internal interface ITagEditorModel
    {
        /// <summary>
        /// Get the collection of tags to apply to one or more OneNote pages.
        /// </summary>
        ObservableTagList<SelectedTagModel> SelectedTags { get; }

        /// <summary>
        /// Collection of tags suggested for page tagging.
        /// </summary>
        KnownTagsSource<SelectableTagModel> TagSuggestions { get; }
        /// <summary>
        /// Get the enumeration of tagging scopes.
        /// </summary>
        IEnumerable<TaggingScopeDescriptor> TaggingScopes { get; }

        TagsAndPages ContextTagCollection { get; }
        int ScopeIndex { get; }

        bool ScopesEnabled { get; }
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
        internal TaggingScopeDescriptor(TaggingScope scope, string label) {
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
    /// View Model to support the tag editor dialog <see cref="TagEditor"/>.
    /// </summary>
    /// <remarks>
    /// Maintains data models for:
    /// <list type="bullet">
    ///     <item>Tags selection</item>
    ///     <item>Suggested tags</item>
    /// </list>
    /// </remarks>
    [ComVisible(false)]
    public class TagEditorModel : WindowViewModelBase, ITagEditorModel
    {
        /// <summary>
        /// Get or set the tagging scope.
        /// </summary>
        [DefaultValue(TaggingScope.CurrentNote)]
        public TaggingScope Scope { get; set; }

        /// <summary>
        /// Get or set the index of the selected tagging scope.
        ///
        /// <seealso cref="Scope" />
        /// </summary>
        public int ScopeIndex {
            get => (int)Scope;
            set => Scope=(TaggingScope)value;
        }

        /// <summary>
        /// Determine if the tagging scope can be changed by the user.
        /// </summary>
        public bool ScopesEnabled => Scope != TaggingScope.SelectedNotes || PagesToTag == null;

        /// <summary>
        /// Collection of OneNote page IDs for tagging.
        /// </summary>
        [DefaultValue(null)]
        public IEnumerable<string> PagesToTag {
            get => ContextTagCollection.SelectedPages;
            set => ContextTagCollection.SelectedPages = value;
        }

        /// <summary>
        /// Create a new view model for the <see cref="TagEditor"/> dialog.
        /// </summary>
        /// <param name="onenote">The OneNote proxy object</param>
        public TagEditorModel(OneNoteProxy onenote) : base(onenote) {
            TaggingScopes = new TaggingScopeDescriptor[] {
                new TaggingScopeDescriptor(TaggingScope.CurrentNote,Properties.Resources.TagEditor_ComboBox_Scope_CurrentNote),
                new TaggingScopeDescriptor(TaggingScope.SelectedNotes,Properties.Resources.TagEditor_ComboBox_Scope_SelectedNotes),
                new TaggingScopeDescriptor(TaggingScope.CurrentSection,Properties.Resources.TagEditor_ComboBox_Scope_CurrentSection),
            };
            TagSuggestions = new KnownTagsSource<SelectableTagModel>();
        }

        #region ITagEditorModel

        /// <summary>
        /// Get the collection of tags that will be used for tagging one or more
        /// OneNote pages.
        /// </summary>
        public ObservableTagList<SelectedTagModel> SelectedTags { get; } = new ObservableTagList<SelectedTagModel>();

        /// <summary>
        /// Collection of tags suggested for page tagging.
        /// </summary>
        public KnownTagsSource<SelectableTagModel> TagSuggestions { get; }

        /// <summary>
        /// Get a collection of scopes available for tagging
        /// </summary>
        public IEnumerable<TaggingScopeDescriptor> TaggingScopes { get; }

        TagsAndPages _contextTags;
        /// <summary>
        /// Collection of tags found in a OneNote hierarchy context (section, section
        /// group, notebook)
        /// </summary>

        public TagsAndPages ContextTagCollection {
            get {
                if (_contextTags == null) {
                    _contextTags = new TagsAndPages(OneNoteApp);
                }
                return _contextTags;
            }
        }

        #endregion ITagEditorModel

        internal int EnqueuePagesForTagging(TagOperation op) {
            // bring suggestions up-to-date with new tags that may have been entered
            TagSuggestions.AddAll(from t in SelectedTags.Values
                                  where t.SelectableTag == null
                                  select new SelectableTagModel() { Tag = t.Tag });
            TagSuggestions.Save();

            // covert scope to context
            IEnumerable<string> pageIDs;
            if (Scope == TaggingScope.CurrentNote) {
                pageIDs = new string[] { OneNoteApp.CurrentPageID };
            } else {
                var ph = new PageHierarchy(OneNoteApp);
                ph.AddPages(OneNoteApp.GetHierarchy(OneNoteApp.CurrentSectionID, Microsoft.Office.Interop.OneNote.HierarchyScope.hsPages));
                if (Scope == TaggingScope.SelectedNotes) {
                    pageIDs = from p in ph.Pages
                              where p.IsSelected
                              select p.ID;
                } else {
                    pageIDs = from p in ph.Pages select p.ID;
                }
            }

            int enqueuedPages = 0;
            var tagset = new PageTagSet(from t in SelectedTags.Values select t.Tag);
            foreach (string pageID in pageIDs) {
                OneNoteApp.TaggingService.Add(new TaggingJob(pageID, tagset, op));
                enqueuedPages++;
            }
            TraceLogger.Log(TraceCategory.Info(), "{0} page(s) enqueued for tagging with '{1}' using {2}", enqueuedPages, tagset.ToString(), op);
            TraceLogger.Flush();
            return enqueuedPages;
        }
        #region IDisposable
        /// <summary>
        /// Dispose all observable tag lists.
        /// </summary>
        public override void Dispose() {
            base.Dispose();
            TagSuggestions.Dispose();
            SelectedTags.Dispose();
        }
        #endregion IDisposable
    }
}