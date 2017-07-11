////////////////////////////////////////////////////////////
// Author: WetHat
// (C) Copyright 2015, 2016 WetHat Lab, all rights reserved
////////////////////////////////////////////////////////////
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
        /// Get the OneNote application object proxy.
        /// </summary>
        public OneNoteProxy OneNoteApp { get; private set; }

        /// <summary>
        /// Initialize this base class
        /// </summary>
        /// <param name="app">OneNote application object</param>
        protected WindowViewModelBase(OneNoteProxy app)
        {
            OneNoteApp = app;
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
        /// Unsubscribe all listeners.
        /// </summary>
        public virtual void Dispose()
        {
            PropertyChanged = null;
        }

        #endregion IDisposable
    }
}