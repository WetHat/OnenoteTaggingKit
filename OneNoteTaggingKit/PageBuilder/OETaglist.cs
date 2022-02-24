using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.HierarchyBuilder;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// A OneNote paragraph proxy containing a comma separated list of tags.
    /// </summary>
    public class OETaglist : OET
    {
        /// <summary>
        /// Regular expression to match some HTML tags
        /// </summary>
        public static readonly Regex HTMLtag_matcher = new Regex(@"</*(a|span)[^<>]*>", RegexOptions.Compiled);

        /// <summary>
        /// Get the  genuine page tags represented tags represented by this paragraph.
        /// </summary>
        /// <value>The collection of names of tags which do not have an import type annotation.</value>
        public IEnumerable<string> PageTags {
            get {
                string taglist = Text;
                if (!string.IsNullOrEmpty(taglist)) {
                    foreach (string rawtag in TaggedPage.ParseTaglist(Element.Value)) {
                        if (!rawtag.Contains("&#")) { // no Emoji HTML entities
                            var tag = TagPageSet.ParseTagName(HTMLtag_matcher.Replace(rawtag, string.Empty));
                            if (string.IsNullOrEmpty(tag.Item2)) {
                                // found a genuine page tag
                                yield return tag.Item1; // return basename
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set the comma separated list of tags.
        /// </summary>
        /// <value>Tags with type annotations are included in this list.</value>
        public IEnumerable<TagDef> Taglist {
            set {
                Text = string.Join(", ", from TagDef td in value select td.Name);
            }
        }
        /// <summary>
        /// Initialize a taglist paragraph proxy
        /// </summary>
        /// <param name="source"></param>
        public OETaglist(OE source) : base(source) {
            // turn spellcheck off for tag lists.
            Element.SetAttributeValue("lang", "yo");
        }

        /// <summary>
        /// Initialize a proxy instance with a new OneNote paragraph
        /// containing a comma separated list of tags.
        /// </summary>
        /// <param name="marker">OneNote tag to mark this tag.</param>
        /// <param name="style">Style to use for the tag list.</param>
        /// <param name="taglist">comma separated list of tags.</param>
        public OETaglist(TagDef marker, QuickStyleDef style, IEnumerable<TagDef> taglist) :
            base(marker.Namespace,
                string.Join(", ",from TagDef td in taglist select td.Name)) {
            Tags.Add(marker);
            QuickStyleIndex = style.Index;
            Element.SetAttributeValue("lang", "yo");
        }
    }
}
