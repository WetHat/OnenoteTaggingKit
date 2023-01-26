using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    ///     Proxy object for `&lt;one:PageSettings&gt;` elements on a OneNote page document.
    /// </summary>
    public class PageSettings : PageStructureObjectBase
    {
        /// <summary>
        ///     Predicate to determine if a page uses the _reft-to-light_ input method.
        /// </summary>
        public bool IsRTL { get; private set; } 
        /// <summary>
        ///     Initialize a proxy object with a `PageSettings` XML element found on
        ///     a OneNote page XML document.
        /// </summary>
        /// <param name="page">Proxy for a OneNote page</param>
        /// <param name="element">The `one:PageSettings` element on the given OneNote page document.</param>
        public PageSettings(OneNotePage page, XElement element) : base(page, element) {
            string rtl = GetAttributeValue("RTL");
            if (!string.IsNullOrEmpty(rtl)) {
                IsRTL = bool.Parse(rtl);
            }
        }

        /// <summary>
        ///     Initialize the proxy object with a new instance of a `one:PageSettings`
        ///     element.
        /// </summary>
        /// <param name="page">Proxy for a OneNote page</param>
        public PageSettings(OneNotePage page) : base (page, new XElement(page.GetName(nameof(PageSettings)))) {
            page.Add(this,PageSchemaPosition.PageSettings); // add this to the page
        }
    }
}
