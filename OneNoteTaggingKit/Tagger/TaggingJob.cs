﻿// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
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
        ///     Union with existing page tags.
        /// </summary>
        UNITE,

        /// <summary>
        ///     Subtraction of tags from page tags.
        /// </summary>
        SUBTRACT,

        /// <summary>
        ///     Replace page tags.
        /// </summary>
        REPLACE,
        /// <summary>
        ///     Resynchronize tags with the internal tag data, re-import tags on page content, and
        ///     update saved searches.
        /// </summary>
        RESYNC,

        /// <summary>
        ///     Do nothing.
        /// </summary>
        NOOP
    }

    /// <summary>
    /// A tagging job to be performed in the background.
    /// </summary>
    public class TaggingJob
    {
        private string _pageid;
        private readonly PageTagSet _tagset;

        /// <summary>
        /// Get the type of operation this job performs.
        /// </summary>
        public TagOperation OperationType { get; }
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
            OperationType = operation;
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
            if (page == null)  {
                page = new OneNotePage(onenote, _pageid);
                if (page.IsDeleted) {
                    return null;
                }
            }
            else if (!_pageid.Equals(page.PageID))
            {  // cannot continue with the given page
                page.Update();
                page = new OneNotePage(onenote, _pageid);
            }
            switch (OperationType)
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
                    page.Update(true);
                    // do re-use this page after because there are may be
                    // a lot of changes with conflict potential.
                    page = null; 
                    break;
            }
            TraceLogger.Log(TraceCategory.Info(), "Background job executed successfully: {0}", this);
            TraceLogger.Flush();
            return page;
        }

        /// <summary>
        ///     Human readable string representation of a job.
        /// </summary>
        /// <returns>String suitable for debugging or logging.</returns>
        public override string ToString() {
            return string.Format("{0} Page={1}", OperationType, _pageid);
        }
    }
}