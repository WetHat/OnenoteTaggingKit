using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// A collection of tag definitions.
    /// </summary>
    /// <remarks>
    /// The collection does not compact. If 
    /// </remarks>
    public class TagDefCollection : PageStructureObjectCollection<TagDef>
    {
        /// <summary>
        /// Intitialize the collection of Meta tags from an OneNote page.
        /// </summary>
        /// <param name="page">The OneNote page XML document.</param>
        public TagDefCollection(XDocument page) : base(page, PagePosition.TagDef) {
        }

        protected override TagDef CreateElement(XElement e) {
            return new TagDef(e);
        }
    }
}
