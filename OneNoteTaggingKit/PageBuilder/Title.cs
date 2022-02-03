using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{

    /// <summary>
    /// Proxy object for 'one:Title' elements on a OneNote page document.
    /// </summary>
    public class Title : PageStructureObjectBase {

        /// <summary>
        /// Get the title's content element.
        /// </summary>
        OE TitleContent { get; }

        /// <summary>
        /// Get the title tags.
        /// </summary>
        public TagCollection Tags => TitleContent.Tags;

        /// <summary>
        /// Initialize a proxy object with a title XML element selected from
        /// a OneNote page XML document.
        /// </summary>
        /// <param name="page">Proxy for a OneNote page</param>
        /// <param name="element">The `one:Title` element on a OneNote page document.</param>
        public Title(OneNotePage page,XElement element) : base(page, element) {
            TitleContent = new OE(element.Element(page.GetName(nameof(OE))));
        }

        /// <summary>
        /// Initialize the proxy object with a new instance of a `one:Title`
        /// element.
        /// </summary>
        /// <param name="page">Proxy for a OneNote page</param>
        /// <param name="title">page title</param>
        public Title(OneNotePage page, string title)
            : base(page,
                   new XElement(page.GetName(nameof(Title)),
                       new XElement(page.GetName(nameof(OE)),
                           new XElement(page.GetName("T"),title)))) {
            // Get the mandatory content element
            TitleContent = new OE(Element.FirstNode as XElement);
        }
    }
}
