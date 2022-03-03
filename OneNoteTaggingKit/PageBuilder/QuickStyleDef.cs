using System;
using System.Drawing;
using System.Globalization;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy object class for `one:QuickStyleDef` style definition elements
    /// on a OneNote page document.
    /// </summary>
    public class QuickStyleDef : DefinitionObjectBase {

        System.Drawing.Font _styleFont;
        /// <summary>
        /// Get the font of this style.
        /// </summary>
        public System.Drawing.Font Font {
            get => _styleFont;
            set {
                _styleFont = value;
                SetAttributeValue("font", value.Name);
                SetAttributeValue("fontSize", value.SizeInPoints.ToString());
                SetAttributeValue("bold", value.Bold.ToString().ToLower());
                SetAttributeValue("italic", value.Italic.ToString().ToLower());
            }
        }

        /// <summary>
        /// Get/set the font color.
        /// </summary>
        public Color FontColor {
            get {
                var color = GetAttributeValue("fontColor");
                var converter = new ColorConverter();
                return color != null ? (Color)converter.ConvertFromString(color) : default(Color);
            }
            set  => SetAttributeValue("fontColor", "#" + value.R.ToString("X2")+value.G.ToString("X2")+value.B.ToString("X2"));
        }

        /// <summary>
        /// Initialize a new proxy object for a style definition element on
        /// a OneNote page document.
        /// </summary>
        /// <param name="page">Proxy of the OneNote page.</param>
        /// <param name="element">The style definition element selected from a OneNote page
        /// definition element.</param>
        public QuickStyleDef(OneNotePage page, XElement element) : base(page,element) {
            var style = FontStyle.Regular;

            if ("true".Equals(GetAttributeValue("bold"),StringComparison.InvariantCultureIgnoreCase)) {
                style |= FontStyle.Bold;
            }
            if ("true".Equals(GetAttributeValue("italic"), StringComparison.InvariantCultureIgnoreCase)) {
                style |= FontStyle.Italic;
            }
            string fontsize = GetAttributeValue("fontSize");
            float fontsizeem = "automatic".Equals(fontsize)
                ? 1
                : float.Parse(fontsize, CultureInfo.InvariantCulture);
            _styleFont = new Font(GetAttributeValue("font"),
                                  fontsizeem,
                                  style,
                                  GraphicsUnit.Point);
        }

        /// <summary>
        /// Initialize proxy object
        /// </summary>
        /// <param name="page">OneNote page proxy object</param>
        /// <param name="name">Style name.</param>
        /// <param name="index">Style definition index.</param>
        /// <param name="font">The style font</param>
        /// <param name="fontColor">Fint foreground color</param>
        public QuickStyleDef(OneNotePage page, string name, int index, System.Drawing.Font font, Color fontColor)
            : base(page,
                  new XElement(page.GetName(nameof(QuickStyleDef)),
                      new XAttribute("highlightColor", "automatic")),
                  name,
                  index) {
            Font = font;
            FontColor = fontColor;
        }
    }
}
