﻿using Microsoft.Office.Interop.OneNote;
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

        FilterPreset[] FilterPresets { get; }
    }

    internal enum TagOperation
    {
        UNITE,
        SUBTRACT
    }

    public enum FilterPresetType
    {
        None = 0,
        CurrentNote,
        SelectedNotes,
        CurrentSection
    }

    public class FilterPreset
    {
        public FilterPresetType Preset {get; set;}
        public string Label { get; set; }
    }

    /// <summary>
    /// View Model to support the tag editor dialog.
    /// </summary>
    /// <remarks>Maintains a data models for:
    /// <list type="bullet">
    ///   <item>Tags selection</item>
    ///   <item>suggested tags</item>
    /// </list>
    /// </remarks>
    public class TagEditorModel : DependencyObject, ITagEditorModel, INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs PAGE_TITLE = new PropertyChangedEventArgs("PageTitle");

        private Microsoft.Office.Interop.OneNote.Application _OneNote;
        private XMLSchema _schema;

        private ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> _pageTags = new ObservableSortedList<TagModelKey, string, SimpleTagButtonModel>();

        private ObservableSortedList<TagModelKey, string, HitHighlightedTagButtonModel> _suggestedTags = new ObservableSortedList<TagModelKey, string, HitHighlightedTagButtonModel>();

        FilterPreset[] _filterPresets;

        internal TagEditorModel(Microsoft.Office.Interop.OneNote.Application onenote,XMLSchema schema)
        {
            _OneNote = onenote;
            _schema = schema;

            _filterPresets = new FilterPreset[] { new FilterPreset()
                                                        {
                                                            Preset = FilterPresetType.None,
                                                            Label = Properties.Resources.TagEditor_Context_None
                                                        },
                                                  new FilterPreset()
                                                        {
                                                            Preset = FilterPresetType.CurrentNote,
                                                            Label = Properties.Resources.TagEditor_Context_CurrentNote
                                                        },
                                                  new FilterPreset()
                                                        {
                                                            Preset = FilterPresetType.SelectedNotes,
                                                            Label =  Properties.Resources.TagEditor_Context_SelectedNotes
                                                        },
                                                  new FilterPreset()
                                                        {
                                                            Preset = FilterPresetType.CurrentSection,
                                                            Label =  Properties.Resources.TagEditor_Context_CurrentSection
                                                        },
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

        public FilterPreset[] FilterPresets
        {
            get { return _filterPresets; }
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

        internal async Task SaveChangesAsync(TagOperation op)
        {
            // pass tags and current page as parameters so that the undelying objects can further be modified in the foreground

            string[] pageTags = (from t in _pageTags.Values select t.TagName).ToArray();
            await Task.Run(() => SaveChangesAction(pageTags,op));

            // update suggestions
            if (pageTags != null && pageTags.Length > 0)
            {
                SuggestedTags.AddAll(from t in pageTags where !SuggestedTags.ContainsKey(t) select new HitHighlightedTagButtonModel(t));
                Properties.Settings.Default.KnownTags = string.Join(",", from v in SuggestedTags.Values select v.TagName);
            }
        }

        internal void UpdateTagFilter(IEnumerable<string> filter)
        {
            foreach (var st in SuggestedTags)
            {
                st.Filter = filter;
            }
        }

        private void SaveChangesAction(string[] tags, TagOperation op)
        {
            OneNotePageProxy page = new OneNotePageProxy(_OneNote, _OneNote.Windows.CurrentWindow.CurrentPageId, _schema);

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
            }
            if (pagetags.Count != countBefore)
            {
                string[] sortedTags = pagetags.ToArray();
                Array.Sort<string>(sortedTags,(x,y) => string.Compare(x,y,true));

                page.PageTags = sortedTags;
                page.Update();
            }
        }

        private void firePropertyChangedEvent(PropertyChangedEventArgs args)
        {
            PropertyChanged(this, args);
        }
#region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
#endregion

        internal Task<IEnumerable<TagPageSet>> GetContextTagsAsync(FilterPreset selected)
        {
            return Task<IEnumerable<TagPageSet>>.Run(() => { return GetContextTagsAction(selected);}); 
        }

        private IEnumerable<TagPageSet> GetContextTagsAction(FilterPreset context)
        {
            HashSet<TagPageSet> tags = new HashSet<TagPageSet>();

            if (context.Preset != FilterPresetType.None)
            {
                TagCollection contextTags = new TagCollection(_OneNote, _schema);

                contextTags.Find(_OneNote.Windows.CurrentWindow.CurrentSectionId);

                switch (context.Preset)
                {
                    case FilterPresetType.CurrentNote:
                        TaggedPage currentPage = (from p in contextTags.Pages where p.Key.Equals(OneNote.Windows.CurrentWindow.CurrentPageId) select p.Value).FirstOrDefault();
                        if (currentPage != null)
                        {
                            tags.UnionWith(currentPage.Tags);
                        }
                        break;
                    case FilterPresetType.SelectedNotes:
                        foreach (var p in (from pg in contextTags.Pages where pg.Value.IsSelected select pg.Value))
                        {
                            tags.UnionWith(p.Tags);
                        }
                        break;
                    case FilterPresetType.CurrentSection:
                        foreach (var p in contextTags.Pages)
                        {
                            tags.UnionWith(p.Value.Tags);
                        }
                        break;
                }
            }
            return tags;
        }
    }
}
