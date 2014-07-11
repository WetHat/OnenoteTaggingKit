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
        /// /remarks>
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
