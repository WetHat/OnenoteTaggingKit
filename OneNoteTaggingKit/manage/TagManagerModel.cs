// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    /// Contract for view models of the
    /// <see cref="WetHatLab.OneNote.TaggingKit.manage.TagManager" /> dialog
    /// </summary>
    /// <seealso cref="TagManager" />

    internal interface ITagManagerModel
    {
        /// <summary>
        /// Get the collection of all tags used for suggestions.
        /// </summary>
        KnownTagsSource<RemovableTagModel> SuggestedTags { get; }

        /// <summary>
        /// Get the add-in version.
        /// </summary>
        string AddinVersion { get; }

        /// <summary>
        /// Get the add-in version.
        /// </summary>
        string FrameworkVersion { get; }

        /// <summary>
        /// Get the location of the logfile
        /// </summary>
        string Logfile { get; }
    }

    /// <summary>
    /// View model backing the <see cref="TagManager" /> dialog.
    /// </summary>
    [ComVisible(false)]
    public class TagManagerModel : WindowViewModelBase, ITagManagerModel
    {
        private class UseCountComparer : IComparer<KeyValuePair<string, RemovableTagModel>>
        {
            #region IComparer<KeyValuePair<string, RemovableTagModel>>
            public int Compare(KeyValuePair<string, RemovableTagModel> x, KeyValuePair<string, RemovableTagModel> y) {
                int res = x.Value.UseCount.CompareTo(y.Value.UseCount);
                return res == 0 ? x.Key.CompareTo(y.Key) : res;
                }

            #endregion IComparer<KeyValuePair<string, RemovableTagModel>>
        };
        private static readonly UseCountComparer sUseCountComparer = new UseCountComparer();
        private KnownTagsSource<RemovableTagModel> _suggestedTags;
        private TagsAndPages _tags;

        /// <summary>
        /// Create a new instance of the view model backing the <see cref="TagManager" /> dialog.
        /// </summary>
        /// <param name="onenote">OneNote application object</param>
        internal TagManagerModel(OneNoteProxy onenote) : base(onenote) {
            _suggestedTags = new KnownTagsSource<RemovableTagModel>(onenote);
            _tags = new TagsAndPages(onenote);

        }

        internal void SortByTagName () {
            _suggestedTags.ItemComparer = KnownTagsSource<RemovableTagModel>.DefaultComparer;
        }


        internal void SortByUsage() {
            _suggestedTags.ItemComparer = sUseCountComparer;
        }

        internal async Task LoadSuggestedTagsAsync() {
            // grab all tags
            var t1 = Task.Run(() => {
                Thread.Yield(); // make sure UI builds up
                _tags.FindPages(SearchScope.AllNotebooks,String.Empty);
            });
            // get the known suggestions (this populates the UI)
            var t2 = _suggestedTags.LoadKnownTagsAsync();

            await t1;
            await t2;

            // populate the suggested tag model with the tags found on pages
            foreach (var t in _suggestedTags.Values) {
                TagPageSet tag;
                if (_tags.Tags.TryGetValue(t.Key, out tag)) {
                    // populate
                    t.PageTag = tag;
                }
            }

            int before = _suggestedTags.Count;
            // suggest tags found on pages but not suggested so far
            _suggestedTags.AddAll(from tps in _tags.Tags.Values
                                  where !_suggestedTags.ContainsKey(tps.Key)
                                  select new RemovableTagModel() {
                                      PageTag = tps
                                  });
            if (before != _suggestedTags.Count) {
                _suggestedTags.Save();
            }
        }

        #region ITagManagerModel

        /// <summary>
        /// Get the collection of tags used for suggestions.
        /// </summary>
        /// <remarks>
        /// This collection includes all tags used on any OneNote pages and additional tags
        /// suggestions which were added manually
        /// </remarks>
        public KnownTagsSource<RemovableTagModel> SuggestedTags {
            get {
                return _suggestedTags;
            }
        }

        /// <summary>
        /// Get the build configuration of the add-in
        /// </summary>
        public string Configuration {
            get {
#if DEBUG
                return "Debug";
#else
                return "Release";
#endif
            }
        }

        /// <summary>
        /// Get the version of the add-in.
        /// </summary>
        public string AddinVersion {
            get {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        /// <summary>
        /// Get the version of the .net framework running the tagging add-in.
        /// </summary>
        public string FrameworkVersion {
            get { return Environment.Version.ToString(); }
        }

        /// <summary>
        /// Get the location of the add-in logfile
        /// </summary>
        public string Logfile {
            get { return TraceLogger.LogFile; }
        }

        #endregion ITagManagerModel

        /// <summary>
        /// Get comma separated list of suggested tags.
        /// </summary>
        public string TagList {
            get {
                StringBuilder tags = new StringBuilder();
                foreach (var t in _suggestedTags.Values) {
                    if (tags.Length > 0) {
                        tags.Append(',');
                    }
                    tags.Append(t.TagName);
                }
                return tags.ToString();
            }
        }


        /// <summary>
        /// Dispose this model.
        /// </summary>
        public override void Dispose() {
            if (_suggestedTags != null) {
                if (_suggestedTags.Count > 0) {
                    _suggestedTags.Update();
                    _suggestedTags.Save();
                }
                _suggestedTags.Dispose();
                _suggestedTags = null;
            }
            base.Dispose();
        }

    }
}