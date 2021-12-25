using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// Base class for view models which expose observable properties.
    /// </summary>
    /// <remarks>
    ///     To support working with view models in background tasks
    ///     the use of dependency objects is not recommended because of their
    ///     thread affinity. Instead observable properties should raise
    ///     PropertyChanged events. Methods to do that in a type save way are
    ///     provided in this base class.
    /// </remarks>
    [ComVisible(false)]
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        #region INotifyPropertyChanged

        /// <summary>
        /// Event to notify registered handlers about property changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise a change event for
        /// </summary>
        /// <remarks>
        /// This method uses compiler services to get the name of the changed
        /// property implicitely. This method should be called in a property
        /// setter when the change is complete.
        /// </remarks>
        /// <example>
        /// <code>
        /// /// Property Example
        /// int _count;
        /// public int Count {
        ///     get => _count
        ///     set {
        ///         _count = value;
        ///         RaisePropertyChanged();
        ///     }
        /// }
        /// </code>
        /// </example>
        ///
        /// <param name="propertyname">Name of the changed property</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyname = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
        #endregion INotifyPropertyChanged

        #region IDisposable
        /// <summary>
        /// Clear all property handlers for this object.
        /// </summary>
        public virtual void Dispose() {
            PropertyChanged = null;
        }
        #endregion IDisposable
    }
}
