using System.Collections.Generic;
using System.Linq;
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
        /// Set the comma separated list of tags.
        /// </summary>
        public string Taglist {
            get => HTMLtag_matcher.Replace(Text, string.Empty);
            set => Text = value;
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
        /// Initialize a proxy with a new OneNote paragraph
        /// containing a comma separated list of tags.
        /// </summary>
        /// <param name="ns">Namespace to create the tag llist in.</param>
        /// <param name="taglist">Comma separated list of tags.</param>
        /// <param name="style">The style to use for this taglist.</param>
        public OETaglist(XNamespace ns, string taglist, QuickStyleDef style = null) :
            base(ns, taglist, style) {
            Element.SetAttributeValue("lang", "yo");
        }
    }
}
