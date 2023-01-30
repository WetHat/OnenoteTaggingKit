// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WetHatLab.OneNote.TaggingKit.PageBuilder;

namespace WetHatLab.OneNote.TaggingKit.Tagger
{
    /// <summary>
    ///     Background OneNote page tagger.
    /// </summary>
    public sealed class BackgroundTagger : IDisposable
    {
        private OneNoteProxy _onenote;

        private BlockingCollection<TaggingJob> _jobs = new BlockingCollection<TaggingJob>();

        private CancellationTokenSource _cancel;

        /// <summary>
        ///     Get the number of jobs executed by this the tagger.
        /// </summary>
        public uint JobCount { get; private set; }

        /// <summary>
        ///     Get the type of last executed job.
        /// </summary>
        public TagOperation LastJobType { get; private set; } = TagOperation.NOOP;
        /// <summary>
        /// Create a new instance of a background page tagger.
        /// </summary>
        /// <param name="onenote">OneNote application proxy object</param>
        public BackgroundTagger(OneNoteProxy onenote)
        {
            _onenote = onenote;
            _cancel = new CancellationTokenSource();
            JobCount = 0;
        }

        /// <summary>
        /// Schedule a tagging job for background operation.
        /// </summary>
        /// <param name="job">Tagging job descriptor</param>
        public void Add(TaggingJob job)
        {
            TraceLogger.Log(TraceCategory.Info(), "Background job scheduled for execution: {0}", job);
            _jobs.Add(job);
        }

        /// <summary>
        /// Run the background tagger.
        /// </summary>
        /// <returns>Awaitable task object.</returns>
        public Task Run()
        {
            TaskFactory tf = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);
            CancellationToken cancel = _cancel.Token;
            return tf.StartNew(() =>
            {
                TraceLogger.Log(TraceCategory.Info(), "Background tagging service started");
                uint delta = 0;
                try {
                    OneNotePage lastPage = null; // reuse pages among subsequent jobs

                    while (!_jobs.IsCompleted) {
                        DateTime now = DateTime.Now;

                        if (_jobs.Count == 0) {
                            if (delta > 1) {
                                // only report a significant amount of changes
                                AddInDialogManager.ShowNotification(Properties.Resources.TaggingKit_About_Appname,
                                                                    string.Format(Properties.Resources.TaggingKit_Notification, delta));
                            }
                            delta = 0; // we ran dry
                        }
                        TaggingJob j = _jobs.Take();
                        JobCount++;
                        delta++;
                        cancel.ThrowIfCancellationRequested();
                        try {
                            LastJobType = j.OperationType;
                            lastPage = j.Execute(_onenote, lastPage);
                            
                            if (lastPage != null && _jobs.Count == 0) { // no more pending pages - must update the last one and stop carrying forward
                                lastPage.Update(j.OperationType == TagOperation.RESYNC);
                                lastPage = null;
                            }
                        } catch (COMException ce) {
                            switch ((uint)ce.ErrorCode) {
                                case 0x80042010: // concurrent page modification
                                    TraceLogger.Log(TraceCategory.Error(), "Concurrent page modification: {0}\nRescheduling tagging job.", ce.Message);
                                    lastPage = null; //stop recycling this page
                                    _onenote.TaggingService.Add(j);
                                    break;

                                case 0x80042030: // blocked by modal dialog
                                    // let user close the dialog
                                    TraceLogger.ShowGenericErrorBox(Properties.Resources.TaggingKit_Blocked, ce);
                                    lastPage = null; // stop recycling this page
                                    _onenote.TaggingService.Add(j);
                                    break;
                                default:
                                    TraceLogger.ShowGenericErrorBox("Page tagging failed", ce);
                                    break;
                            }
                        } catch (Exception e) {
                            lastPage = null;
                            TraceLogger.ShowGenericErrorBox("Page tagging failed", e);
                        }
                        TraceLogger.Flush();
                    }
                } catch (InvalidOperationException) {
                    TraceLogger.Log(TraceCategory.Warning(), "Background tagging job queue depleted");
                }
                catch (OperationCanceledException) {
                    TraceLogger.Log(TraceCategory.Warning(), "Background tagging canceled");
                }
                TraceLogger.Flush();
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