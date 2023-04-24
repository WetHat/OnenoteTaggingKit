// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using Extensibility;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.OneNote;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;
using WetHatLab.OneNote.TaggingKit.edit;
using WetHatLab.OneNote.TaggingKit.find;
using WetHatLab.OneNote.TaggingKit.manage;
using WetHatLab.OneNote.TaggingKit.Tagger;

namespace WetHatLab.OneNote.TaggingKit
{
    /// <summary>
    /// OneNote application connector.
    /// </summary>
    /// <remarks>
    /// Manages the add-in life-cycle and executes Ribbon bar actions.
    /// <para>
    /// This add-in implements a simple but flexible tagging system for OneNote pages
    /// </para>
    /// </remarks>
    [Guid("C3CE0D94-89A1-4C7E-9633-C496FF3DC4FF"), ProgId("WetHatLab.OneNote.TaggingKitAddin")]
    public class ConnectTaggingKitAddin : IDTExtensibility2, IRibbonExtensibility
    {
        private OneNoteProxy _onProxy;

        private AddInDialogManager _dialogmanager = null;

        /// <summary>
        /// Create a new instance of the OneNote connector object.
        /// </summary>
        public ConnectTaggingKitAddin() {
            TraceLogger.Register();
            try {
                // Upgrade Settings if necessary. On new version the UpdateRequired flag is
                // reset to default (true)
                if (Properties.Settings.Default.UpdateRequired) {
                    Properties.Settings.Default.Upgrade();
                    Properties.Settings.Default.UpdateRequired = false;

                    try {
                        string knownTags = Properties.Settings.Default.GetPreviousVersion("KnownTags") as string;
                        if (knownTags != null) {
                            var tags = new PageTagSet(knownTags, TagFormat.AsEntered);
                            Properties.Settings.Default.KnownTagsCollection.AddRange((from t in tags select t.ToString()).ToArray());
                        }
                    }
                    catch (SettingsPropertyNotFoundException) {
                        // no tags known so far
                        TraceLogger.Log(TraceCategory.Info(), "No previously defined tags found! Starting with empty tag suggestion list");
                    } 
                    
                }
                switch (Properties.Settings.Default.DisplayLanguage) {
                    case 1:
                        TraceLogger.Log(TraceCategory.Info(), "Setting Display Language to English");
                        Properties.Resources.Culture = CultureInfo.GetCultureInfo("en"); // Englisch
                        break;
                    case 2:
                        TraceLogger.Log(TraceCategory.Info(), "Setting Display Language to English");
                        Properties.Resources.Culture = CultureInfo.GetCultureInfo("zh"); // Chinese
                        break;
                        default:
                        TraceLogger.Log(TraceCategory.Info(), "Setting Display Language to System");
                        break;
                }
            } catch (Exception ex) {
                TraceLogger.ShowGenericErrorBox("Tagging Kit Initialization failed!", ex);
            }
        }

        #region IDTExtensibility2

        /// <summary>
        /// Occurs whenever an add-in is loaded or unloaded.
        /// </summary>
        /// <param name="custom">
        /// An empty array that you can use to pass host-specific data for use in the add-in.
        /// </param>
        public void OnAddInsUpdate(ref Array custom) {
            TraceLogger.Log(TraceCategory.Info(), "{0} update initiated; Arguments '{1}'", Properties.Resources.TaggingKit_About_Appname, custom);
        }

        /// <summary>
        /// Occurs whenever OneNote shuts down while an add-in is running.
        /// </summary>
        /// <param name="custom">
        /// An empty array that you can use to pass host-specific data for use in the add-in.
        /// </param>
        public void OnBeginShutdown(ref Array custom) {
            TraceLogger.Log(TraceCategory.Info(), "Beginning {0} shutdown; Arguments '{1}'", Properties.Resources.TaggingKit_About_Appname, custom);
            Properties.Settings.Default.Save();
            if (_dialogmanager != null) {
                _dialogmanager.Dispose();
                _dialogmanager = null;
            }

            if (_onProxy != null) {
                _onProxy.Dispose();
                _onProxy = null;
            }
        }

        /// <summary>
        /// Handle the connection to the OneNote application.
        /// </summary>
        /// <param name="app">        The instance of OneNote which added the add-in</param>
        /// <param name="ConnectMode">
        /// Enumeration value that indicates the way the add-in was loaded.
        /// </param>
        /// <param name="AddInInst">  Reference to the add-in's own instance</param>
        /// <param name="custom">
        /// An empty array that you can use to pass host-specific data for use in the add-in
        /// </param>
        public void OnConnection(object app, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom) {
            try {
                TraceLogger.Log(TraceCategory.Info(), "Connection mode '{0}'", ConnectMode);

                _onProxy = new OneNoteProxy(app as Microsoft.Office.Interop.OneNote.Application);
                _dialogmanager = new AddInDialogManager();
                TraceLogger.Flush();
            } catch (Exception ex) {
                TraceLogger.Log(TraceCategory.Error(), "Connecting {0} failed: {1}", Properties.Resources.TaggingKit_About_Appname, ex);
                TraceLogger.Flush();
                throw;
            }
        }

        /// <summary>
        /// handle disconnection of the OneNote application.
        /// </summary>
        /// <param name="RemoveMode">
        /// Enumeration value that informs an add-in why it was unloaded
        /// </param>
        /// <param name="custom">
        /// An empty array that you can use to pass host-specific data for use after the
        /// add-in unloads.
        /// </param>
        public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom) {
            TraceLogger.Log(TraceCategory.Info(), "Disconnecting; mode='{0}'; Arguments: '{1}'", RemoveMode, custom);
            if (_dialogmanager != null) {
                _dialogmanager.Dispose();
                _dialogmanager = null;
            }
            Trace.Flush();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (RemoveMode == ext_DisconnectMode.ext_dm_HostShutdown
                 || RemoveMode == ext_DisconnectMode.ext_dm_UserClosed) {
                // a dirty hack to make sure the ddlhost shuts down after an exception
                // occurred. This is necessary to allow the add-in to be loaded
                // successfully next time OneNote starts (a zombie dllhost would prevent that)
                TraceLogger.Log(TraceCategory.Info(), "Forcing COM Surrogate shutdown");
                Trace.Flush();
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// Occurs whenever an add-in loads.
        /// </summary>
        /// <param name="custom">
        /// An empty array that you can use to pass host-specific data for use when the
        /// add-in loads.
        /// </param>
        public void OnStartupComplete(ref Array custom) {
            TraceLogger.Log(TraceCategory.Info(), "Startup Arguments '{0}'", custom);
            try {
                XMLSchema s = _onProxy.OneNoteSchema; // cache the schema
            } catch (Exception ex) {
                TraceLogger.Log(TraceCategory.Error(), "{0} initialization failed: {1}", Properties.Resources.TaggingKit_About_Appname, ex);
                TraceLogger.Flush();
                throw;
            }
            TraceLogger.Log(TraceCategory.Info(), "{0} initialization complete!", Properties.Resources.TaggingKit_About_Appname);
            TraceLogger.Flush();
        }

        #endregion IDTExtensibility2

        #region IRibbonExtensibility

        /// <summary>
        /// Get the ribbon definition of this add-in
        /// </summary>
        /// <param name="RibbonID">identifier of the ribbon</param>
        /// <returns>ribbon definition XML as string</returns>
        public string GetCustomUI(string RibbonID) {
            TraceLogger.Log(TraceCategory.Info(), "UI configuration requested: {0}", RibbonID);
            return Properties.Resources.ribbon;
        }

        #endregion IRibbonExtensibility

        /// <summary>
        /// Action to open a tag editor dialog.
        /// </summary>
        /// <remarks>Opens the page tag editor</remarks>
        /// <param name="ribbon">OneNote ribbon bar</param>
        public void editTags(IRibbonControl ribbon) {
            TraceLogger.Log(TraceCategory.Info(), "Show tag editor");
            _dialogmanager.Show<TagEditor, TagEditorModel>(() => new TagEditorModel(_onProxy));
        }

        /// <summary>
        /// Action to open the search tags UI
        /// </summary>
        /// <param name="ribbon">OneNote ribbon bar</param>
        public void findTags(IRibbonControl ribbon) {
            TraceLogger.Log(TraceCategory.Info(), "Show tag finder");
            _dialogmanager.Show<FindTaggedPages, FindTaggedPagesModel>(() => new FindTaggedPagesModel(_onProxy));
        }

        /// <summary>
        /// Action to open a tag management dialog.
        /// </summary>
        /// <param name="ribbon"></param>
        public void manageTags(IRibbonControl ribbon) {
            TraceLogger.Log(TraceCategory.Info(), "Show settings editor");
            AddInDialogManager.ShowDialog<TagManager, TagManagerModel>(() => new TagManagerModel(_onProxy));
        }
        /// <summary>
        ///     Action to refresh the current page.
        /// </summary>
        /// <param name="ribbon">
        ///     The ribbon control which called this command.
        /// </param>
        public void updatePage(IRibbonControl ribbon) {
            TraceLogger.Log(TraceCategory.Info(), "Refresh current page");

            if (_onProxy != null) {
                var currentPage = _onProxy.CurrentPageID;
                if (!string.IsNullOrEmpty(currentPage)) {
                    int jobs = _onProxy.TaggingService.PendingJobCount;
                    _dialogmanager.Show<VerboseCommand, VerboseCommandModel>(() => new VerboseCommandModel(_onProxy) {
                        Message = jobs > 0 
                                  ? string.Format(Properties.Resources.Message_UpdatePage_Scheduled, jobs)
                                  : Properties.Resources.Message_UpdatePage_Immediate,
                        DisplayTimeMillies = 10000,
                        Command = new Action(() => _onProxy.TaggingService.Add(new TaggingJob(currentPage,
                                                                                              null,
                                                                                              TagOperation.RESYNC)))
                    });
                }
            }
        }
        /// <summary>
        /// Get images for ribbon bar buttons
        /// </summary>
        /// <param name="imageName">name of image to get</param>
        /// <returns>image stream</returns>
        public IStream GetImage(string imageName) {
            MemoryStream mem = new MemoryStream();

            switch (imageName) {
                case "pageTags.png":
                    Properties.Resources.tag_32x32.Save(mem, ImageFormat.Png);
                    break;
                case "refreshPage.png":
                    Properties.Resources.refreshPage_32x32.Save(mem, ImageFormat.Png);
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