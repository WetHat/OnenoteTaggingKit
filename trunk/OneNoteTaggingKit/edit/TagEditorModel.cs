using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// Contract used by the tag editor view model
    /// </summary>
    /// <seealso cref="WetHatLab.OneNote.TaggingKit.edit.TagEditor"/>
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

    internal enum TagOperation
    {
        UNITE,
        SUBTRACT,
        REPLACE
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
        public TaggingScope Scope {get; private set;}
        /// <summary>
        /// Get the scope UI label.
        /// </summary>
        public string Label {get; private set;}
    }

    /// <summary>
    /// View Model to support the tag editor dialog.
    /// </summary>
    /// <remarks>Maintains data models for:
    /// <list type="bullet">
    ///   <item>Tags selection</item>
    ///   <item>suggested tags</item>
    /// </list>
    /// </remarks>
    public class TagEditorModel : DependencyObject, ITagEditorModel, INotifyPropertyChanged
    {
        static readonly PropertyChangedEventArgs PAGE_TITLE = new PropertyChangedEventArgs("PageTitle");

        Microsoft.Office.Interop.OneNote.Application _OneNote;
        XMLSchema _schema;

        SuggestedTagsSource<HitHighlightedTagButtonModel> _suggestionSource;

        ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> _pageTags = new ObservableSortedList<TagModelKey, string, SimpleTagButtonModel>();

        TaggingScopeDescriptor[] _taggingScopes;

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

        public TagsAndPages ContextTagCollection
        {
            get
            {
                return new TagsAndPages(_OneNote, _schema);
            }
        }

        internal TagEditorModel(Microsoft.Office.Interop.OneNote.Application onenote,XMLSchema schema)
        {
            _OneNote = onenote;
            _schema = schema;
            _taggingScopes = new TaggingScopeDescriptor[] {
               new TaggingScopeDescriptor(TaggingScope.CurrentNote,Properties.Resources.TagEditor_ComboBox_Scope_CurrentNote),
               new TaggingScopeDescriptor(TaggingScope.SelectedNotes,Properties.Resources.TagEditor_ComboBox_Scope_SelectedNotes),
               new TaggingScopeDescriptor(TaggingScope.CurrentSection,Properties.Resources.TagEditor_ComboBox_Scope_CurrentSection),
            };
        }

        internal Microsoft.Office.Interop.OneNote.Application OneNote
        {
            get
            {
                return _OneNote;
            }
        }

        internal XMLSchema OneNoteSchema
        {
            get
            {
                return _schema;
            }
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

        internal Task<int> SavePageTagsAsync(TagOperation op,TaggingScope scope)
        {
            // bring suggestions up-to-date with new tags that may have been entered
            TagSuggestions.AddAll(from t in _pageTags where !TagSuggestions.ContainsKey(t.Key) select new HitHighlightedTagButtonModel() { TagName = t.TagName });
               
            // pass tags and current page as parameters so that the undelying objects can further be modified in the foreground
            string[] pageTags = (from t in _pageTags.Values select t.TagName).ToArray();
            return Task<int>.Run(() => SaveChangesAction(pageTags, op, scope));
        }

        private int SaveChangesAction(string[] tags, TagOperation op, TaggingScope scope)
        {
            TagSuggestions.Save();
            int pagesTagged = 0;

            TagsAndPages tc = new TagsAndPages(_OneNote, _schema);

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
            tc.GetPagesFromHierarchy(_OneNote.Windows.CurrentWindow, ctx);

            foreach (string pageID in (from p in tc.Pages select p.Key))
            {
                OneNotePageProxy page = new OneNotePageProxy(_OneNote, pageID, _schema);

                HashSet<string> pagetags = new HashSet<string>(page.PageTags);

                int countBefore = pagetags.Count;

                switch (op)
                {
                    case TagOperation.SUBTRACT:
                        pagetags.ExceptWith(tags);
                        break;
                    case TagOperation.UNITE:
                        pagetags.UnionWith(tags);
                        break;
                    case TagOperation.REPLACE:
                        pagetags.Clear();
                        pagetags.UnionWith(tags);
                        break;
                }
                if ((pagetags.Count != countBefore) || op == TagOperation.REPLACE)
                {
                    string[] sortedTags = pagetags.ToArray();
                    Array.Sort<string>(sortedTags, (x, y) => string.Compare(x, y, true));

                    page.PageTags = sortedTags;
                    page.Update();
                }
                pagesTagged++;
            }
            return pagesTagged;
        }

        private void firePropertyChangedEvent(PropertyChangedEventArgs args)
        {
            PropertyChanged(this, args);
        }
#region INotifyPropertyChanged
        /// <summary>
        /// event to notify registered listener about property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
#endregion

        internal Task<IEnumerable<TagPageSet>> GetContextTagsAsync(TagContext filter)
        {
            return Task<IEnumerable<TagPageSet>>.Run(() => { return GetContextTagsAction(filter);}); 
        }

        private IEnumerable<TagPageSet> GetContextTagsAction(TagContext filter)
        {
            HashSet<TagPageSet> tags = new HashSet<TagPageSet>();

            TagsAndPages contextTags = new TagsAndPages(_OneNote, _schema);

            contextTags.FindPages(_OneNote.Windows.CurrentWindow.CurrentSectionId, includeUnindexedPages: true);

            switch (filter)
            {
                case TagContext.CurrentNote:
                    TaggedPage currentPage = (from p in contextTags.Pages where p.Key.Equals(OneNote.Windows.CurrentWindow.CurrentPageId) select p.Value).FirstOrDefault();
                    if (currentPage != null)
                    {
                        tags.UnionWith(currentPage.Tags);
                    }
                    break;
                case TagContext.SelectedNotes:
                    foreach (var p in (from pg in contextTags.Pages where pg.Value.IsSelected select pg.Value))
                    {
                        tags.UnionWith(p.Tags);
                    }
                    break;
                case TagContext.CurrentSection:
                    foreach (var p in contextTags.Pages)
                    {
                        tags.UnionWith(p.Value.Tags);
                    }
                    break;
            }
            return tags;
        }
    }

}
