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
        /// Get the collection of suggested tags.
        /// </summary>
        ObservableSortedList<TagModelKey, string, HitHighlightedTagButtonModel> SuggestedTags { get; }

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

    internal enum PresetFilter
    {
        CurrentNote,
        SelectedNotes,
        CurrentSection
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

        ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> _pageTags = new ObservableSortedList<TagModelKey, string, SimpleTagButtonModel>();

        ObservableSortedList<TagModelKey, string, HitHighlightedTagButtonModel> _suggestedTags = new ObservableSortedList<TagModelKey, string, HitHighlightedTagButtonModel>();

        TaggingScopeDescriptor[] _taggingScopes;

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
        /// Get the collection of all tags known to the addin.
        /// </summary>
        /// <remarks>These tags are used to suggest page tags</remarks>
        public ObservableSortedList<TagModelKey, string, HitHighlightedTagButtonModel> SuggestedTags
        {
            get { return _suggestedTags; }
        }

        /// <summary>
        /// Get a collection of scopes available for tagging
        /// </summary>
        public IEnumerable<TaggingScopeDescriptor> TaggingScopes
        {
            get { return _taggingScopes; }
        }

        #endregion ITagEditorModel

        /// <summary>
        /// Asnchronously load all tags used anywhere on OneNote pages.
        /// </summary>
        /// <returns>task object</returns>
        internal async Task LoadSuggestedTagsAsync()
        {
            _suggestedTags.Clear();
            HitHighlightedTagButtonModel[] mdls = await Task<HitHighlightedTagButtonModel[]>.Run(() => LoadSuggetedTagsAction());
            _suggestedTags.AddAll(mdls);
        }

        private HitHighlightedTagButtonModel[] LoadSuggetedTagsAction()
        {
            return (from string t in OneNotePageProxy.ParseTags(Properties.Settings.Default.KnownTags) select new HitHighlightedTagButtonModel(t)).ToArray();
        }

        internal async Task<int> SaveChangesAsync(TagOperation op,TaggingScope scope)
        {
            // pass tags and current page as parameters so that the undelying objects can further be modified in the foreground
            int pagesTagged = 0;
            string[] pageTags = (from t in _pageTags.Values select t.TagName).ToArray();
            pagesTagged = await Task<int>.Run(() => SaveChangesAction(pageTags, op, scope));

            // update suggestions
            if (pageTags != null && pageTags.Length > 0)
            {
                SuggestedTags.AddAll(from t in pageTags where !SuggestedTags.ContainsKey(t) select new HitHighlightedTagButtonModel(t));
                Properties.Settings.Default.KnownTags = string.Join(",", from v in SuggestedTags.Values select v.TagName);
            }
            return pagesTagged;
        }

        internal void UpdateTagFilter(IEnumerable<string> filter)
        {
            TextSplitter splitter = new TextSplitter(filter);
            foreach (var st in SuggestedTags)
            {
                st.Filter = splitter;
            }
        }

        private int SaveChangesAction(string[] tags, TagOperation op, TaggingScope scope)
        {
            IEnumerable<string> pageIDs = null;
            int pagesTagged = 0;

            TagCollection tagCollection = null;

            switch (scope)
            {
                default:
                case TaggingScope.CurrentNote:
                    pageIDs = new string[] { _OneNote.Windows.CurrentWindow.CurrentPageId };
                    break;
                case TaggingScope.SelectedNotes:
                    tagCollection = new TagCollection(_OneNote, _schema);
                    tagCollection.LoadHierarchy(_OneNote.Windows.CurrentWindow.CurrentSectionId);
                    pageIDs = from p in tagCollection.Pages where p.Value.IsSelected select p.Key;
                    break;
                case TaggingScope.CurrentSection:
                    tagCollection = new TagCollection(_OneNote, _schema);
                    tagCollection.LoadHierarchy(_OneNote.Windows.CurrentWindow.CurrentSectionId);
                    pageIDs = from p in tagCollection.Pages select p.Key;
                    break;
            }

            foreach (string pageID in pageIDs)
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

        internal Task<IEnumerable<TagPageSet>> GetContextTagsAsync(PresetFilter filter)
        {
            return Task<IEnumerable<TagPageSet>>.Run(() => { return GetContextTagsAction(filter);}); 
        }

        private IEnumerable<TagPageSet> GetContextTagsAction(PresetFilter filter)
        {
            HashSet<TagPageSet> tags = new HashSet<TagPageSet>();

            TagCollection contextTags = new TagCollection(_OneNote, _schema);

            contextTags.Find(_OneNote.Windows.CurrentWindow.CurrentSectionId, includeUnindexedPages: true);

            switch (filter)
            {
                case PresetFilter.CurrentNote:
                    TaggedPage currentPage = (from p in contextTags.Pages where p.Key.Equals(OneNote.Windows.CurrentWindow.CurrentPageId) select p.Value).FirstOrDefault();
                    if (currentPage != null)
                    {
                        tags.UnionWith(currentPage.Tags);
                    }
                    break;
                case PresetFilter.SelectedNotes:
                    foreach (var p in (from pg in contextTags.Pages where pg.Value.IsSelected select pg.Value))
                    {
                        tags.UnionWith(p.Tags);
                    }
                    break;
                case PresetFilter.CurrentSection:
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
