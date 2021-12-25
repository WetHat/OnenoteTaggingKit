using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    public class SelectableTagDesignerModel : SelectableTagModel
    {
        public SelectableTagDesignerModel() {
            TagName = "Highlightable";
            Highlighter = new TextSplitter(new string[] { "tab" });
        }
    }
}
