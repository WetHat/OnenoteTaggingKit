using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for HitHighlightedTag.xaml
    /// </summary>
    public partial class HitHighlightedTagButton : UserControl
    {
        /// <summary>
        /// Click event for this button.
        /// </summary>
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HitHighlightedTagButton));

        public HitHighlightedTagButton()
        {
            InitializeComponent();
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

        private void createHitHighlightedTag(IHitHighlightedTagButtonModel mdl)
        {
            hithighlightedTag.Inlines.Clear();
            if (mdl.Hit.Length > 0)
            {
                hithighlightedTag.Inlines.Add(new Run(mdl.TagName.Substring(0, mdl.Hit.Index)));

                Run r = new Run(mdl.TagName.Substring(mdl.Hit.Index, mdl.Hit.Length));
                r.Background = Brushes.Yellow;
                hithighlightedTag.Inlines.Add(r);

                hithighlightedTag.Inlines.Add(new Run(mdl.TagName.Substring(mdl.Hit.Index + mdl.Hit.Length)));
            }
            else
            { 
                hithighlightedTag.Inlines.Add(new Run(mdl.TagName));
            }
        }
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            IHitHighlightedTagButtonModel mdl = DataContext as IHitHighlightedTagButtonModel;

            if (mdl != null)
            {
                createHitHighlightedTag(mdl);
                mdl.PropertyChanged += mdl_PropertyChanged;
            }
        }

        void mdl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == HitHighlightedTagButtonModel.HIT_Property)
            {
                IHitHighlightedTagButtonModel mdl = sender as IHitHighlightedTagButtonModel;
                createHitHighlightedTag(mdl);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent,this));
        }
    }
}
