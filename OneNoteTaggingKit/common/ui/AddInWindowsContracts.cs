////////////////////////////////////////////////////////////
// Author: WetHat
// (C) Copyright 2015, 2016 WetHat Lab, all rights reserved
////////////////////////////////////////////////////////////

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// interface for dialogs opened by this add-in
    /// </summary>
    /// <typeparam name="M">type of the view model backing the window</typeparam>
    internal interface IOneNotePageWindow<M> where M : WindowViewModelBase
    {
        /// <summary>
        /// Get or set the view model backing the dialog
        /// </summary>
        M ViewModel { get; set; }
    }
}