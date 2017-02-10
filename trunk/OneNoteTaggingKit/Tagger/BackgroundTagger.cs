﻿// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.Tagger
{
    /// <summary>
    /// Background OneNote page tagger.
    /// </summary>
    public class BackgroundTagger : IDisposable
    {
        private OneNoteProxy _onenote;

        private BlockingCollection<TaggingJob> _jobs = new BlockingCollection<TaggingJob>();

        private CancellationTokenSource _cancel;

        /// <summary>
        /// Create a new instance of a background page tagger.
        /// </summary>
        /// <param name="onenote">OneNote application proxy object</param>
        public BackgroundTagger(OneNoteProxy onenote)
        {
            _onenote = onenote;
            _cancel = new CancellationTokenSource();
        }

        /// <summary>
        /// Schedule a tagging job for background operation.
        /// </summary>
        /// <param name="job">Tagging job descriptor</param>
        public void Add(TaggingJob job)
        {
            _jobs.Add(job);
        }

        /// <summary>
        /// Run the background tagger.
        /// </summary>
        /// <returns></returns>
        public Task Run()
        {
            TaskFactory tf = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);
            CancellationToken cancel = _cancel.Token;
            return tf.StartNew(() =>
            {
                TraceLogger.Log(TraceCategory.Info(), "Background tagging service started");
                try
                {
                    OneNotePageProxy lastPage = null;
                    while (!_jobs.IsCompleted)
                    {
                        TaggingJob j = _jobs.Take();
                        cancel.ThrowIfCancellationRequested();
                        try
                        {
                            lastPage = j.Execute(_onenote, lastPage);
                            if (lastPage != null && _jobs.Count == 0)
                            { // no more pending pages - must update the last one and stop carrying forward
                                lastPage.Update();
                                lastPage = null;
                            }
                        }
                        catch (Exception e)
                        {
                            lastPage = null;
                            TraceLogger.ShowGenericErrorBox("page tagging failed", e);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    TraceLogger.Log(TraceCategory.Warning(), "Background tagging job queue depleted");
                    TraceLogger.Flush();
                }
                catch (OperationCanceledException)
                {
                    TraceLogger.Log(TraceCategory.Warning(), "Background tagging canceled");
                    TraceLogger.Flush();
                }
            }, cancel);
        }

        #region IDisposable

        /// <summary>
        /// Dispose the background tagger.
        /// </summary>
        public void Dispose()
        {
            if (_jobs != null)
            {
                _jobs.CompleteAdding();
                _jobs = null;
                _cancel.Cancel();
            }
        }

        #endregion IDisposable
    }
}