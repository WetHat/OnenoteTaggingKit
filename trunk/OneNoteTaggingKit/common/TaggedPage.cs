﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.edit;

namespace WetHatLab.OneNote.TaggingKit.common
{

    /// <summary>
    /// Representation of an element in the hierarchy of the OneNote note tree
    /// </summary>
    /// <remarks>
    /// Collections of instances of this class are typically used to describe a path to a OneNote page
    /// </remarks>
    public class HierarchyElement : IKeyedItem<string>
    {
        /// <summary>
        /// create a new instance of an element in the OneNote object hierarchy.
        /// </summary>
        /// <param name="id">unique element id</param>
        /// <param name="name">user friendly element name</param>
        public HierarchyElement(string id, string name)
        {
            Key = id;
            Name = name;
        }

        /// <summary>
        /// Get the name of this element in the OneNote hierachy
        /// </summary>
        public string Name { get; private set; }

        #region IKeyedItem<string>
        /// <summary>
        /// get the unique key of this item
        /// </summary>
        public string Key
        {
            get;
            private set;
        }
        #endregion IKeyedItem<string>
    }

    /// <summary>
    /// Representation of a OneNote page with its page level tags.
    /// </summary>
    public class TaggedPage : IKeyedItem<string>
    {
        bool _isSelected = false;
        /// <summary>
        /// OneNote hierarchy path to this page
        /// </summary>
        IEnumerable<HierarchyElement> _path;
        /// <summary>
        /// Set of tags
        /// </summary>
        ISet<TagPageSet> _tags = new HashSet<TagPageSet>();
        /// <summary>
        /// page title
        /// </summary>
        string _title;

        /// <summary>
        /// Names of tags as recorded in the page's meta section;
        /// </summary>
        IEnumerable<string> _tagnames;

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
            XElement meta = page.Elements(one.GetName("Meta")).FirstOrDefault(m => OneNotePageProxy.META_NAME.Equals(m.Attribute("name").Value));
            if (meta != null)
            {
                _tagnames = OneNotePageProxy.ParseTags(meta.Attribute("content").Value);
            }
            else
            {
                _tagnames = new string[0];
            }
            
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


        /// <summary>
        /// get the page's ID
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Get the selection status of the page
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
        }

        /// <summary>
        /// Get the path to this OneNote page in the OneNote hierarchy.
        /// </summary>
        public IEnumerable<HierarchyElement> Path { get { return _path; } }

        /// <summary>
        /// Get the page's title.
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
        #region IKeyedItem
        /// <summary>
        /// Get pages unique key suitable for hashing
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
        /// get the ollections of tags as recorded in the pages metadata
        /// </summary>
        internal IEnumerable<string> TagNames
        {
            get
            {
                return _tagnames;
            }
        }

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
        /// Check two page objects for equality
        /// </summary>
        /// <param name="obj">other page object</param>
        /// <returns>true if equal; false otherwise</returns>
        public override bool Equals(object obj)
        {
            TaggedPage tp = obj as TaggedPage;

            return tp == null ? false : ID.Equals(tp.ID);
        }

        /// <summary>
        /// Compute the hashcode
        /// </summary>
        /// <returns>hashcode</returns>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
