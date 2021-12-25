using System.Windows;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.common
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
        /// Get the data context for the selected tag.
        /// </summary>
        public SelectableTagModel SelectedTag { get; }

        /// <summary>
        /// Create a new instance of event details for tag selection events.
        /// </summary>
        /// <param name="routedEvent">The event that was fired</param>
        /// <param name="source">The oject which raised the event</param>
        /// <param name="tag">the tag's data context.</param>
        public TagSelectedEventArgs(RoutedEvent routedEvent,
                                    object source, SelectableTagModel tag)
            : base(routedEvent, source) {
            SelectedTag = tag;
        }
    }
}
