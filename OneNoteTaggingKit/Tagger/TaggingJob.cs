// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Collections.Generic;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.PageBuilder;

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
        private readonly PageTagSet _tagset;
        private readonly TagOperation _op;

        /// <summary>
        /// Create a new instance of a tagging job.
        /// </summary>
        /// <param name="pageID">   ID of the OneNote page to apply the tags</param>
        /// <param name="tags">The set of tags to apply</param>
        /// <param name="operation">operation</param>
        public TaggingJob(string pageID, PageTagSet tags, TagOperation operation)
        {
            _pageid = pageID;
            _tagset = tags;
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
        internal OneNotePage Execute(OneNoteProxy onenote, OneNotePage page)
        {
            if (page == null)
            {
                page = new OneNotePage(onenote, _pageid);
                if (page.IsDeleted)
                {
                    return null;
                }
            }
            else if (!_pageid.Equals(page.PageID))
            {  // cannot continue with the given page
                page.Update();
                page = new OneNotePage(onenote, _pageid);
            }
            switch (_op)
            {
                case TagOperation.SUBTRACT:
                    page.Tags.ExceptWith(_tagset);
                    break;
                case TagOperation.UNITE:
                    page.Tags.UnionWith(_tagset);
                    break;
                case TagOperation.REPLACE:
                    page.Tags = new PageTagSet(_tagset);
                    break;
                case TagOperation.RESYNC:
                    // Just resync the displayed tags with the recorded tags.
                    break;

            }
            return page;
        }
    }
}