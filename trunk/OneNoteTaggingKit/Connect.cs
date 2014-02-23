using Extensibility;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.OneNote;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using WetHatLab.OneNote.TaggingKit.edit;
using WetHatLab.OneNote.TaggingKit.find;
using WetHatLab.OneNote.TaggingKit.manage;


namespace WetHatLab.OneNote.TaggingKit
{
    /// <summary>
    /// OneNote application connector.
    /// </summary>
    /// <remarks>Manages the addin lifecycle and executes Ribbon bar actions.
    /// <para>This addin implements a simple but flexible tagging system for OneNote pages</para>
    /// </remarks>
    [Guid("C3CE0D94-89A1-4C7E-9633-C496FF3DC4FF"), ProgId("WetHatLab.OneNote.TaggingKitAddin")]
    public class ConnectTaggingKitAddin : IDTExtensibility2, IRibbonExtensibility
    {
        private Microsoft.Office.Interop.OneNote.Application _OneNoteApp;

        private XMLSchema _schema = XMLSchema.xsCurrent;

        private Thread findTagsUI;

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
            if (_OneNoteApp != null)
            {
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
            _OneNoteApp = Application as Microsoft.Office.Interop.OneNote.Application;

            // Upgrade Settings if necessary. On new version the UpdateRequired flag is reset to default (true)
            if (Properties.Settings.Default.UpdateRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpdateRequired = false;
            }

            try
            {
                // determine version of OneNote running
                Process OneNoteProcess = (from p in Process.GetProcesses()
                                          where "ONENOTE".Equals(p.ProcessName,StringComparison.InvariantCultureIgnoreCase)
                                          select p).Single();
                int onVersion = OneNoteProcess.Modules[0].FileVersionInfo.ProductMajorPart;
                switch (onVersion)
                {
                    case 15:
                        _schema = XMLSchema.xs2013;
                        break;
                    case 14:
                        _schema = XMLSchema.xs2010;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.Adding_ErrorBox_ConnectionError, ex), string.Format(Properties.Resources.Addin_ErrorBox_Title, Properties.Resources.TaggingKit_About_Appname));
            }

        }

        /// <summary>
        /// handle disconnection of the OneNote application.
        /// </summary>
        /// <param name="RemoveMode">Enumeration value that informs an add-in why it was unloaded</param>
        /// <param name="custom">An empty array that you can use to pass host-specific data for use after the add-in unloads.</param>
        public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Occurs whenever an add-in loads.
        /// </summary>
        /// <param name="custom">An empty array that you can use to pass host-specific data for use when the add-in loads.</param>
        public void OnStartupComplete(ref Array custom)
        {
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
            try
            {
                Microsoft.Office.Interop.OneNote.Window currentWindow = _OneNoteApp.Windows.CurrentWindow;

                ShowDialog<TagEditor, TagEditorModel>(currentWindow, () => new TagEditorModel(_OneNoteApp, currentWindow.CurrentPageId,_schema));
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.TagEditor_OpenDialogError, ex), Properties.Resources.TagEditor_ErrorBox_Title);
            }
        }

        /// <summary>
        /// Action to open the search tags UI
        /// </summary>
        /// <param name="ribbon">OneNote ribbon bar</param>
        public void findTags(IRibbonControl ribbon)
        {
            if (findTagsUI == null || !findTagsUI.IsAlive)
            {
                Microsoft.Office.Interop.OneNote.Window currentWindow = _OneNoteApp.Windows.CurrentWindow;
                findTagsUI = Show<FindTaggedPages, FindTaggedPagesModel>(currentWindow, () => new FindTaggedPagesModel(_OneNoteApp, currentWindow, _schema));
            }
        }

        /// <summary>
        /// Action to open a tag management dialog.
        /// </summary>
        /// <param name="ribbon"></param>
        public void manageTags(IRibbonControl ribbon)
        {
            Microsoft.Office.Interop.OneNote.Window currentWindow = _OneNoteApp.Windows.CurrentWindow;
            ShowDialog<TagManager, TagManagerModel>(currentWindow, () => new TagManagerModel(_OneNoteApp, _schema));
        }

        /// <summary>
        /// Get mages for ribbon bar buttons
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
                    Properties.Resources.tag_32x32.Save(mem, ImageFormat.Png);
                    break;
            }

            return new COMReadonlyStreamAdapter(mem);
        }

        /// <summary>
        /// Show a WPF dialog.
        /// </summary>
        /// <typeparam name="T">dialog type</typeparam>
        /// <typeparam name="M">view model type</typeparam>
        /// <param name="window">current OneNote windows</param>
        /// <param name="viewModel">view model instance</param>
        /// <returns>dialog result</returns>
        internal static bool? ShowDialog<T, M>(Microsoft.Office.Interop.OneNote.Window window, Func<M> viewModelFactory) where T : System.Windows.Window, IOneNotePageWindow<M>, new()
        {
            bool? retval = null;
            var thread = new Thread(() =>
            {
                System.Windows.Window w = new T();
                w.Closed += (s, e) => w.Dispatcher.InvokeShutdown();
                w.Topmost = true;
                ((IOneNotePageWindow<M>)w).ViewModel = viewModelFactory();
                var helper = new WindowInteropHelper(w);
                helper.Owner = (IntPtr)window.WindowHandle;
                retval = w.ShowDialog();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();

            thread.Join();
            return retval;
        }

        /// <summary>
        /// Show a WPF window.
        /// </summary>
        /// <typeparam name="T">window type</typeparam>
        /// <typeparam name="M">view model type</typeparam>
        /// <param name="window">current OneNote window</param>
        /// <param name="viewModelFactory">factory method to generate the view model in the UI thread of the WPF window</param>
        internal static Thread Show<T, M>(Microsoft.Office.Interop.OneNote.Window window, Func<M> viewModelFactory) where T : System.Windows.Window, IOneNotePageWindow<M>, new()
        {
            var thread = new Thread(() =>
            {
                System.Windows.Window w = new T();
                w.Closed += (s, e) => w.Dispatcher.InvokeShutdown();
                w.Topmost = true;
                ((IOneNotePageWindow<M>)w).ViewModel = viewModelFactory();
                var helper = new WindowInteropHelper(w);
                helper.Owner = (IntPtr)window.WindowHandle;
                w.Show();
                // Turn this thread into an UI thread
                System.Windows.Threading.Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
            return thread;
        }
    }
}
