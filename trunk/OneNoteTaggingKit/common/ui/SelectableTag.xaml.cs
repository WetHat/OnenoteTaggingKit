using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// Interaction logic for SelectableTag.xaml
    /// </summary>
    public partial class SelectableTag : UserControl, INotifyPropertyChanged
    {

        static readonly PropertyChangedEventArgs PAGE_COUNT = new PropertyChangedEventArgs("PageCount");
        static readonly PropertyChangedEventArgs PAGE_COUNT_VISIBILITY = new PropertyChangedEventArgs("PageCountVisibility");
        static readonly PropertyChangedEventArgs HIGHLIGHT_COLOR = new PropertyChangedEventArgs("HighlightColor");
        static readonly PropertyChangedEventArgs VISIBILITY = new PropertyChangedEventArgs("Visibility");
        
        ISelectableTagModel _model;

        Brush _highlightColor = Brushes.Yellow;

        bool _isChecked = false;

        public SelectableTag()
        {
            InitializeComponent();
        }

        void firePropertyChanged(PropertyChangedEventArgs args)
        {
            if (args != null && PropertyChanged != null)
            {
                PropertyChanged(this,args);
            }
        }

        public SelectableTag(ISelectableTagModel model) : this()
        {
            _model = model;
            model.PropertyChanged += OnModelPropertyChanged;
        }

        public int PageCount
        {
            get
            {
                return _model != null ? _model.PageCount : 0;
            }
        }

        public Brush HighlightColor
        {
            get
            {
                return _highlightColor;
            }
            set
            {
                if (!_highlightColor.Equals(value))
                {
                    _highlightColor = value;
                    firePropertyChanged(HIGHLIGHT_COLOR);
                }
            }
        }

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    if (_model != null)
                    {
                        _model.IsChecked = _isChecked;
                    }
                }
            }
        }
        public Visibility PageCountVisibility
        {
            get
            {
                return PageCount < 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ISelectableTagModel mdl = sender as ISelectableTagModel;

            switch (e.PropertyName)
            {
                case "PageCount":
                    firePropertyChanged(PAGE_COUNT);
                    firePropertyChanged(PAGE_COUNT_VISIBILITY);
                    break;
                case "Visibility":
                    firePropertyChanged(VISIBILITY);
                    break;
            }
        }

        internal string[] Filter
        {
            set
            {
                tagName.Inlines.Clear();
                string tagname=_model.TagName;
                bool matched = false;
                if (value != null)
                {
                    // find a match
                    
                    foreach (string pattern in value)
                    {
                        int index = tagname.IndexOf(pattern, StringComparison.CurrentCultureIgnoreCase);
                        if (index >= 0)
                        {
                            // build UI
                            tagName.Inlines.Add(new Run (tagname.Substring(0,index)));
                            
                            Run r = new Run(tagname.Substring(index,pattern.Length));
                            r.Background = HighlightColor;
                            tagName.Inlines.Add(r);
                            tagName.Inlines.Add(new Run(tagname.Substring(index + pattern.Length)));
                            matched = true;
                            break;
                        }
                    }
                }

                if (!matched)
                {
                    tagName.Inlines.Add(new Run(_model.TagName));
                }
            }
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion INotifyPropertyChanged
    }
}
