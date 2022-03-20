// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    /// Interaction logic for RemovableTag.xaml
    /// </summary>
    [ComVisible(false)]
    public partial class RemovableTag : UserControl
    {
        #region ActionEvent
        /// <summary>
        /// Click event for this button.
        /// </summary>
        public static readonly RoutedEvent ActionEvent = EventManager.RegisterRoutedEvent("Action", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(RemovableTag));

        /// <summary>
        /// create a new instance of a <see cref="RemovableTag"/> user control
        /// </summary>
        public RemovableTag()
        {
            InitializeComponent();
            DisableEditing();
            ShowActions();
        }

        /// <summary>
        /// Add or remove the handler for tag edit actions.
        /// </summary>
        internal event RoutedEventHandler Action
        {
            add { AddHandler(ActionEvent, value); }
            remove { RemoveHandler(ActionEvent, value); }
        }
        #endregion ActionEvent
        /// <summary>
        /// Hoopk up to the view model when it is set.
        /// </summary>
        /// <param name="sender">The UI control where the data context is set</param>
        /// <param name="e">Event details</param>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RemovableTag t = sender as RemovableTag;

            if (t.DataContext is RemovableTagModel mdl) {
                mdl.PropertyChanged += mdl_PropertyChanged;
                mdl_PropertyChanged(mdl, new PropertyChangedEventArgs(nameof(RemovableTagModel.HighlightedTagName)));
            }
        }

        /// <summary>
        /// Track property changes in the view model.
        /// </summary>
        /// <param name="sender">The view model whose property changed</param>
        /// <param name="e">Property event details</param>
        private void mdl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is RemovableTagModel mdl) {
                switch (e.PropertyName) {
                    case nameof(RemovableTagModel.HighlightedTagName):
                        tagName.Inlines.Clear();
                        tagName.Inlines.AddRange(mdl.HighlightedTagName.Select((f) => {
                            Run r = new Run(f.Text);
                            if (f.IsMatch) {
                                r.Background = Brushes.Yellow;
                            }
                            return r;
                        }));
                        break;
                }
            }
        }

        private void ShowConfirmation()
        {
            confirmAction.Visibility = Visibility.Visible;
            cancelAction.Visibility = Visibility.Visible;

            renameAction.Visibility = Visibility.Collapsed;
            deleteAction.Visibility = Visibility.Collapsed;
        }

        private void ShowActions()
        {
            confirmAction.Visibility = Visibility.Collapsed;
            cancelAction.Visibility = Visibility.Collapsed;

            renameAction.Visibility = Visibility.Visible;
            deleteAction.Visibility = Visibility.Visible;
        }

        private void EnableEditing()
        {
            tagName.Visibility = Visibility.Collapsed;
            tagType.Visibility = Visibility.Collapsed;
            tagNameEditBox.Visibility = Visibility.Visible;
            tagNameEditBox.Focus();
            tagNameEditBox.SelectAll();
        }

        private void DisableEditing()
        {
            tagName.Visibility = Visibility.Visible;
            tagType.Visibility = Visibility.Visible;
            tagNameEditBox.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Process clicks on the menu items associated with this tag.
        /// </summary>
        /// <param name="sender">Menu item that was clicked.</param>
        /// <param name="e">Event details</param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem itm = sender as MenuItem;
            RemovableTagModel mdl = DataContext as RemovableTagModel;
            switch (itm.Tag.ToString())
            {
                case "DeleteTag":
                    Tag = itm.Tag; // Remember the action for the confiramtion stage
                    ShowConfirmation();
                    actionMenu.IsSubmenuOpen = true;
                    break;

                case "RenameTag":
                    Tag = itm.Tag; // Remember the action for the confiramtion stage
                    EnableEditing();
                    ShowConfirmation();
                    actionMenu.IsSubmenuOpen = false;
                    break;

                case "CancelAction":
                    if ("RenameTag".Equals(Tag)) {
                        // revert any name edits.
                        mdl.LocalName = mdl.TagName;
                    }
                    DisableEditing();
                    ShowActions();
                    actionMenu.IsSubmenuOpen = false;
                    break;

                case "ConfirmAction":
                    DisableEditing();
                    ShowActions();
                    actionMenu.IsSubmenuOpen = false;
                    switch (Tag) {
                        case "RenameTag":
                            mdl.LocalName = tagNameEditBox.Text.Trim();
                            if (!mdl.LocalName.Equals(mdl.TagName, StringComparison.CurrentCultureIgnoreCase)) {
                                // Process rename
                                RaiseEvent(new RoutedEventArgs(ActionEvent, this));
                            }
                            break;
                        default:
                            RaiseEvent(new RoutedEventArgs(ActionEvent, this));
                            break;
                    }
                    break;
            }
        }

        private void tagNameEditBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            RemovableTagModel mdl = DataContext as RemovableTagModel;
            switch (e.Key) {
                case System.Windows.Input.Key.Escape:
                    mdl.LocalName = mdl.TagName;
                    DisableEditing();
                    ShowActions();
                    actionMenu.IsSubmenuOpen = false;
                    e.Handled = true;
                    break;
                case System.Windows.Input.Key.Enter:
                    ShowConfirmation();
                    actionMenu.IsSubmenuOpen = true;
                    break;
            }
        }
        private void tagNameEditBox_LostFocus(object sender, RoutedEventArgs e) {
            if (DataContext is RemovableTagModel mdl
                && mdl.LocalName.Equals(tagNameEditBox.Text)) {
                DisableEditing();
                ShowActions();
                actionMenu.IsSubmenuOpen = false;
            }
            e.Handled = true;
        }
    }
}