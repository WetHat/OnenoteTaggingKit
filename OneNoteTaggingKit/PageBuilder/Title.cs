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
        /// Get the title's text content element.
        /// </summary>
        OET TitleContent { get; }

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
            TitleContent = new OET(element.Element(page.GetName(nameof(OE))));
        }

        /// <summary>
        /// Initialize the proxy object with a new instance of a `one:Title`
        /// element.
        /// </summary>
        /// <param name="page">Proxy for a OneNote page</param>
        /// <param name="title">page title</param>
        public Title(OneNotePage page, string title)
            : base(page,
                   new XElement(page.GetName(nameof(Title)))) {
            // Get the mandatory content element
            TitleContent = new OET(page.Namespace,title);
            Element.Add(TitleContent.Element);
        }
    }
}
