using Microsoft.Office.Interop.OneNote;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// Base class for view models supporting the MVVM pattern for top level add-in windows.
    /// </summary>
    [ComVisible(false)]
    public abstract class WindowViewModelBase : DependencyObject, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Get the OneNote application object
        /// </summary>
        protected Microsoft.Office.Interop.OneNote.Application OneNoteApp { get; private set; }

        /// <summary>
        /// Get the OneNote current window object
        /// </summary>
        internal Microsoft.Office.Interop.OneNote.Window CurrentOneNoteWindow { get; private set; }

        /// <summary>
        /// Get the highest version of the schema supported by OneNote.
        /// </summary>
        internal XMLSchema CurrentSchema
        {
            get
            {
                string outXml;

                // determine schema version we can use
                foreach (var schema in new XMLSchema[] { XMLSchema.xs2013, XMLSchema.xs2010 })
                {
                    try
                    {
                        OneNoteApp.GetHierarchy(CurrentNotebookID, HierarchyScope.hsSelf, out outXml, schema);
                        // we can use this schema
                        TraceLogger.Log(TraceCategory.Info(), "OneNote schema Version: {0}", schema);
                        return schema;
                    }
                    catch (Exception xe)
                    {
                        TraceLogger.Log(TraceCategory.Info(), "Test of OneNote Schema Version: {0} failed with {1}", schema, xe);
                        TraceLogger.Flush();
                    }
                }
                return XMLSchema.xs2010;
            }
        }

        /// <summary>
        /// Get the id of the current OneNote page
        /// </summary>
        internal string CurrentPageID
        {
            get { return CurrentOneNoteWindow.CurrentPageId; }
        }

        internal string CurrentSectionID
        {
            get { return CurrentOneNoteWindow.CurrentSectionId; }
        }

        internal string CurrentSectionGroupID
        {
            get { return CurrentOneNoteWindow.CurrentSectionGroupId; }
        }

        internal string CurrentNotebookID
        {
            get { return CurrentOneNoteWindow.CurrentNotebookId; }
        }

        /// <summary>
        /// Initialize this base class
        /// </summary>
        /// <param name="app">OneNote application object</param>
        /// <param name="schema">OneNote schema to use</param>
        protected WindowViewModelBase(Microsoft.Office.Interop.OneNote.Application app)
        {
            OneNoteApp = app;
            CurrentOneNoteWindow = app.Windows.CurrentWindow;
        }

        #region INotifyPropertyChanged

        /// <summary>
        /// Event to notify registered handlers about property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged

        /// <summary>
        /// Notify listeners subscribed to the <see cref="E:WetHatLab.OneNote.TaggingKit.common.PropertyChanged"/> about changes to model properties.
        /// </summary>
        /// <remarks>typically this method is used together with a number of static PropertyChangedEventArgs members.
        /// <example>
        /// <code>
        ///   static readonly PropertyChangedEventArgs PAGE_TITLE = new PropertyChangedEventArgs("PageTitle");
        ///   ....
        ///   fireNotifyPropertyChanged(PAGE_TITLE);
        /// </code>
        /// </example>
        /// </remarks>
        /// <param name="propArgs">event and property details</param>
        protected void fireNotifyPropertyChanged(PropertyChangedEventArgs propArgs)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, propArgs);
            }
        }

        /// <summary>
        /// Notify listeners subscribed to the <see cref="E:WetHatLab.OneNote.TaggingKit.common.PropertyChanged"/> about changes to model properties.
        /// </summary>
        /// <remarks>Notification is performed in a given thread context</remarks>
        /// <param name="dispatcher">thread context to use</param>
        /// <param name="propArgs">event and property details</param>
        protected void fireNotifyPropertyChanged(Dispatcher dispatcher, PropertyChangedEventArgs propArgs)
        {
            dispatcher.Invoke(() => fireNotifyPropertyChanged(propArgs));
        }

        #region IDisposable

        /// <summary>
        /// Unsubsribe all listeners.
        /// </summary>
        public virtual void Dispose()
        {
            PropertyChanged = null;
        }

        #endregion IDisposable
    }
}