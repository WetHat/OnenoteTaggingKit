using System;
using System.Runtime.InteropServices;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    ///     View Model to support self-closing command windows <see cref="VerboseCommand"/>
    ///     which display a message.
    /// </summary>
    [ComVisible(false)]
    public class VerboseCommandModel : WindowViewModelBase
    {
        /// <summary>
        ///     Get or set the maximum time the dialog is open unless closed manually.
        /// </summary>
        public int DisplayTimeMillies { get; set; } = 3000;

        /// <summary>
        ///     The message displayed while the command executes.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        ///     Initialize a new view model
        /// </summary>
        /// <param name="onenote">OneNote application object.</param>
        public VerboseCommandModel(OneNoteProxy onenote) : base(onenote) {
        }

        /// <summary>
        ///     The command to execute while the message is displayed.
        /// </summary>
        public Action Command { get; set; }
    }
}
