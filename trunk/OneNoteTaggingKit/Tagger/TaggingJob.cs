// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.Tagger
{
    /// <summary>
    /// Tagging Operations
    /// </summary>
    public enum TagOperation
    {
        /// <summary>
        /// Union with existing page tags.
        /// </summary>
        UNITE,

        /// <summary>
        /// Subtraction of tags from page tags.
        /// </summary>
        SUBTRACT,

        /// <summary>
        /// Replace page tags.
        /// </summary>
        REPLACE
    }

    /// <summary>
    /// A tagging job to be performed in the background.
    /// </summary>
    public class TaggingJob
    {
        private string _pageid;
        private string[] _tags;
        private TagOperation _op;

        /// <summary>
        /// Create a new instance of a tagging job.
        /// </summary>
        /// <param name="pageID">ID of the OneNote page to apply the tags</param>
        /// <param name="tags">tags to apply</param>
        /// <param name="operation">operation</param>
        public TaggingJob(string pageID, string[] tags, TagOperation operation)
        {
            _pageid = pageID;
            _tags = tags;
            _op = operation;
        }

        internal OneNotePageProxy Execute(OneNoteProxy onenote, OneNotePageProxy page)
        {
            if (page == null)
            {
                page = new OneNotePageProxy(onenote, _pageid);
            }
            else if (!_pageid.Equals(page.PageID))
            {  // cannot continue with the given page
                page.Update();
                page = new OneNotePageProxy(onenote, _pageid);
            }

            TraceLogger.Log(TraceCategory.Info(), "Tagging page: {0}", page.Title);
            HashSet<string> pagetags = new HashSet<string>(page.PageTags);

            int countBefore = pagetags.Count;

            switch (_op)
            {
                case TagOperation.SUBTRACT:
                    pagetags.ExceptWith(_tags);
                    break;

                case TagOperation.UNITE:
                    pagetags.UnionWith(_tags);
                    break;

                case TagOperation.REPLACE:
                    pagetags.Clear();
                    pagetags.UnionWith(_tags);
                    break;
            }
            if ((pagetags.Count != countBefore) || _op == TagOperation.REPLACE)
            {
                string[] sortedTags = pagetags.ToArray();
                Array.Sort<string>(sortedTags, (x, y) => string.Compare(x, y, true));

                page.PageTags = sortedTags;
            }
            return page;
        }
    }
}