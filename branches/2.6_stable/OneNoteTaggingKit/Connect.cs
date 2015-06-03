using Extensibility;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.edit;
using WetHatLab.OneNote.TaggingKit.find;
using WetHatLab.OneNote.TaggingKit.manage;
using WetHatLab.OneNote.TaggingKit.nexus;


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
            lock(_SingletonWindows)
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
                        helper.Owner = (IntPtr)viewmodel.CurrentOneNoteWindow.WindowHandle;
                        w.Show();
                        _SingletonWindows.Add(typeof(W),w);
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
        /// Show a WPF dialog.
        /// </summary>
        /// <typeparam name="T">dialog type</typeparam>
        /// <typeparam name="M">view model type</typeparam>
        /// <param name="window">current OneNote windows</param>
        /// <param name="viewModelFactory">factory method to create a view model</param>
        /// <returns>dialog result</returns>
        public bool? ShowDialog<T, M>(Microsoft.Office.Interop.OneNote.Window window, Func<M> viewModelFactory)
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
                    helper.Owner = (IntPtr)viewmodel.CurrentOneNoteWindow.WindowHandle;
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
                        w.Dispatcher.Invoke(() => {
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
    /// <summary>
    /// OneNote application connector.
    /// </summary>
    /// <remarks>Manages the add-in life-cycle and executes Ribbon bar actions.
    /// <para>This add-in implements a simple but flexible tagging system for OneNote pages</para>
    /// </remarks>
    [Guid("C3CE0D94-89A1-4C7E-9633-C496FF3DC4FF"), ProgId("WetHatLab.OneNote.TaggingKitAddin")]
    public class ConnectTaggingKitAddin : IDTExtensibility2, IRibbonExtensibility
    {

        private Microsoft.Office.Interop.OneNote.Application _OneNoteApp;

        private XMLSchema _schema = XMLSchema.xsCurrent;
        private bool _schemaChecked = false;
        private bool _forceExit = false;

        private AddInDialogManager _dialogmanager = null;
        
        /// <summary>
        /// Create a new instance of the OneNote connector object.
        /// </summary>
        public ConnectTaggingKitAddin()
        {
            TraceLogger.Register();
        }

        /// <summary>
        /// Get the highest version of the schema supported by OneNote.
        /// </summary>
        private XMLSchema CurrentSchema
        {
            get
            {
                if (_schemaChecked)
                {
                    return _schema;
                }

                string currentSectionID = _OneNoteApp.Windows.CurrentWindow.CurrentSectionId;
                string outXml;

                Exception schemaException = null;

                // determine schema version we can use
                foreach (var schema in new XMLSchema[] {XMLSchema.xs2013, XMLSchema.xs2010 })
                {
                    try
                    {
                        _OneNoteApp.GetHierarchy(currentSectionID, HierarchyScope.hsSelf, out outXml, schema);
                        // we can use this schema
                        _schema = schema;
                        schemaException = null;
                        _schemaChecked = true;
                        TraceLogger.Log(TraceCategory.Info(), "OneNote schema Version: {0}", _schema);
                        break;
                    }
                    catch (Exception xe)
                    {
                        _forceExit = true;
                        schemaException = xe;
                        TraceLogger.Log(TraceCategory.Info(), "Test of OneNote Schema Version: {0} failed with {1}", schema, xe);
                    }
                }

                if (!_schemaChecked)
                {
                    TraceLogger.Log(TraceCategory.Error(), "Unable to determine OneNote version!");
                    TraceLogger.ShowGenericErrorBox(Properties.Resources.TaggingKit_Error_VersionLookup, schemaException);
                }
                Trace.Flush();
                return _schema;
            }
        }

        #region IDTExtensibility2
        /// <summary>
        /// Occurs whenever an add-in is loaded or unloaded.
        /// </summary>
        /// <param name="custom">An empty array that you can use to pass host-specific data for use in the add-in.</param>
        public void OnAddInsUpdate(ref Array custom)
        {

        }
        /// <summary>
        /// Occurs whenever OneNote shuts down while an add-in is running.
        /// </summary>
        /// <param name="custom">An empty array that you can use to pass host-specific data for use in the add-in.</param>
        public void OnBeginShutdown(ref Array custom)
        {
            TraceLogger.Log(TraceCategory.Info(), "Beginning Add-In shutdown; Arguments '{0}'",custom);
            if (_OneNoteApp != null)
            {
                if (_dialogmanager != null)
                {
                    _dialogmanager.Dispose();
                    _dialogmanager = null;
                }
                _OneNoteApp = null;
            }
        }

        /// <summary>
        /// Handle the connection to the OneNote application. 
        /// </summary>
        /// <param name="Application">The instance of OneNote which added the addin</param>
        /// <param name="ConnectMode">Enumeration value that indicates the way the add-in was loaded.</param>
        /// <param name="AddInInst">Reference to the add-in's own instance</param>
        /// <param name="custom">An empty array that you can use to pass host-specific data for use in the add-in</param>
        public void OnConnection(object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
        {
            TraceLogger.Log(TraceCategory.Info(), "Connection mode '{0}'", ConnectMode);
            _OneNoteApp = Application as Microsoft.Office.Interop.OneNote.Application;
            
            // Upgrade Settings if necessary. On new version the UpdateRequired flag is reset to default (true)
            if (Properties.Settings.Default.UpdateRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateRequired = false;
            }

            _dialogmanager = new AddInDialogManager();
        }

        /// <summary>
        /// handle disconnection of the OneNote application.
        /// </summary>
        /// <param name="RemoveMode">Enumeration value that informs an add-in why it was unloaded</param>
        /// <param name="custom">An empty array that you can use to pass host-specific data for use after the add-in unloads.</param>
        public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
        {
            TraceLogger.Log(TraceCategory.Info(), "Disconnecting; mode='{0}'; Arguments: '{1}'",RemoveMode,custom);
            if (_dialogmanager != null)
            {
                _dialogmanager.Dispose();
                _dialogmanager = null;
            }
            Trace.Flush();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (_forceExit 
                && ( RemoveMode == ext_DisconnectMode.ext_dm_HostShutdown
                  || RemoveMode == ext_DisconnectMode.ext_dm_UserClosed))
            {
                // a dirty hack to make sure the ddlhost shuts down after an exception occurred.
                // This is necessary to allow the add-in to be loaded successfully next time
                // OneNote starts (a zombie dllhost would prevent that)
                TraceLogger.Log(TraceCategory.Info(), "Forcing COM Surrogate shutdown");
                Trace.Flush();
                Environment.Exit(0);
            }
            
        }

        /// <summary>
        /// Occurs whenever an add-in loads.
        /// </summary>
        /// <param name="custom">An empty array that you can use to pass host-specific data for use when the add-in loads.</param>
        public void OnStartupComplete(ref Array custom)
        {
            TraceLogger.Log(TraceCategory.Info(), "Startup Arguments '{0}'", custom);
        }
        #endregion IDTExtensibility2

        #region IRibbonExtensibility
        /// <summary>
        /// Get the ribbon definition of this addin
        /// </summary>
        /// <param name="RibbonID">identifier of the ribbon</param>
        /// <returns>ribbon definition XML as string</returns>
        public string GetCustomUI(string RibbonID)
        {
            TraceLogger.Log(TraceCategory.Info(), "UI configuration requested: {0}", RibbonID);
            return Properties.Resources.ribbon;
        }
        #endregion IRibbonExtensibility

        /// <summary>
        /// Action to open a tag editor dialog.
        /// </summary>
        /// <remarks>Opens the page tag editor</remarks>
        /// <param name="ribbon">OneNote ribbon bar</param>
        public void editTags(IRibbonControl ribbon)
        {
            TraceLogger.Log(TraceCategory.Info(), "Show tag editor");
            XMLSchema s = CurrentSchema;
            _dialogmanager.Show<TagEditor, TagEditorModel>(() => new TagEditorModel(_OneNoteApp, s));
        }

        /// <summary>
        /// Action to open the search tags UI
        /// </summary>
        /// <param name="ribbon">OneNote ribbon bar</param>
        public void findTags(IRibbonControl ribbon)
        {
            TraceLogger.Log(TraceCategory.Info(), "Show tag finder");
            XMLSchema s = CurrentSchema;
            _dialogmanager.Show<FindTaggedPages, FindTaggedPagesModel>(() => new FindTaggedPagesModel(_OneNoteApp, s));
        }

        /// <summary>
        /// Action to open the "Related Pages" UI
        /// </summary>
        /// <param name="ribbon">OneNote ribbon bar</param>
        public void relatedPages(IRibbonControl ribbon)
        {
            TraceLogger.Log(TraceCategory.Info(), "Show related pages tracer");
            XMLSchema s = CurrentSchema;
            _dialogmanager.Show<RelatedPages, RelatedPagesModel>(() => new RelatedPagesModel(_OneNoteApp, s));
        }

        /// <summary>
        /// Action to open a tag management dialog.
        /// </summary>
        /// <param name="ribbon"></param>
        public void manageTags(IRibbonControl ribbon)
        {
            TraceLogger.Log(TraceCategory.Info(), "Show settings editor");
            XMLSchema s = CurrentSchema;
            _dialogmanager.ShowDialog<TagManager, TagManagerModel>(_OneNoteApp.Windows.CurrentWindow, () => new TagManagerModel(_OneNoteApp, s));
        }

        /// <summary>
        /// Get images for ribbon bar buttons
        /// </summary>
        /// <param name="imageName">name of image to get</param>
        /// <returns>image stream</returns>
        public IStream GetImage(string imageName)
        {
            MemoryStream mem = new MemoryStream();
            
            switch (imageName)
            {
                case "pageTags.png":
                    Properties.Resources.tag_32x32.Save(mem, ImageFormat.Png);
                    break;
                case "managePageTags.png":
                    Properties.Resources.settings_32x32.Save(mem, ImageFormat.Png);
                    break;
                 case "findPageTags.png":
                    Properties.Resources.tagSearch_32x32.Save(mem, ImageFormat.Png);
                    break;
                default:
                    TraceLogger.Log(TraceCategory.Warning(), "Unknown image requested: {0}", imageName);
                    Properties.Resources.tag_32x32.Save(mem, ImageFormat.Png);
                    break;
            }

            return new COMReadonlyStreamAdapter(mem);
        }
    }
}
