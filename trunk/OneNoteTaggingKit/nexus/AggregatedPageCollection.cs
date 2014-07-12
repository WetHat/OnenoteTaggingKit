using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.nexus
{

    internal class AggregatedPageCollection : TagsAndPages
    {
        /// <summary>
        /// set of pages aggregated from tags
        /// </summary>
        private ObservableDictionary<string, TaggedPage> _aggregatedPages = new ObservableDictionary<string, TaggedPage>();

        /// <summary>
        /// Tags used to aggregate pages
        /// </summary>
        private ObservableDictionary<string, TagPageSet> _aggregationTags = new ObservableDictionary<string, TagPageSet>();

        internal AggregatedPageCollection(Application onenote, XMLSchema schema)
            : base(onenote, schema)
        {
            _aggregationTags.CollectionChanged += OnTagsCollectionChanged;
        }

        /// <summary>
        /// Update the aggregated pages as the set of tags changes.
        /// </summary>
        /// <param name="sender">collection which emits the event</param>
        /// <param name="e">event details</param>
        private void OnTagsCollectionChanged(object sender, NotifyDictionaryChangedEventArgs<string, TagPageSet> e)
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Add:
                    foreach (var item in e.Items)
                    {
                        _aggregatedPages.UnionWith(item.Pages);
                    }
                    break;
                case NotifyDictionaryChangedAction.Remove:
                    // rebuild the set
                    _aggregatedPages.Clear();
                    foreach (var item in e.Items)
                    {
                        _aggregatedPages.UnionWith(item.Pages);
                    }
                    break;
                case NotifyDictionaryChangedAction.Reset:
                    _aggregatedPages.Clear();
                    break;
            }
        }

        internal ObservableDictionary<string, TaggedPage> AggregatedPages
        {
            get
            {
                return _aggregatedPages;
            }
        }

        internal ObservableDictionary<string, TagPageSet> AggregationTags
        {
            get
            {
                return _aggregationTags;
            }
        }

        internal IEnumerable<TagPageSet>GetOrCreateTags(IEnumerable<string> tagNames)
        {
            foreach (string t in tagNames)
            {
                TagPageSet tps;
                if (!Tags.TryGetValue(t, out tps))
                {
                    tps = new TagPageSet(t);
                    Tags.Add(t, tps);
                }
                yield return tps;
            }
        }
    }
}
