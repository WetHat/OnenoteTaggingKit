// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// Enumeration of scopes to search for tagged pages
    /// </summary>
    public enum SearchScope
    {
        /// <summary>
        /// OneNote section
        /// </summary>
        Section = 0,

        /// <summary>
        /// OneNote section group
        /// </summary>
        SectionGroup = 1,

        /// <summary>
        /// OneNote notebook
        /// </summary>
        Notebook = 2,

        /// <summary>
        /// All notebooks open in OneNote
        /// </summary>
        AllNotebooks = 3,
    }

    /// <summary>
    /// Search Scope UI facade
    /// </summary>
    public class SearchScopeFacade
    {
        /// <summary>
        /// Get or set the search scope
        /// </summary>
        public SearchScope Scope { get; set; }

        /// <summary>
        /// get or set the display label.
        /// </summary>
        public string ScopeLabel { get; set; }
    }

    /// <summary>
    /// Event details for the <see cref="E:WetHatLab.OneNote.TaggingKit.common.ui.ScopeSelector.ScopeChanged"/> event
    /// </summary>
    [ComVisible(false)]
    public class ScopeChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Get the selected scope
        /// </summary>
        public SearchScope Scope { get; private set; }

        /// <summary>
        /// Create a new instance of the event details
        /// </summary>
        /// <param name="routedEvent">routed event which fired</param>
        /// <param name="source">object which fired the event</param>
        /// <param name="selectedScope">selected scope</param>
        internal ScopeChangedEventArgs(RoutedEvent routedEvent, object source, SearchScope selectedScope)
            : base(routedEvent, source)
        {
            Scope = selectedScope;
        }
    }

    /// <summary>
    /// handlers for the <see cref="E:WetHatLab.OneNote.TaggingKit.common.ui.ScopeSelector.ScopeChanged"/> event
    /// </summary>
    /// <param name="sender">object emitting the event</param>
    /// <param name="e">event details</param>
    public delegate void ScopeChangedEventEventHandler(object sender, ScopeChangedEventArgs e);

    /// <summary>
    /// View model for the <see cref="ScopeSelector"/> UI control.
    /// </summary>
    public class ScopeSelectorModel
    {
        private static readonly SearchScopeFacade[] s_scopes = new SearchScopeFacade[] {
                                                                                new SearchScopeFacade {
                                                                                    Scope = SearchScope.Section,
                                                                                    ScopeLabel = Properties.Resources.TagSearch_Scope_Section_Label
                                                                                },
                                                                                new SearchScopeFacade {
                                                                                    Scope = SearchScope.SectionGroup,
                                                                                    ScopeLabel = Properties.Resources.TagSearch_Scope_SectionGroup_Label
                                                                                },
                                                                                new SearchScopeFacade {
                                                                                    Scope = SearchScope.Notebook,
                                                                                    ScopeLabel = Properties.Resources.TagSearch_Scope_Notebook_Label
                                                                                },
                                                                                new SearchScopeFacade {
                                                                                    Scope = SearchScope.AllNotebooks,
                                                                                    ScopeLabel = Properties.Resources.TagSearch_Scope_AllNotebooks_Label
                                                                                }
                                                                            };

        /// <summary>
        /// Get the collection of scope facade objects
        /// </summary>
        public IEnumerable<SearchScopeFacade> Scopes
        {
            get { return s_scopes; }
        }
    }

    /// <summary>
    /// Interaction logic for ScopeSelector.xaml
    /// </summary>
    [ComVisible(false)]
    public partial class ScopeSelector : UserControl
    {
        /// <summary>
        /// Dependency property for the selected scope
        /// </summary>
        /// <seealso cref="SelectedScope"/>
        public static readonly DependencyProperty SelectedScopeProperty = DependencyProperty.Register("SelectedScope", typeof(SearchScope), typeof(ScopeSelector), new PropertyMetadata(HandleScopeChange));

        private static void HandleScopeChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ScopeSelector control = d as ScopeSelector;
            if (control != null)
            {
                control.scopeSelect.SelectedIndex = (int)e.NewValue;
            }
        }

        /// <summary>
        /// Get or set the selected scope
        /// </summary>
        public SearchScope SelectedScope
        {
            get { return (SearchScope)GetValue(SelectedScopeProperty); }
            set
            {
                SetValue(SelectedScopeProperty, value);
                scopeSelect.SelectedIndex = (int)value;
            }
        }

        /// <summary>
        /// Routed event to inform subscribers about scope changes
        /// </summary>
        /// <seealso cref="ScopeChanged"/>
        public static readonly RoutedEvent ScopeChangedEvent = EventManager.RegisterRoutedEvent("ScopeChanged", RoutingStrategy.Direct, typeof(ScopeChangedEventEventHandler), typeof(ScopeSelector));

        /// <summary>
        /// Event to notify listeners when the scope has changed
        /// </summary>
        public event ScopeChangedEventEventHandler ScopeChanged
        {
            add { AddHandler(ScopeChangedEvent, value); }

            remove { RemoveHandler(ScopeChangedEvent, value); }
        }

        /// <summary>
        /// Create a new instance of a scope selector component
        /// </summary>
        public ScopeSelector()
        {
            InitializeComponent();
            scopeSelect.DataContext = new ScopeSelectorModel();
        }

        private void ScopeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedScope = (SearchScope)scopeSelect.SelectedIndex;
            RaiseEvent(new ScopeChangedEventArgs(ScopeChangedEvent, this, SelectedScope));
        }
    }
}