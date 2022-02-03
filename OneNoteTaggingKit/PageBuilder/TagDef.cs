using System;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy object for tag definitions.
    /// </summary>
    /// The XML representation of a tag definition on a OneNotePage looks like
    /// <code lang="xml">
    /// <one:TagDef index="0" name="Test Tag 1" type="0" symbol="0" />
    /// </code>
    public class TagDef : DefinitionObjectBase {
        int _type = -1;
        /// <summary>
        /// Get the tag type.
        /// </summary>
        public int Type {
            get => _type;
            set {
                if (_type != value) {
                    _type = value;
                    SetAttributeValue("type", value.ToString());
                }
            }
        }

        int _symbol = -1;
        /// <summary>
        /// Get tag symbol index.
        /// </summary>
        public int Symbol {
            get => _symbol;
            set {
                if (_symbol != value) {
                    _symbol = value;
                    SetAttributeValue("symbol", value.ToString());
                }
            }
        }

        /// <summary>
        /// The tag name without type annotation.
        /// </summary>
        public string TagName { get; private set; }

        static readonly string[] sTypePriority = new string[] {
            string.Empty, // genuine page tags
            Properties.Settings.Default.ImportOneNoteTagMarker,
            Properties.Settings.Default.ImportOneNoteTagMarker,
        };

        string _tagType = string.Empty;
        /// <summary>
        /// Get/set the tag's type indicator.
        /// </summary>
        public string TagType {
            get => _tagType;
            set {
                if (!_tagType.Equals(value)
                    && (Symbol != 0
                       || Array.IndexOf<string>(sTypePriority, _tagType) > Array.IndexOf<string>(sTypePriority, value))) {
                    _tagType = value;
                    Name = TagName + value;
                }
            }
        }

        /// <summary>
        /// Initialize a tag definition proxy object with a TagDef
        /// XML element from an OneNote page.
        /// </summary>
        /// <param name="page">Proxy of the page which owns this object.</param>
        /// <param name="element">TagDef element from an OneNote page</param>
        public TagDef(OneNotePage page, XElement element) : base (page,element) {
            _type = int.Parse(GetAttributeValue("type"));
            _symbol = int.Parse(GetAttributeValue("symbol"));
            var parsed = TagPageSet.ParseTagName(Name);
            TagName = parsed.Item1;
            _tagType = parsed.Item2;
        }

        /// <summary>
        /// Initialize a new instance of a Meta proxy object for a given
        /// key/value pair.
        /// </summary>
        /// <param name="page">Proxy of the page which owns this object.</param>
        /// <param name="tagname">Tag basename without type annotation</param>
        /// <param name="index">Definition index.</param>
        /// <param name="tagtype">Tag type annotation</param>
        /// <param name="type">Tag type index</param>
        /// <param name="symbol">tag symbol.</param>
        internal TagDef(OneNotePage page, string tagname, int index, string tagtype, int type, int symbol)
            : base(page,new XElement(page.GetName(nameof(TagDef))), tagname + tagtype, index) {
            Type = type;
            Symbol = symbol;
            TagName = tagname;
            TagType = tagtype;
        }
    }
}
