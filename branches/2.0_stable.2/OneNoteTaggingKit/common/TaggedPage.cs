using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.common
{
    public class HierarchyElement : IKeyedItem<string>
    {
        public HierarchyElement(string id, string name)
        {
            Key = id;
            Name = name;
        }
        public string Name { get; private set; }
        #region IKeyedItem<string>
        public string Key
        {
            get;
            private set;
        }
        #endregion IKeyedItem<string>
    }

    /// <summary>
    /// Representation of a OneNote page which has page tags.
    /// </summary>
    public class TaggedPage : IKeyedItem<string>
    {
        string _title;
        ISet<TagPageSet> _tags = new HashSet<TagPageSet>();
        bool _isSelected = false;
        IEnumerable<HierarchyElement> _path;

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
            set
            {
                _title = value ?? String.Empty;
            }
        }

        public IEnumerable<HierarchyElement> Path { get { return _path; } }

        public bool IsSelected
        {
            get { return _isSelected; }
        }

        #region IKeyedItem
        /// <summary>
        /// Get pages unique key suitable for hasing
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
        /// Get or set the collection of tags on this page
        /// </summary>
        internal ISet<TagPageSet> Tags
        {
            get
            {
                return _tags;
            }
        }


        /// <summary>
        /// Create an internal representation of a page returned from FindMeta
        /// </summary>
        /// <param name="page">&lt;one:Page&gt; element</param>
        internal TaggedPage(XElement page)
        {
            XNamespace one = page.GetNamespaceOfPrefix("one");
            ID = page.Attribute("ID").Value;
            Title = page.Attribute("name").Value;

            XAttribute selected = page.Attribute("selected");
            if (selected != null && "all".Equals(selected.Value))
            {
                _isSelected = true;
            }
            XElement meta = page.Elements(one.GetName("Meta")).FirstOrDefault( m =>  OneNotePageProxy.META_NAME.Equals(m.Attribute("name").Value) );

            // build the items path
            LinkedList<HierarchyElement> path = new LinkedList<HierarchyElement>();
            XElement e = page;
            while (e.Parent != null)
            {
                e = e.Parent;
                XAttribute id = e.Attribute("ID");
                XAttribute name = e.Attribute("name");
                if (id == null || name == null)
                {
                    break;
                }
                path.AddFirst(new HierarchyElement(e.Attribute("ID").Value, e.Attribute("name").Value));
            }
            _path = path;
        }

        internal TaggedPage(string id, string title)
        {
            ID = id;
            Title = title;
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
