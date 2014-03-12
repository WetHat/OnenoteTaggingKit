using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WetHatLab.OneNote.TaggingKit.find
{
    class TagSelectorDesignerModel : ITagSelectorModel
    {
        #region ITagSelectorModel
        public bool IsChecked
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        public int PageCount
        {
            get { return 1; }
        }

        public string TagName
        {
            get { return "A Tag"; }
        }
        #endregion
    }
}
