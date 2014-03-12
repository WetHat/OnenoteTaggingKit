using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// Representation of a OneNote page which has page tags.
    /// </summary>
    public class TaggedPage : IKeyedItem<string>
    {
        private string _title;

        /// <summary>
        /// get the page's ID
        /// </summary>
        public string ID { get; private set; }
        /// <summary>
        /// get the page's title
        /// </summary>
        public string Title {
            get
            {
                return _title;
            }
            private set
            {
                _title = value ?? String.Empty;
            }
        }

        #region IKeyedItem
        /// <summary>
        /// Get a unique key suitable for sorting
        /// </summary>
        public string Key
        {
            get
            {
                return  ID;
            }
        }
        #endregion IKeyedItem

        /// <summary>
        /// 
        /// </summary>
        internal string[] Tags {get; private set;}

        /// <summary>
        /// Create an internal representation of a page returned from FindMeta
        /// </summary>
        /// <param name="page">&lt;one:Page&gt; element</param>
        internal TaggedPage(XElement page)
        {
            XNamespace one = page.GetNamespaceOfPrefix("one");
            ID = page.Attribute("ID").Value;
            Title = page.Attribute("name").Value;

            XElement meta = page.Elements(one.GetName("Meta")).FirstOrDefault( m =>  OneNotePageProxy.META_NAME.Equals(m.Attribute("name").Value) );

            if (meta != null)
            {
                Tags = OneNotePageProxy.ParseTags(meta.Attribute("content").Value);
            }
            else
            {
                Tags = new string[0];
            }
        }

        /// <summary>
        /// Compute the hashcode
        /// </summary>
        /// <returns>hashcode</returns>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        /// <summary>
        /// Check two page objects for equality
        /// </summary>
        /// <param name="obj">other page object</param>
        /// <returns>true if equal; false otherwise</returns>
        public override bool Equals(object obj)
        {
            TaggedPage tp = obj as TaggedPage;

            return tp == null ? false : ID.Equals(tp.ID); 
        }
    }
}
