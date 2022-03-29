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
    public class PageNode : HierarchyNode
    {
        /// <summary>
        /// Supported taglist separators
        /// </summary>
        readonly static char[] sTagListSeparators = new char[] { ',' };

        /// <summary>
        /// Parse a comma separated list of tags.
        /// </summary>
        /// <remarks>
        /// This function does **not** handle HTML markup in the taglist.
        /// </remarks>
        /// <param name="taglist">Array of tags.</param>
        /// <returns>Array of parsed tags.</returns>
        public static string[] ParseTaglist(string taglist) {
            var tags =  taglist.Split(sTagListSeparators, StringSplitOptions.RemoveEmptyEntries);
            // trim tags
            for(int i = 0; i < tags.Length; i++) {
                tags[i] = tags[i].Trim();
            }
            return tags;
        }
        private readonly bool _isSelected = false;

        /// <summary>
        /// Set of tags
        /// </summary>
        private readonly ISet<TagPageSet> _tags = new HashSet<TagPageSet>();

        /// <summary>
        /// Create an internal representation of a page returned from FindMeta
        /// </summary>
        /// <param name="page">&lt;one:Page&gt; element</param>
        /// <param name="parent">The parent node of the page.</param>
        internal PageNode(XElement page,HierarchyNode parent) : base(page,parent,HierarchyElement.hePages)  {
            XNamespace one = page.Name.Namespace;
            var rbin = page.Attribute("isInRecycleBin");
            IsInRecycleBin = "true".Equals(rbin != null ? rbin.Value : String.Empty);
            XAttribute selected = page.Attribute("selected");
            if (selected != null && "all".Equals(selected.Value)) {
                _isSelected = true;
            }
            XElement meta = page.Elements(one.GetName("Meta")).FirstOrDefault(m => MetaCollection.PageTagsMetaKey.Equals(m.Attribute("name").Value));
            if (meta != null) {
                Tags = new PageTagSet(meta.Attribute("content").Value,TagFormat.AsEntered);
            } else {
                Tags = new PageTagSet();
            }
        }

        /// <summary>
        /// Determine if the tagged pages is recycled
        /// </summary>
        /// <value>'true' if page is recycled; 'false' if page is still alive</value>
        public bool IsInRecycleBin { get; private set; }

        /// <summary>
        /// Get the selection status of the page
        /// </summary>
        public bool IsSelected {
            get { return _isSelected; }
        }

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
        /// Get the collection of tags on this page
        /// </summary>
        public PageTagSet Tags { get; }

        /// <summary>
        /// Check two page objects for equality
        /// </summary>
        /// <param name="obj">other page object</param>
        /// <returns>true if equal; false otherwise</returns>
        public override bool Equals(object obj) {
            PageNode tp = obj as PageNode;

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