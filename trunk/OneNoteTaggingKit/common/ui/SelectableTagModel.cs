using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// Contract for implementations of view models supporting the <see cref="SelectableTag"/> control.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface should provide property changed events for all getters.
    /// </remarks>
    public interface ISelectableTagModel : INotifyPropertyChanged, ISortableKeyedItem<TagModelKey,string>
    {
        /// <summary>
        /// Set the checked state
        /// </summary>
        bool IsChecked { set; }

        /// <summary>
        /// Get the number of pages having a particular tag 
        /// </summary>
        /// <value>if -1 is returned the page count is not shown in the UI</value>
        int PageCount { get; }

        /// <summary>
        /// Get the visibility of a tag in the UI.
        /// </summary>
        Visibility Visibility { get; }

        /// <summary>
        /// Get the tag name
        /// </summary>
        string TagName { get; }
    }
}
