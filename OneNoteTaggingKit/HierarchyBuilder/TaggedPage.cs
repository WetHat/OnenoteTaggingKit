// Author: WetHat | (C) Copyright 2013 - 2022 WetHat Lab, all rights reserved
using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.PageBuilder;

namespace WetHatLab.OneNote.TaggingKit.HierarchyBuilder
{
    /// <summary>
    /// Representation of a OneNote page with its page level tags.
    /// </summary>
    public class TaggedPage : HierarchyNode
    {
        readonly static char[] sTagListSeparator = new char[] { ',' };

        /// <summary>
        /// Parse a comma separated list of tags.
        /// </summary>
        /// <remarks>
        /// This function does **not** handle HTML markup in the taglist.
        /// </remarks>
        /// <param name="taglist">Array of tags.</param>
        /// <returns>Array of parsed tags.</returns>
        public static string[] ParseTaglist(string taglist) {
            var tags =  taglist.Split(sTagListSeparator, StringSplitOptions.RemoveEmptyEntries);
            // trim tags
            for(int i = 0; i < tags.Length; i++) {
                tags[i] = tags[i].Trim();
            }
            return tags;
        }
        private readonly bool _isSelected = false;

        /// <summary>
        /// OneNote hierarchy path to this page
        /// </summary>
        private readonly IEnumerable<HierarchyNode> _path;

        /// <summary>
        /// Set of tags
        /// </summary>
        private readonly ISet<TagPageSet> _tags = new HashSet<TagPageSet>();

        /// <summary>
        /// page title
        /// </summary>
        private string _title;

        /// <summary>
        /// Names of tags as recorded in the page's meta section;
        /// </summary>
        private readonly IEnumerable<string> _tagnames;

        /// <summary>
        /// Create an internal representation of a page returned from FindMeta
        /// </summary>
        /// <param name="page">&lt;one:Page&gt; element</param>
        internal TaggedPage(XElement page) : base(page,HierarchyElement.hePages)  {
            XNamespace one = page.GetNamespaceOfPrefix("one");
            var rbin = page.Attribute("isInRecycleBin");
            IsInRecycleBin = "true".Equals(rbin != null ? rbin.Value : String.Empty);
            XAttribute selected = page.Attribute("selected");
            if (selected != null && "all".Equals(selected.Value)) {
                _isSelected = true;
            }
            XElement meta = page.Elements(one.GetName("Meta")).FirstOrDefault(m => MetaCollection.PageTagsMetaKey.Equals(m.Attribute("name").Value));
            if (meta != null) {
                _tagnames = ParseTaglist(meta.Attribute("content").Value);
            } else {
                _tagnames = new string[0];
            }

            // build the items path
            LinkedList<HierarchyNode> path = new LinkedList<HierarchyNode>();
            XElement e = page;
            while (e.Parent != null) {
                e = e.Parent;
                path.AddFirst(new HierarchyNode(e));
            }
            _path = path;
        }

        /// <summary>
        /// Determine if the tagged pages is recycled
        /// </summary>
        /// <value>'true' if page is recycled; 'false' if page is still alive</value>
        public bool IsInRecycleBin { get; private set; }

        /// <summary>
        /// get the page's ID
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Get the selection status of the page
        /// </summary>
        public bool IsSelected {
            get { return _isSelected; }
        }

        /// <summary>
        /// Get the path to this OneNote page in the OneNote hierarchy.
        /// </summary>
        public IEnumerable<HierarchyNode> Path { get { return _path; } }


        #region IKeyedItem

        /// <summary>
        /// Get pages unique key suitable for hashing
        /// </summary>
        public string Key {
            get {
                return ID;
            }
        }

        #endregion IKeyedItem

        /// <summary>
        /// Get the collection of tags as recorded in the page's metadata.
        /// </summary>
        internal IEnumerable<string> TagNames {
            get {
                return _tagnames;
            }
        }

        /// <summary>
        /// Get the collection of tags on this page
        /// </summary>
        internal ISet<TagPageSet> Tags {
            get {
                return _tags;
            }
        }

        /// <summary>
        /// Check two page objects for equality
        /// </summary>
        /// <param name="obj">other page object</param>
        /// <returns>true if equal; false otherwise</returns>
        public override bool Equals(object obj) {
            TaggedPage tp = obj as TaggedPage;

            return tp == null ? false : ID.Equals(tp.ID);
        }

        /// <summary>
        /// Compute the hashcode
        /// </summary>
        /// <returns>hashcode</returns>
        public override int GetHashCode() {
            return ID.GetHashCode();
        }
    }
}