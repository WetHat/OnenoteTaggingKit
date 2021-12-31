using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WetHatLab.OneNote.TaggingKit.common;
using WetHatLab.OneNote.TaggingKit.common.ui;

namespace WetHatLab.OneNote.TaggingKit.presets
{
    /// <summary>
    /// View model of a custom tag collection preset.
    /// </summary>
    public class CustomPresetModel: ObservableObject {
        string _title = string.Empty;
        /// <summary>
        /// Get/set the preset's title.
        /// </summary>
        public string Title {
            get => _title;
            set {
                _title = value;
                RaisePropertyChanged();
            }
        }

        ObservableTagList<SelectedTagModel> _selectedTags = null;
        /// <summary>
        /// Get an overvable list of view models for the tags in the preset.
        /// </summary>
        public ObservableTagList<SelectedTagModel> SelectedTags {
            get {
                if (_selectedTags == null) {
                    //lazy construction
                    _selectedTags = new ObservableTagList<SelectedTagModel>();

                    // add all tags
                    _selectedTags.AddAll(from t in TagNames
                                         select new SelectedTagModel() {
                                             TagName = t
                                         });
                }
                return _selectedTags;
            }
        }

        /// <summary>
        /// Get the names of the tags in the preset.
        /// </summary>
        IEnumerable<string> TagNames { get;}

        /// <summary>
        /// Generate a presistable string representation of the preset-
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return string.Format("{{{{{0}}}}}{1}", Title, string.Join(",", TagNames));
        }

        /// <summary>
        /// Create a new instance of the view model from a serialized preset.
        /// definition.
        /// </summary>
        /// <remarks>
        /// The preset has the form
        /// <code>{{title}}tag1,tag2,...</code>.
        /// </remarks>
        /// <param name="preset"></param>
        public CustomPresetModel(string preset) {

            int titleNdx = preset.IndexOf("}}");
            if (titleNdx >=0 ) {
                Title = preset.Substring(2, titleNdx - 2);
            }

            TagNames = OneNotePageProxy.ParseTags(preset.Substring(titleNdx+2));
        }

        /// <summary>
        /// Create a new empty preset.
        /// </summary>
        public CustomPresetModel() {
        }
    }
}
