using System.Drawing;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Collection of style definition proxy objects.
    /// </summary>
    [ComVisible(false)]
    public class QuickStyleDefCollection : DefinitionObjectCollection<QuickStyleDef> {
        #region Tagstyle
        const string TagstyleName = "PagetagsStyle";
        QuickStyleDef _tagStyle;
        /// <summary>
        /// Get the Style for below-title tag lists.
        /// </summary>
        public QuickStyleDef TagOutlineStyleDef {
            get {
                if (_tagStyle == null) {
                    _tagStyle = new QuickStyleDef(Page,
                                                  TagstyleName,
                                                  Items.Count,
                                                  new Font("Calibri",
                                                            9,
                                                            FontStyle.Regular,
                                                            GraphicsUnit.Point),
                                                  Color.Black);

                    Add(_tagStyle);
                }
                return _tagStyle;
            }
        }
        #endregion Tagstyle
        #region Labelstyle
        const string LabelstyleName = "LabelStyle";
        QuickStyleDef _labelStyleDef;
        /// <summary>
        /// Get the label style definition.
        /// </summary>
        public QuickStyleDef LabelStyleDef {
            get {
                if (_labelStyleDef == null) {
                    _labelStyleDef = new QuickStyleDef(Page,
                                                 LabelstyleName,
                                                 Items.Count,
                                                 new Font("Segoe UI",
                                                          10,
                                                          FontStyle.Bold,
                                                          GraphicsUnit.Point),
                                                 Color.Black);
                    Add(_labelStyleDef);
                }
                return _labelStyleDef;
            }
        }
        #endregion Labelstyle
        #region BreadcrumbStyle
        const string BreadcrumbstyleName = "breadcrumb";
        QuickStyleDef _breadcrumbStyleDef;
        /// <summary>
        ///     Get the breadcrumb (path) style definition.
        /// </summary>
        public QuickStyleDef BreadcrumbStyleDef {
            get {
                if (_breadcrumbStyleDef == null) {
                    _breadcrumbStyleDef = new QuickStyleDef(Page,
                                                            BreadcrumbstyleName,
                                                            Items.Count,
                                                            new Font("Segoe UI Symbol",
                                                                    10,
                                                                    FontStyle.Bold,
                                                                    GraphicsUnit.Point),
                                                            Color.FromArgb(0x000000));
                    Add(_breadcrumbStyleDef);
                }
                return _breadcrumbStyleDef;
            }
        }
        #endregion Citationstyle
        /// <summary>
        /// Initialize an instance of this collection for elements with a
        /// specified XML name found on a OneNote page XML document .
        /// </summary>
        /// <param name="page">The OneNote page Xml document.</param>
        public QuickStyleDefCollection(OneNotePage page) : base (page.GetName(nameof(QuickStyleDef)),page,PageSchemaPosition.QuickStyleDef) {
        }

        /// <summary>
        /// Create a new style definition for an XML style definition found
        /// on a OneNote page document.
        /// </summary>
        /// <param name="styledef">A `one:QuickStyleDef` XML element.</param>
        /// <returns></returns>
        protected override QuickStyleDef CreateElementProxy(XElement styledef) {
            var def = new QuickStyleDef(Page, styledef);
            switch (def.Name) {
                case TagstyleName:
                    _tagStyle = def;
                    break;
                case LabelstyleName:
                    _labelStyleDef = def;
                    break;
                case BreadcrumbstyleName:
                    _breadcrumbStyleDef = def;
                    break;
            }
            return def;
        }
    }
}
