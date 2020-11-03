// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
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
                DateTime lastnotification = DateTime.MinValue;
                int jobcount = 0;
                int lastreportedCount = 0;
                try {
                    OneNotePageProxy lastPage = null; // reuse pages among subsequent jobs

                    while (!_jobs.IsCompleted) {
                        DateTime now = DateTime.Now;
                        int delta = jobcount - lastreportedCount;

                        if (_jobs.Count == 0
                            && delta > 10
                            && (now - lastnotification).TotalMinutes > 2) { // do not spam notifications
                            AddInDialogManager.ShowNotification(Properties.Resources.TaggingKit_About_Appname,
                                                                string.Format(Properties.Resources.TaggingKit_Notification, delta));
                            lastnotification = now;
                            lastreportedCount = jobcount;
                        }
                        TaggingJob j = _jobs.Take();
                        jobcount++;
                        cancel.ThrowIfCancellationRequested();
                        try {
                            lastPage = j.Execute(_onenote, lastPage);
                            if (lastPage != null && _jobs.Count == 0) { // no more pending pages - must update the last one and stop carrying forward
                                lastPage.Update();
                                lastPage = null;
                            }
                        } catch (COMException ce) {
                            switch ((uint)ce.ErrorCode) {
                                case 0x80042010: // concurrent page modification
                                    TraceLogger.Log(TraceCategory.Error(), "The last modified date does not match. Concurrent page modification: {0}\n Rescheduling tagging job.", ce.Message);
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
                    }
                } catch (InvalidOperationException) {
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