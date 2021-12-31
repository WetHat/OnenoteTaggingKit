using System.Windows;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// Delegate for tag selection events
    /// </summary>
    /// <param name="sender">object emitting the event</param>
    /// <param name="e">     event details</param>
    public delegate void TagSelectedEventHandler(object sender, TagSelectedEventArgs e);

    /// <summary>
    /// Event details for tag selection events.
    /// </summary>
    public class TagSelectedEventArgs: RoutedEventArgs
    {
        /// <summary>
        /// Get the selection status.
        /// </summary>
        public bool IsSelected { get; }

        /// <summary>
        /// Create a new instance of event details for tag selection events.
        /// </summary>
        /// <param name="routedEvent">The event that was fired</param>
        /// <param name="source">The object which raised the event</param>
        /// <param name="is_selected">Flag to indicate the selection status.</param>
        public TagSelectedEventArgs(RoutedEvent routedEvent,
                                    object source, bool is_selected)
            : base(routedEvent, source) {
            IsSelected = is_selected;
        }
    }
}
