using WetHatLab.OneNote.TaggingKit.common;
namespace WetHatLab.OneNote.TaggingKit
{
    /// <summary>
    /// interface for dialogs opened by this addin 
    /// </summary>
    /// <typeparam name="M"></typeparam>
    internal interface IOneNotePageWindow<M> where M: WindowViewModelBase
    {
        /// <summary>
        /// Get or set the view model backing the dialog
        /// </summary>
        M ViewModel { get; set; }
    }
}
