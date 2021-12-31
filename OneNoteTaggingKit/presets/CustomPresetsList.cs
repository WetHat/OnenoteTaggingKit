using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace WetHatLab.OneNote.TaggingKit.presets
{
    public class CustomPresetsList : ObservableCollection<CustomPresetModel>
    {
        /// <summary>
        /// Create a new observable of custom presets from persisted data.
        /// </summary>
        /// <param name="presets"></param>
        public CustomPresetsList(StringCollection presets) {
            foreach (string p in presets) {
                Add(new CustomPresetModel(p));
            }
        }
    }
}
