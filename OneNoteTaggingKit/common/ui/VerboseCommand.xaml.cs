using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    ///     Interaction logic the dialog which executes a headless command
    ///     while displaying a message which looks like a popup over the OneNote
    ///     application window.
    /// </summary>
    public partial class VerboseCommand : Window, IOneNotePageWindow<VerboseCommandModel>
    {
        /// <summary>
        ///     Initialize the dialog componements.
        /// </summary>
        public VerboseCommand() {
            InitializeComponent();
        }

        #region IOneNotePageWindow<MessageModel>
        private VerboseCommandModel _model;
        /// <summary>
        /// get or set the view model backing this UI
        /// </summary>
        public VerboseCommandModel ViewModel {
            get => _model;
            set {
               DataContext = _model = value;
            }
        }
        #endregion IOneNotePageWindow<MessageModel>

        #region Event Handlers
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            Close();
        }

        private void Window_StylusDown(object sender, StylusDownEventArgs e) {
            Close();
        }

        private void Window_TouchDown(object sender, TouchEventArgs e) {
            Close();
        }

        void AutoClose(object state) {
            Dispatcher.Invoke(() => Close());
        }

        DispatcherTimer _t;
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            _t = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, ViewModel.DisplayTimeMillies),
                                     DispatcherPriority.Normal,
                                     (source, ev) => Close(),
                                     Dispatcher);
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (_t != null) {
                _t.Stop();
                _t = null;
            }
        }
 
        private void Window_ContentRendered(object sender, System.EventArgs e) {
            ViewModel.Command.Invoke();
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e) {
            Close();
        }

        private void Window_StylusLeave(object sender, StylusEventArgs e) {
            Close();
        }

        #endregion Event Handlers
    }
}