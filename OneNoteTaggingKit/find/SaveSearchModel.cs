using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.find
{
    public class SaveSearchModel : WindowViewModelBase
    {
        public IEnumerable<SelectedTagModel> Tags { get; set; }
        public SaveSearchModel(OneNoteProxy onenote) : base(onenote) {}

    }
}
