////////////////////////////////////////////////////////////
// Author: WetHat
// (C) Copyright 2015, 2016 WetHat Lab, all rights reserved
////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit
{
    /// <summary>
    /// Manage the singleton dialog windows of the add-in
    /// </summary>
    /// <remarks>This class is thread safe</remarks>
    internal class AddInDialogManager : IDisposable
    {
        private Dictionary<Type, System.Windows.Window> _SingletonWindows = new Dictionary<Type, System.Windows.Window>();
        private bool _disposed = false;

        private void UnregisterWindow(Type windowType)
        {
            lock (_SingletonWindows)
            {
                _SingletonWindows.Remove(windowType);
            }
        }

        /// <summary>
        /// Show a WPF window.
        /// </summary>
        /// <typeparam name="W">window type</typeparam>
        /// <typeparam name="M">view model type</typeparam>
        /// <param name="viewModelFactory">factory method to generate the view model in the UI thread of the WPF window</param>
        public void Show<W, M>(Func<M> viewModelFactory)
            where W : System.Windows.Window, IOneNotePageWindow<M>, new()
            where M : WindowViewModelBase
        {
            var thread = new Thread(() =>
            {
                try
                {
                    lock (_SingletonWindows)
                    {
                        System.Windows.Window w;
                        if (_SingletonWindows.TryGetValue(typeof(W), out w))
                        {
                            w.Dispatcher.Invoke(() => w.WindowState = WindowState.Normal);
                            return;
                        }
                        w = new W();
                        w.Closing += (o, e) => UnregisterWindow(typeof(W));
                        w.Closed += (s, e) => w.Dispatcher.InvokeShutdown();
                        w.Topmost = true;
                        M viewmodel = viewModelFactory();
                        ((IOneNotePageWindow<M>)w).ViewModel = viewmodel;
                        var helper = new WindowInteropHelper(w);
                        helper.Owner = (IntPtr)viewmodel.OneNoteApp.CurrentWindow.WindowHandle;
                        w.Show();
                        _SingletonWindows.Add(typeof(W), w);
                    }
                    // Turn this thread into an UI thread
                    System.Windows.Threading.Dispatcher.Run();
                }
                catch (ThreadAbortException ta)
                {
                    TraceLogger.Log(TraceCategory.Warning(), "Window Thread aborted: {0}", ta);
                }
                catch (Exception ex)
                {
                    TraceLogger.Log(TraceCategory.Error(), "Exception while creating dialog: {0}", ex);
                    TraceLogger.ShowGenericErrorBox(Properties.Resources.TagEditor_Error_WindowCreation, ex);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// Show a singleton WPF dialog.
        /// </summary>
        /// <typeparam name="T">dialog type</typeparam>
        /// <typeparam name="M">view model type</typeparam>
        /// <param name="viewModelFactory">factory lambda function to create a view model</param>
        /// <returns>dialog result</returns>
        public bool? ShowDialog<T, M>(Func<M> viewModelFactory)
            where T : System.Windows.Window, IOneNotePageWindow<M>, new()
            where M : WindowViewModelBase
        {
            bool? retval = null;
            var thread = new Thread(() =>
            {
                try
                {
                    System.Windows.Window w = new T();
                    w.Closed += (s, e) => w.Dispatcher.InvokeShutdown();
                    w.Topmost = true;
                    M viewmodel = viewModelFactory();
                    ((IOneNotePageWindow<M>)w).ViewModel = viewmodel;
                    var helper = new WindowInteropHelper(w);
                    helper.Owner = (IntPtr)viewmodel.OneNoteApp.CurrentWindow.WindowHandle;
                    retval = w.ShowDialog();
                    Trace.Flush();
                }
                catch (Exception ex)
                {
                    TraceLogger.Log(TraceCategory.Error(), "Exception while creating dialog: {0}", ex);
                    TraceLogger.ShowGenericErrorBox(Properties.Resources.TagEditor_Error_WindowCreation, ex);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            thread.Join();
            return retval;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                // We do not lock the window collection because closing a window
                // also attempts to lock that collection.
                foreach (System.Windows.Window w in _SingletonWindows.Values.ToArray())
                {
                    try
                    {
                        w.Dispatcher.Invoke(() =>
                        {
                            TraceLogger.Log(TraceCategory.Info(), "Closing Window: {0}", w.Title);
                            w.Close();
                        });
                    }
                    catch (Exception e)
                    {
                        TraceLogger.Log(TraceCategory.Error(), "Closing window failed: {0}", e);
                    }
                }
            }
        }
    }
}