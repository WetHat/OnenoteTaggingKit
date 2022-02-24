using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WetHatLab.OneNote.TaggingKit.PageBuilder
{
    /// <summary>
    /// Proxy class for text content on a OneNote page.
    /// </summary>
    public class OET : OE
    {

        /// <summary>
        /// Get or set of a text content element.
        /// </summary>
        public string Text {
            get {
                var txt = new StringBuilder("");

                foreach (var t in Element.Elements(GetName("T"))) {
                    txt.Append(t.Value);
                }
                return txt.ToString();
            }
            set {
                XName tn = GetName("T");
                XElement t = Element.Element(tn);
                while (t != null) {
                    t.Remove();
                    t = Element.Element(tn);
                }
                // Create a new text node with the given text
                Element.Add(new XElement(tn, new XCData(value)));
            }
        }
        /// <summary>
        /// Initialize proxy instance from text content in a OneNote page.
        /// </summary>
        /// <param name="element">A `one:OE` element containing text.</param>
        public OET(XElement element) : base(element) {

        }

        /// <summary>
        /// Initialize a new instance of a text content proxy with data from
        /// a generic elelment proxy.
        /// </summary>
        /// <param name="source">The proxy sourcing the data.</param>
        protected OET(OE source) : base(source.Element) {

        }

        /// <summary>
        /// Initialize a new text content proxy with a given text.
        /// </summary>
        /// <param name="ns">The XML namespace to use</param>
        /// <param name="text">Text content.</param>
        public OET(XNamespace ns, string text)
            : base(ns, new XElement(new XElement(ns.GetName("T"),
                                        new XCData(text)))) {

        }
    }
}
