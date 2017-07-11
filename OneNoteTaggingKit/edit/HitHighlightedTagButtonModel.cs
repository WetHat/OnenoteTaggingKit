using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.edit
{
    /// <summary>
    /// View model for the <see cref="HitHighlightedTagButton"/> control.
    /// </summary>
    public class HitHighlightedTagButtonModel : SuggestedTagDataContext
    { /// <summary>
        /// predefined event descriptor for <see cref="E:PropertyChanged"/> event fired for the <see cref="Visibility"/> property
        /// </summary>
        internal static readonly PropertyChangedEventArgs VISIBILITY_Property = new PropertyChangedEventArgs("Visibility");
        
        bool _isFiltered = false;

        /// <summary>
        /// Create a new instance of the control
        /// </summary>
        public HitHighlightedTagButtonModel()
        {
        }
       
        /// <summary>
        /// Get the visibility the associated <see cref="HitHighlightedTagButton"/> control has. 
        /// </summary>
        public Visibility Visibility
        {
            get
            {
                return !_isFiltered || HasHighlights ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }

        
        /// <summary>
        /// Set a filter string which is used to determine the appearance of the <see cref="HitHighlightedTagButton"/>
        /// control.
        /// </summary>
        /// <remarks>
        /// Setting this property has a side effect on the <see cref="Visibility"/> property.
        /// The appropriate <see cref="E:WetHatLab.OneNote.TaggingKit.edit.PropertyChanged"/> events are fired as necessary.
        /// </remarks>
        public override TextSplitter Highlighter
        {
            set
            {
                Visibility visBefore = Visibility;

                _isFiltered = value.SplitPattern != null;
                base.Highlighter = value;
                if (visBefore != Visibility)
                {
                    firePropertyChanged(VISIBILITY_Property);
                }
            }
        }
    }
}
