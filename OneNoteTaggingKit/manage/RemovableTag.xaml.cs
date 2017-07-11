// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    /// Interaction logic for RemovableTag.xaml
    /// </summary>
    [ComVisible(false)]
    public partial class RemovableTag : UserControl
    {
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
            HideConfirmation();
            DisableEditing();
        }

        /// <summary>
        /// Add or remove the click handler.
        /// </summary>
        /// <remarks>clr wrapper for routed event</remarks>
        internal event RoutedEventHandler Action
        {
            add { AddHandler(ActionEvent, value); }

            remove { RemoveHandler(ActionEvent, value); }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            RemovableTag t = sender as RemovableTag;

            RemovableTagModel mdl = t.DataContext as RemovableTagModel;
            if (mdl != null)
            {
                mdl.PropertyChanged += mdl_PropertyChanged;
                mdl_PropertyChanged(mdl, RemovableTagModel.HIGHLIGHTED_TAGNAME);
            }
        }

        private void mdl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RemovableTagModel mdl = sender as RemovableTagModel;
            if (mdl != null)
            {
                if (e == RemovableTagModel.HIGHLIGHTED_TAGNAME)
                {
                    highlighedTag.Inlines.Clear();
                    highlighedTag.Inlines.AddRange(mdl.HighlightedTagName.Select((f) =>
                    {
                        Run r = new Run(f.Text);
                        if (f.IsMatch)
                        {
                            r.Background = Brushes.Yellow;
                        }
                        return r;
                    }));
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

        private void HideConfirmation()
        {
            confirmAction.Visibility = Visibility.Collapsed;
            cancelAction.Visibility = Visibility.Collapsed;

            renameAction.Visibility = Visibility.Visible;
            deleteAction.Visibility = Visibility.Visible;
        }

        private void EnableEditing()
        {
            tagNameEditBox.Visibility = Visibility.Visible;
            highlighedTag.Visibility = Visibility.Collapsed;
            tagNameEditBox.Focus();
            tagNameEditBox.SelectAll();
        }

        private void DisableEditing()
        {
            tagNameEditBox.Visibility = Visibility.Collapsed;
            highlighedTag.Visibility = Visibility.Visible;
            tagNameEditBox.Focus();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem itm = sender as MenuItem;
            RemovableTagModel mdl = DataContext as RemovableTagModel;
            switch (itm.Tag.ToString())
            {
                case "DeleteTag":
                    Tag = itm.Tag;
                    if (mdl.CanRemove)
                    {
                        RaiseEvent(new RoutedEventArgs(ActionEvent, this));
                    }
                    else
                    {
                        ShowConfirmation();
                        actionMenu.IsSubmenuOpen = true;
                    }
                    break;

                case "RenameTag":
                    Tag = itm.Tag;
                    EnableEditing();
                    ShowConfirmation();
                    break;

                case "CancelAction":
                    HideConfirmation();
                    DisableEditing();
                    break;

                case "ConfirmAction":
                    HideConfirmation();
                    DisableEditing();
                    if (!"RenameTag".Equals(itm.Tag) || mdl.LocalName.Equals(mdl.TagName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        RaiseEvent(new RoutedEventArgs(ActionEvent, this));
                    }

                    break;
            }
        }

        private void tagNameEditBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            RemovableTagModel mdl = DataContext as RemovableTagModel;
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                e.Handled = true;
                HideConfirmation();
                DisableEditing();
            }
        }
    }
}