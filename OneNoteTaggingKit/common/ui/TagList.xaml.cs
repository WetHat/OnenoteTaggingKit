using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// View for tags represented by data context objects
    /// coming from lists of type <see cref="TagSource"/>.
    /// </summary>
    [ComVisible(false)]
    public partial class TagList : UserControl
    {
        #region TagTemplateProperty
        /// <summary>
        /// Backing store for the <see cref="TagTemplate"/> property.
        /// </summary>
        public static readonly DependencyProperty TagTemplateProperty = DependencyProperty.Register(
            nameof(TagTemplate),
            typeof(DataTemplate),
            typeof(TagList));

        /// <summary>
        /// Get or set the data template for rendering individual tags in the
        /// list.
        /// </summary>
        public DataTemplate TagTemplate {
            get => GetValue(TagTemplateProperty) as DataTemplate;
            set => SetValue(TagTemplateProperty, value);
        }
        #endregion TagTemplateProperty
        #region TagSourceProperty
        /// <summary>
        /// Definition of the <see cref="TagSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TagSourceProperty = DependencyProperty.Register(
                nameof(TagSource),
                typeof(IObservableTagList),
                typeof(TagList),
                new FrameworkPropertyMetadata(OnTagSourceChanged)
            );

        /// <summary>
        /// Get(set the observable list of tag data context objects managed by instances
        /// of this list control.
        /// </summary>
        public IObservableTagList TagSource {
            get => GetValue(TagSourceProperty) as IObservableTagList;
            set => SetValue(TagSourceProperty, value);
        }

        static void OnTagSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            var tl = d as TagList;
            if (d != null) {
                var oldSource = args.OldValue as IObservableTagList;
                if (oldSource != null) {
                    oldSource.CollectionChanged -= tl.OnTagSourceContentChanged;
                    tl.tagsPanel.Children.Clear();
                    oldSource.Dispose();
                }
                var newSource = args.NewValue as IObservableTagList;
                if (newSource != null) {
                    newSource.CollectionChanged += tl.OnTagSourceContentChanged;
                    // inform TagList control about the already existing tags
                    // by simulating a change event
                    tl.OnTagSourceContentChanged(tl,
                                 new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add,
                                                                      tl.TagSource.ToTagList(),0));
                }
            }
        }
        /// <summary>
        /// Update the tag list display based on the changes in the underlying
        /// tag model list.
        /// </summary>
        /// <param name="sender">The tag source where the changes occured.</param>
        /// <param name="e">Change details</param>
        void OnTagSourceContentChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    // Use the tag template to create the UI controls
                    // for the added tag models.
                    DataTemplate tpl = TagTemplate;
                    int newItemCount = e.NewItems.Count;
                    for (int i = 0; i < newItemCount; i++) {
                        FrameworkElement tagControl = tpl.LoadContent() as FrameworkElement;
                        tagControl.DataContext = e.NewItems[i];
                        tagsPanel.Children.Insert(i + e.NewStartingIndex, tagControl);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    int oldItemCount = e.OldItems.Count;
                    for (int i = 0; i < oldItemCount; i++) {
                        tagsPanel.Children.RemoveAt(e.OldStartingIndex);
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    foreach (FrameworkElement fe in tagsPanel.Children) {
                        fe.DataContext = null;
                    }
                    tagsPanel.Children.Clear();
                    break;
            }
        }
        #endregion TagSourceProperty
        #region HeaderProperty
        /// <summary>
        /// Dependency property for panel header.
        /// </summary>
        /// <seealso cref="Header" />
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(TagList),
            new FrameworkPropertyMetadata("Tag list"));

        /// <summary>
        /// Get or set the group header text for the tags list.
        /// </summary>
        public string Header {
            get => GetValue(HeaderProperty) as string;
            set => SetValue(HeaderProperty, value);
        }
        #endregion HeaderProperty
        #region NotificationProperty
        /// <summary>
        /// Dependecy property definition for <see cref="Notification"/>
        /// </summary>
        public static readonly DependencyProperty NotificationProperty = DependencyProperty.Register(
            nameof(Notification),
            typeof(string),
            typeof(TagList),
            new FrameworkPropertyMetadata(OnNotificationrPropertyChanged));

        /// <summary>
        /// React to changes to the notification. Opens the notification popup for
        /// 5 seconds.
        /// </summary>
        /// <param name="d">The <see cref="TagList"/> control hosting the notification.</param>
        /// <param name="args"></param>
        static void OnNotificationrPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            if (d is TagList tl) {
                string notification = args.NewValue as string;
                if (string.IsNullOrWhiteSpace(notification)) {
                    tl.notificationPopup.IsOpen = false;
                } else {
                    //tl.notificationText.Text = notification;
                    tl.notificationPopup.IsOpen = true;
                    DispatcherTimer closeTimer = new DispatcherTimer(tl.NotificationDisplayTime,
                        DispatcherPriority.Normal,
                        (sender, e) => {
                            var timer = sender as DispatcherTimer;
                            if (timer != null) {
                                timer.Stop();
                                if (tl.notificationPopup.IsOpen) {
                                    tl.Notification = string.Empty;
                                }
                            }
                        }, tl.Dispatcher);
                    closeTimer.Start();
                }
            }
        }
        /// <summary>
        /// Get or set the popup notification which is displayed for a few second
        /// over this list control.
        /// </summary>
        public string Notification {
            get => GetValue(NotificationProperty) as string;
            set => SetValue(NotificationProperty,value);
        }
        #endregion NotificationProperty

        private void handlePopupPointerAction(object sender, RoutedEventArgs e) {
            Popup p = sender as Popup;
            if (p != null) {
                p.IsOpen = false;
            }
            e.Handled = true;
        }

        /// <summary>
        /// Get or set the timespan the notification is displayed.
        /// </summary>
        /// <value>The default timespan is 5 seconds.</value>
        public TimeSpan NotificationDisplayTime { get; set; } = TimeSpan.FromSeconds(5);
        /// <summary>
        /// Create a new component instance.
        /// </summary>
        public TagList() {
            InitializeComponent();
        }
    }
}
