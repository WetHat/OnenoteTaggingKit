using System;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// The enumeration of process types a tag can participate in.
    /// </summary>
    public enum TagProcessClassification
    {
        /// <summary>
        /// A OneNote tag in the page title whose name is the list of page tags.
        /// </summary>
        InTitleMarker = 0,
        /// <summary>
        /// A OneNote tag marking a `one:OE` element containing a list of page tags.
        /// </summary>
        BelowTitleMarker,
        /// <summary>
        /// A OneNote tag marking a `one:OE` element containing a saved search.
        /// </summary>
        SavedSearchMarker,
        /// <summary>
        /// A regular, importable OneNote Paragrapg tag.
        /// </summary>
        OneNoteTag,
        /// <summary>
        /// A regular page tag managed by the Tagging Kit.
        /// </summary>
        PageTag
    }
    /// <summary>
    /// Proxy object for tag definitions.
    /// </summary>
    /// The XML representation of a tag definition on a OneNotePage looks like
    /// <code lang="xml">
    /// <one:TagDef index="0" name="Test Tag 1" type="0" symbol="0" />
    /// </code>
    public class TagDef : DefinitionObjectBase {
        /// <summary>
        /// The now obsolete name for below title marker tags
        /// </summary>
        public const string sLegacyBelowTitleMarkerName = "Page Tags";

        /// <summary>
        /// The symbol index for in-title and below-title markers.
        /// </summary>
        private const int cTaglistMarkerSymbol = 26;
        private const int cSavedSearchMarkerSymbol = 135;

        /// <summary>
        /// The OneNote tag name suffix to identify taglists.
        /// </summary>
        private const string cTaglistSuffix = "🔖";
        /// <summary>
        /// The OneNote tag name suffix to identify saved searches.
        /// </summary>
        private const string cSavedSearchSuffix = "🔍";

        private const int cInTitleMarkerType = 99;
        private const int cBelowTitleMarkerType = 23;
        private const int cSavedSearchMarkerType = 21;

        /// <summary>
        /// The name suffixes for process tags.
        /// </summary>
        static readonly string[] sProcessSuffixMap = new string[] {
            string.Empty, // InTitleMarker
            cTaglistSuffix, // BelowTitleMarker,
            cSavedSearchSuffix, // SavedSearchMarker
            string.Empty, // OneNoteTag
            string.Empty, // PageTag
        };

        int _type = -1;
        /// <summary>
        /// Get or set the OneNote tag type.
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
        /// Get OneNote tag display symbol index.
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

        PageTag _pageTag;
        /// <summary>
        /// The page tag underlying the definition.
        /// </summary>
        public PageTag Tag {
            get => _pageTag;
            set {
                if (value != null) {
                    Name = value.DisplayName;
                }
                _pageTag = value;
            }
        }

        /// <summary>
        /// Determine if two tag definitions are equal.
        /// </summary>
        /// <param name="obj">The _other_ tagdefinition.</param>
        /// <returns>``true` if both tags have the same index.</returns>
        public override bool Equals(object obj) {
            TagDef td = obj as TagDef;
            return td != null && td.Index == Index;
        }

        /// <summary>
        /// Get the hash code for this tag definition.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode() => Index.GetHashCode();

        /// <summary>
        /// Initialize a tag definition proxy object with a TagDef
        /// XML element from an OneNote page.
        /// </summary>
        /// <param name="page">Proxy of the page which owns this object.</param>
        /// <param name="element">TagDef element from an OneNote page</param>
        public TagDef(OneNotePage page, XElement element) : base (page,element) {
            _type = int.Parse(GetAttributeValue("type"));
            _symbol = int.Parse(GetAttributeValue("symbol"));
            if (_symbol == 0) {
                // looks like a page tag.
                _pageTag = new PageTag(Name, PageTagType.Unknown);
            }
        }

        /// <summary>
        /// Initialize a new instance of a tag definition proxy object.
        /// </summary>
        /// <param name="page">Proxy of the page which owns this object.</param>
        /// <param name="tag">The page tag for this definition</param>
        /// <param name="index">Definition index.</param>
        public TagDef(OneNotePage page, PageTag tag, int index)
            : base(page,new XElement(page.GetName(nameof(TagDef))), tag.DisplayName, index) {
            Type = index; // make type unique for page tags
            Symbol = 0;
            _pageTag = tag;
        }
        static string AnnotateName(string basename, TagProcessClassification classification) {
            switch (classification) {
                case TagProcessClassification.BelowTitleMarker:
                    return basename + cTaglistSuffix;
                case TagProcessClassification.SavedSearchMarker:
                    return basename + cSavedSearchSuffix;
            }
            return basename;
        }
        /// <summary>
        /// Intitialize a new instance of a process tag definition
        /// </summary>
        /// <param name="page">The OneNote page proxy for which to define a process tag.</param>
        /// <param name="basename">The tag name without any suffixes.</param>
        /// <param name="index">THe definition index</param>
        /// <param name="classification">The tag process classification.</param>
        public TagDef(OneNotePage page, string basename, int index, TagProcessClassification classification)
            : base(page, new XElement(page.GetName(nameof(TagDef))), AnnotateName(basename,classification), index) {
            switch (classification) {
                case TagProcessClassification.BelowTitleMarker:
                    Type = cBelowTitleMarkerType;
                    Symbol = cTaglistMarkerSymbol;
                    break;
                case TagProcessClassification.InTitleMarker:
                    Type = cInTitleMarkerType;
                    Symbol = cTaglistMarkerSymbol;
                    break;
                case TagProcessClassification.SavedSearchMarker:
                    Type = cSavedSearchMarkerType;
                    Symbol = cSavedSearchMarkerSymbol;
                    break;
            }
        }

        /// <summary>
        /// Get the process classification  of this tag definition.
        /// </summary>
        public TagProcessClassification ProcessClassification {
            get {
                if (_pageTag == null) {
                    switch (Symbol) {
                        case cTaglistMarkerSymbol:
                            switch (Type) {
                                case cBelowTitleMarkerType:
                                    return Name.Equals(sLegacyBelowTitleMarkerName)
                                           || Name.EndsWith(cTaglistSuffix)
                                           ? TagProcessClassification.BelowTitleMarker
                                           : TagProcessClassification.OneNoteTag;
                                case cInTitleMarkerType:
                                    return TagProcessClassification.InTitleMarker;
                            }
                            break;
                        case cSavedSearchMarkerSymbol:
                            return Name.EndsWith(cSavedSearchSuffix)
                                   ? TagProcessClassification.SavedSearchMarker
                                   : TagProcessClassification.OneNoteTag;
                        default:
                            return TagProcessClassification.OneNoteTag;
                    }
                }
                return TagProcessClassification.PageTag;
            }
        }

        /// <summary>
        /// Dispose a tag  definition.
        /// </summary>
        public override void Dispose() {
            base.Dispose();
            Tag = null;
        }
    }
}
