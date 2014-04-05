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
    }

    internal enum TagOperation
    {
        UNITE,
        SUBTRACT
    }

    /// <summary>
    /// View Model to support the tag editor dialog.
    /// </summary>
    /// <remarks>Maintains a data models for:
    /// <list type="bullet">
    ///   <item>Tags on the current page</item>
    ///   <item>similar pages (based on the tags they share with the current page)</item>
    /// </list>
    /// </remarks>
    public class TagEditorModel : DependencyObject, ITagEditorModel, INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs PAGE_TITLE = new PropertyChangedEventArgs("PageTitle");

        private Microsoft.Office.Interop.OneNote.Application _OneNote;
        private XMLSchema _schema;

        private ObservableSortedList<TagModelKey, string, SimpleTagButtonModel> _pageTags = new ObservableSortedList<TagModelKey, string, SimpleTagButtonModel>();

        private ObservableSortedList<TagModelKey, string, HitHighlightedTagButtonModel> _suggestedTags = new ObservableSortedList<TagModelKey, string, HitHighlightedTagButtonModel>();

        /// <summary>
        /// True if the tags on the current page were changed.
        /// </summary>
        private bool _tagsChanged;

        private CancellationTokenSource _cancelPageUpdater = new CancellationTokenSource();
        private Task _pageUpdater;

        internal TagEditorModel(Microsoft.Office.Interop.OneNote.Application onenote,XMLSchema schema)
        {
            _OneNote = onenote;
            _schema = schema;
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

        #endregion ITagEditorModel

        /// <summary>
        /// Associate a tag with the current page
        /// </summary>
        /// <param name="tags">tags to apply</param>
        internal Task ApplyPageTagsAsync(IEnumerable<string> tags)
        {
            return Task.Run(() =>
            {
                _tagsChanged = true;
            });
        }

        /// <summary>
        /// Dissassociate a tag with the current page.
        /// </summary>
        /// <param name="tagname">name of the tag</param>
        internal Task UnapplyPageTagAsync(string tagname)
        {
            return Task.Run(() =>
                {
                    _tagsChanged = true;
                });
        }

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

        internal void UpdateTagFilter(string[] filter)
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
    }
}
