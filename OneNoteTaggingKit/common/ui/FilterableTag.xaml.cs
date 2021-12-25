using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// A control to present selectable tags.
    /// </summary>
    public partial class FilterableTag : UserControl
    {
        #region TagSelectedEvent
        public static readonly RoutedEvent TagSelectedEvent = EventManager.RegisterRoutedEvent(
            nameof(TagSelected),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(FilterableTag));

        /// <summary>
        /// Track changes to tag (de)selection.
        /// </summary>
        public event RoutedEventHandler TagSelected {
            add { AddHandler(TagSelectedEvent, value); }
            remove { RemoveHandler(TagSelectedEvent, value); }
        }
        #endregion TagSelectedEvent
        void UpdateTagNameHighlight(IList<TextFragment> highlightedName) {
            tagName.Inlines.Clear();
            foreach (var f in highlightedName) {
                Run r = new Run(f.Text);
                if (f.IsMatch) {
                    r.Background = Brushes.Yellow;
                }
                tagName.Inlines.Add(r);
            }
        }

        /// <summary>
        /// Handle changes in the data context.
        /// </summary>
        /// <param name="sender">View model that changed its state.</param>
        /// <param name="args">Event details.</param>
        void OnModelPropertyChanged(object sender, PropertyChangedEventArgs args) {
            if (sender is FilterableTagModel stm) {
                switch (args.PropertyName) {
                    case nameof(FilterableTagModel.HighlightedTagName):
                        UpdateTagNameHighlight(stm.HighlightedTagName);
                        break;
                }
            }
        }
        /// <summary>
        /// Create a new instance of a control to provide tag selection.
        /// </summary>
        public FilterableTag() {
            InitializeComponent();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue is FilterableTagModel oldMdl) {
                oldMdl.Dispose();
            }

            if (e.NewValue is FilterableTagModel newMdl) {
                UpdateTagNameHighlight(newMdl.HighlightedTagName);
                newMdl.PropertyChanged += OnModelPropertyChanged;
            }
        }
        private void tagBtn_Checked(object sender, RoutedEventArgs e) {
            var mdl = DataContext as FilterableTagModel;
            RaiseEvent(new TagSelectedEventArgs(TagSelectedEvent,this, mdl));
        }
    }
}
