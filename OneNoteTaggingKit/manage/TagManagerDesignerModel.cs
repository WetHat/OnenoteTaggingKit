// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using WetHatLab.OneNote.TaggingKit.common;

namespace WetHatLab.OneNote.TaggingKit.manage
{
    /// <summary>
    /// View model to support the design mode for the <see cref="TagManager" /> dialog
    /// </summary>
    public class TagManagerDesignerModel : ITagManagerModel
    {
        private KnownTagsSource<RemovableTagModel> _tags = new KnownTagsSource<RemovableTagModel>();

        /// <summary>
        /// Create a new instance of the view model including some dummy data.
        /// </summary>
        public TagManagerDesignerModel() {
            _tags.AddAll(new RemovableTagModel[] { new RemovableTagModel() { PageTag = new TagPageSet(new PageTag("suggested tag 1",PageTagType.PlainTag)) },
                                                   new RemovableTagModel() { PageTag = new TagPageSet(new PageTag("suggested tag 2", PageTagType.PlainTag)) }});
        }

        /// <summary>
        /// Get the collection of tags used for suggestions.
        /// </summary>
        public KnownTagsSource<RemovableTagModel> SuggestedTags {
            get {
                return _tags;
            }
        }

        /// <summary>
        /// Get the add-in build configuration.
        /// </summary>
        public string Configuration {
            get {
                return "Debug";
            }
        }

        /// <summary>
        /// Get the version string for the addin
        /// </summary>
        public string AddinVersion {
            get { return "1.2.3.4"; }
        }

        /// <summary>
        /// Get the design time version of the .net framework
        /// </summary>
        public string FrameworkVersion {
            get { return Environment.Version.ToString(); }
        }

        /// <summary>
        /// Get the design time log file location
        /// </summary>
        public string Logfile {
            get { return TraceLogger.LogFile; }
        }
    }
}