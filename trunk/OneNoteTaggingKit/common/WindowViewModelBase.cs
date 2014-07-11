using Microsoft.Office.Interop.OneNote;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// Base class for view models supporting the MVVM pattern for top level add-in windows.
    /// </summary>
    public abstract class WindowViewModelBase: DependencyObject, INotifyPropertyChanged, IDisposable
    {
        protected Microsoft.Office.Interop.OneNote.Application OneNoteApp {get; private set;}
        protected XMLSchema OneNotePageSchema {get; private set;}

        internal Microsoft.Office.Interop.OneNote.Window CurrentOneNoteWindow {get; private set;}
        
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

        protected WindowViewModelBase(Microsoft.Office.Interop.OneNote.Application app, XMLSchema schema)
        {
            OneNoteApp = app;
            OneNotePageSchema = schema;
            CurrentOneNoteWindow = app.Windows.CurrentWindow;
        }

        #region INotifyPropertyChanged 
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

        #region IDisposable
        /// <summary>
        /// Unsubsribe all listeners.
        /// </summary>
        public virtual void Dispose()
        {
            PropertyChanged = null;
        }
        #endregion
    }
}
