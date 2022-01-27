using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy object for tag definitions.
    /// </summary>
    public class TagDef : DefinitionObjectBase {

        int _type = -1;
        /// <summary>
        /// Get the tag type.
        /// </summary>
        public int Type {
            get => _type;
            private set {
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
        /// Initialize a tag definition proxy object with a TagDef
        /// XML element from an OneNote page.
        /// </summary>
        /// <param name="element">TagDef element from an OneNote page</param>
        public TagDef(XElement element) : base (element) {
            _type = int.Parse(GetAttributeValue("type"));
            _symbol = int.Parse(GetAttributeValue("symbol"));
        }

        /// <summary>
        /// Initialize a new instance of a Meta proxy object for a given
        /// key/value pair.
        /// </summary>
        /// <param name="ns">The Xml NameSpace to use.</param>
        /// <param name="key">Unique name</param>
        /// <param name="index">Element index</param>
        /// <param name="type">tag type index</param>
        /// <param name="symbol">tag symbol.</param>
        public TagDef(XNamespace ns, string key, int index, int type, int symbol)
            : base(ns.GetName("TagDef"), index, key) {
            Type = type;
            Symbol = symbol;
        }
    }
}
