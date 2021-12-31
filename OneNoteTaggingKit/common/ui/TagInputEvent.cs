// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// Handler for the <see cref="E:TagInputBox.TagInput"/>
    /// or <see cref="E:HitHighlightedTagButton.TagInput"/>  event.
    /// </summary>
    /// <param name="sender">object emitting the event</param>
    /// <param name="e">     event details</param>
    public delegate void TagInputEventHandler(object sender, TagInputEventArgs e);

    /// <summary>
    /// Event details for a `<see cref="E:TagInputBox.TagInput"/>
    /// or <see cref="E:HitHighlightedTagButton.TagInput"/> /> event.
    /// </summary>
    [ComVisible(false)]
    public class TagInputEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Enumeration of actions to take.
        /// </summary>
        public enum TaggingAction
        {
            /// <summary>
            /// No page tagging action requested. Just process tags.
            /// </summary>
            /// <remarks>
            /// Equivalent to clicking on suggested tags or typing tags.
            /// </remarks>
            None,

            /// <summary>
            /// Add tags to page(s) action requested.
            /// </summary>
            /// <remarks>
            /// Equivalent to pressing the 'Add Tags' button.
            /// </remarks>
            Add,

            /// <summary>
            /// Set tags on page(s) action requested.
            /// </summary>
            /// <remarks>
            /// Equivalent to pressing the 'Set Tags' button.
            /// </remarks>
            Set,

            /// <summary>
            /// Remove tags from page(s) action requested.
            /// </summary>
            Remove,

            /// <summary>
            /// Clear all selected tags in the dialog.
            /// </summary>
            /// <remarks>
            /// Equivalent to pressing the 'Remove Tags' button.
            /// </remarks>
            Clear
        }

        /// <summary>
        /// Get or set a flag to determine if tag input is complete.
        /// </summary>
        /// <value>true if tag input is complete; false if tag input is still in progress</value>
        public bool TagInputComplete { get; set; }


        /// <summary>
        /// Get the requested tagging action.
        /// </summary>
        public TaggingAction Action { get; set; }

        /// <summary>
        /// Get the tags.
        /// </summary>
        public IEnumerable<string> Tags { get; private set; }
        /// <summary>
        /// Create a new instance of the event metadata object.
        /// </summary>
        /// <param name="routedEvent"> Routed event which fired</param>
        /// <param name="source">      Control which fired the event</param>
        /// <param name="tags">        The page tags the event was fired for.</param>
        /// <param name="e">           the keyboard event arguments.
        ///                            Can be null if the event was generated without keyboard input.
        ///                            </param>
        internal TagInputEventArgs(RoutedEvent routedEvent, object source, IEnumerable<string> tags, KeyEventArgs e)
            : base(routedEvent, source)
        {
            Tags = tags;
            TagInputComplete = false;
            Action = TaggingAction.None;
            if (e != null) {
                if (e.Key == System.Windows.Input.Key.Escape) {
                    TagInputComplete = true;
                    if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.None) {
                        Action = TaggingAction.Clear;
                    }
                } else if (e.Key == System.Windows.Input.Key.Enter) {
                    TagInputComplete = true;
                    switch (Keyboard.Modifiers) {
                        case ModifierKeys.Shift:
                            Action = TaggingAction.Add;
                            break;

                        case ModifierKeys.Control:
                            Action = TaggingAction.Remove;
                            break;

                        default:
                            if ((Keyboard.Modifiers & (ModifierKeys.Shift | ModifierKeys.Control)) != ModifierKeys.None) {
                                Action = TaggingAction.Set;
                            }
                            break;
                    }
                } else if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None) {
                    if (e.Key == System.Windows.Input.Key.OemPlus || e.Key == System.Windows.Input.Key.Add) {
                        TagInputComplete = true;
                        Action = TaggingAction.Add;
                    } else if (e.Key == System.Windows.Input.Key.OemMinus || e.Key == System.Windows.Input.Key.OemMinus) {
                        TagInputComplete = true;
                        Action = TaggingAction.Remove;
                    }
                }
            }
        }
    }
}