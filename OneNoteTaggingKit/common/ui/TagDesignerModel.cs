using System.Windows.Media;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// Tag contros design time support.
    /// </summary>
    class TagDesignerModel : ITagModel
    {
        public string TagName => "Tag";

        public string TagTypePrefix => "?";

        public string TagTypePostfix => "¿";

        public Brush TagIndicatorColor => Brushes.Red;

        public string TagIndicator => "❌";
    }
}
