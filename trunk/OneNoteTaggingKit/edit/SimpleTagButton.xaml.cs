﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    ///  A simple Tag Button.
    /// </summary>
    public partial class SimpleTagButton : UserControl
    {
        /// <summary>
        /// Dependency property for the collection of tags this panel displays.
        /// </summary>
        public static readonly DependencyProperty TagNameProperty = DependencyProperty.Register("TagName", typeof(SimpleTag), typeof(SimpleTagButton));
 
        /// <summary>
        /// Click event for this button.
        /// </summary>
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SimpleTagButton));

        /// <summary>
        /// Create a new instance of a simple Tag button.
        /// </summary>
        public SimpleTagButton()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Get or set the TagName dependency property;
        /// </summary>
        internal SimpleTag TagName
        {
            get { return (SimpleTag)GetValue(TagNameProperty); }
            set { SetValue(TagNameProperty, value); }
        }

        /// <summary>
        /// Add or remove the click handler.
        /// </summary>
        /// <remarks>clr wrapper for routed event</remarks>
        internal event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }

            remove { RemoveHandler(ClickEvent, value); }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs newClickEventArgs = new RoutedEventArgs(ClickEvent,this);

            RaiseEvent(newClickEventArgs);
        }
    }
}
