using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using WetHatLab.OneNote.TaggingKit.find;
namespace WetHatLab.OneNote.TaggingKit
{
    /// <summary>
    /// interface for dialogs opened by this addin 
    /// </summary>
    /// <typeparam name="M"></typeparam>
    internal interface IOneNotePageWindow<M>
    {
        /// <summary>
        /// Get or set the view model backing the dialog
        /// </summary>
        M ViewModel { get; set; }
    }
}
