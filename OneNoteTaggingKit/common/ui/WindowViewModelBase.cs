////////////////////////////////////////////////////////////
// Author: WetHat
// (C) Copyright 2015, 2016 WetHat Lab, all rights reserved
////////////////////////////////////////////////////////////
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// Base class for view models supporting the MVVM pattern for top level add-in windows.
    /// </summary>
    [ComVisible(false)]
    public abstract class WindowViewModelBase : DependencyObject, IDisposable
    {
        /// <summary>
        /// Get the OneNote application object proxy.
        /// </summary>
        public OneNoteProxy OneNoteApp { get; private set; }

        /// <summary>
        /// Event raised when any dependency property changed.
        /// </summary>
        public event EventHandler<DependencyPropertyChangedEventArgs> DependencyPropertyChanged;

        /// <summary>
        /// Initialize this base class
        /// </summary>
        /// <param name="app">OneNote application object</param>
        protected WindowViewModelBase(OneNoteProxy app)
        {
            OneNoteApp = app;
        }

        /// <summary>
        /// Forward dependency property changes to registered listeners.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);
            DependencyPropertyChanged?.Invoke(this, e);
        }

        #region IDisposable
        /// <summary>
        /// Unsubscribe all listeners.
        /// </summary>
        public virtual void Dispose() {
            DependencyPropertyChanged = null;
        }

        #endregion IDisposable
    }
}