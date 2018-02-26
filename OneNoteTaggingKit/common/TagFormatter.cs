// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    /// Format tags.
    /// </summary>
    internal class TagFormatter
    {
        /// <summary>
        /// Tag formatting options.
        /// </summary>
        internal enum TagFormat
        {
            /// <summary>
            /// Tags start with an uppercase letter.
            /// </summary>
            Capitalized = 0,

            /// <summary>
            /// Tags are lowercase, without spaces, and start with a '#'
            /// </summary>
            HashTag = 1,

            /// <summary>
            /// Tags are as entered by the user
            /// </summary>
            AsEntered = 2
        }

        /// <summary>
        /// Format a page tag according to the configured option in app settings.
        /// Avalailable formatting options are defined in the <see cref="TagFormat" /> enumeration.
        /// </summary>
        /// <param name="tag">tag string to format.</param>
        /// <returns>formatted tag string.</returns>
        internal static string Format(string tag)
        {
            switch ((TagFormat)Properties.Settings.Default.TagFormatting)
            {
                case TagFormat.Capitalized:
                    return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tag);

                case TagFormat.HashTag:
                    var t = CultureInfo.CurrentCulture.TextInfo.ToLower(tag.Replace(" ", ""));
                    return t.StartsWith("#") ? t : "#" + t;

                default:
                    return tag;
            }
        }
    }
}