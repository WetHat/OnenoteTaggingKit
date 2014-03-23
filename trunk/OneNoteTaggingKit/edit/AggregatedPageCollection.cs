using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.edit
{

    internal class AggregatedPageCollection : TagCollection
    {
        /// <summary>
        /// set of pages aggregatedd from tags
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

        internal IEnumerable<TagPageSet> TagPage(IEnumerable<string> tags, TaggedPage page)
        {
            LinkedList<TagPageSet> appliedTags = new LinkedList<TagPageSet>();
            foreach (string tag in tags)
            {
                // get the tag from the pool or create one.
                TagPageSet tps;
                if (!Tags.TryGetValue(tag, out tps))
                {
                    tps = new TagPageSet(tag);
                    Tags.Add(tag, tps);
                }

                if (tps.AddPage(page))
                {
                    appliedTags.AddLast(tps);
                }
                page.Tags.Add(tps);
            }
            return appliedTags;
        }

        internal TaggedPage TagPage(IEnumerable<string> tags, string pageID, string pageTitle)
        {
            // get the page from the pool or create one
            TaggedPage tp;
            if (!Pages.TryGetValue(pageID, out tp))
            {
                tp = new TaggedPage(pageID, pageTitle);
                Pages.Add(pageID, tp);
            }
            else
            {
                tp.Title = pageTitle;
            }
            TagPage(tags, tp);

            return tp;
        }

        internal IEnumerable<TagPageSet> UntagPage(string tagName,string pageID)
        {
            LinkedList<TagPageSet> removed = new LinkedList<TagPageSet>();
            TaggedPage tp;
            if (Pages.TryGetValue(pageID, out tp))
            {
                TagPageSet t;
                if (Tags.TryGetValue(tagName, out t))
                {
                    if (tp.Tags.Remove(t))
                    {
                        removed.AddLast(t);
                    }
                    t.RemovePage(tp);
                }
            }
            return removed;
        }
    }
}
