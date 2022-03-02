using System.Windows.Media;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Colelction of style definition proxy objects.
    /// </summary>
    public class QuickStyleDefCollection : DefinitionObjectCollection<QuickStyleDef> {

        QuickStyleDef _tagStyle;
        /// <summary>
        /// Get the Style for below-title tag lists.
        /// </summary>
        public QuickStyleDef TagOutlineStyleDef {
            get {
                if (_tagStyle == null) {
                    // new XAttribute("index", (quickstyleDefs.Count() + 1).ToString()),
                    //                            new XAttribute("name", Properties.Settings.Default.TagOutlineStyle_Name),
                    //                            new XAttribute("fontColor", "#595959"),
                    //                            new XAttribute("highlightColor", "automatic"),
                    //                            new XAttribute("bold", Properties.Settings.Default.TagOutlineStyle_Font.Bold.ToString().ToLower()),
                    //                            new XAttribute("italic", Properties.Settings.Default.TagOutlineStyle_Font.Italic.ToString().ToLower()),
                    //                            new XAttribute("font", Properties.Settings.Default.TagOutlineStyle_Font.Name),
                    //                            new XAttribute("fontSize", Propertie
                    _tagStyle = new QuickStyleDef(Page,
                                                  Properties.Settings.Default.TagOutlineStyle_Name,
                                                  Items.Count,
                                                  Properties.Settings.Default.TagOutlineStyle_Font,
                                                  Brushes.Black.Color);

                    Add(_tagStyle);
                }
                return _tagStyle;
            }
        }

        QuickStyleDef _labelStyleDef;
        /// <summary>
        /// Get the label style definition.
        /// </summary>
        public QuickStyleDef LabelStyleDef {
            get {
                if (_labelStyleDef == null) {
                    _labelStyleDef = new QuickStyleDef(Page,
                                                 "LabelStyle",
                                                 Items.Count,
                                                 Properties.Settings.Default.LabelStyle_Font,
                                                 Brushes.Black.Color);
                    Add(_labelStyleDef);
                }
                return _labelStyleDef;
            }
        }

        /// <summary>
        /// Initialize an instance of this collection for elements with a
        /// specified XML name found on a OneNote page XML document .
        /// </summary>
        /// <param name="page">The OneNote page Xml document.</param>
        public QuickStyleDefCollection(OneNotePage page) : base (page.GetName(nameof(QuickStyleDef)),page) {
        }

        /// <summary>
        /// Create a new style definition for an XML style definition found
        /// on a OneNote page document.
        /// </summary>
        /// <param name="e">A `one:QuickStyleDef` XML element.</param>
        /// <returns></returns>
        protected override QuickStyleDef CreateElementProxy(XElement e) {
            var def = new QuickStyleDef(Page, e);
            if (Properties.Settings.Default.TagOutlineStyle_Name.Equals(def.Name)) {
                _tagStyle = def;
            }
            return def;
        }
    }
}
