﻿// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
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
        REPLACE,
        /// <summary>
        /// Resynchronize tags with the internal tag data.
        /// </summary>
        RESYNC
    }

    /// <summary>
    /// A tagging job to be performed in the background.
    /// </summary>
    public class TaggingJob
    {
        private string _pageid;
        private readonly string[] _tags;
        private readonly TagOperation _op;

        /// <summary>
        /// Create a new instance of a tagging job.
        /// </summary>
        /// <param name="pageID">   ID of the OneNote page to apply the tags</param>
        /// <param name="tags">     tags to apply</param>
        /// <param name="operation">operation</param>
        public TaggingJob(string pageID, string[] tags, TagOperation operation)
        {
            _pageid = pageID;
            _tags = tags;
            _op = operation;
        }

        /// <summary>
        /// Tag a singe OneNote page
        /// </summary>
        /// <remarks>
        /// A tagged page is not saved immediately. The caller can hold on to a previously
        /// returned page and pass it into this method again. This avoids saving a page
        /// multiple times, if there are subsequent tagging jobs for the same page. If the
        /// ID of the page passed into this method does not match the ID of this job, the
        /// passed in page is saved.
        /// </remarks>
        /// <param name="onenote">OneNote application proxy object</param>
        /// <param name="page">   an unsaved OneNote page which has been tagged previously</param>
        /// <returns>Unsaved, tagged OneNote page.</returns>
        internal OneNotePageProxy Execute(OneNoteProxy onenote, OneNotePageProxy page)
        {
            if (page == null)
            {
                page = new OneNotePageProxy(onenote, _pageid);
                if (page.IsDeleted)
                {
                    return null;
                }
            }
            else if (!_pageid.Equals(page.PageID))
            {  // cannot continue with the given page
                page.Update();
                page = new OneNotePageProxy(onenote, _pageid);
            }

            // collect the genuine page tags
            HashSet<string> pagetags = new HashSet<string>(from name in page.PageTags
                                                           where !name.EndsWith(Properties.Settings.Default.ImportOneNoteTagMarker)
                                                                 && !name.EndsWith(Properties.Settings.Default.ImportHashtagMarker)
                                                           select name);

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
                case TagOperation.RESYNC:
                    // Just resync the displayed tags with the recorded tags.
                    break;

            }
            if (Properties.Settings.Default.MapOneNoteTags) {
                // add the OneNote tags with import marker appended
                pagetags.UnionWith(from ot in page.OneNoteTags select ot + Properties.Settings.Default.ImportOneNoteTagMarker);
            }
            if (Properties.Settings.Default.MapHashTags) {
                // add the OneNote tags with import marker appended
                pagetags.UnionWith(from ot in page.HashTags select ot + Properties.Settings.Default.ImportHashtagMarker);
            }
            string[] sortedTags = pagetags.ToArray();
            Array.Sort<string>(sortedTags, (x, y) => string.Compare(x, y, true));

            page.PageTags = sortedTags;
            return page;
        }
    }
}