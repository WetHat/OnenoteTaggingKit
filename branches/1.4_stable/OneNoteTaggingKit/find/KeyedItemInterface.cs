using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WetHatLab.OneNote.TaggingKit.find
{
    // Contract for class which want to provide a unique Key suitable for sorting
    interface IKeyedItem
    {
        /// <summary>
        /// Get a sortable key
        /// </summary>
        string Key { get; }
    }
}
