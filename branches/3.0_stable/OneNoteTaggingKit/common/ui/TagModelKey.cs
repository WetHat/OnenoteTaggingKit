// Author: WetHat | (C) Copyright 2013 - 2017 WetHat Lab, all rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    /// <summary>
    /// Sortable key for OneNote page tags for use in view models.
    /// </summary>
    public class TagModelKey : IEquatable<TagModelKey>, IComparable<TagModelKey>
    {
        private string _tagKey;

        /// <summary>
        /// create a new instance of a tag key
        /// </summary>
        /// <param name="tagname">name of the tag</param>
        internal TagModelKey(string tagname)
        {
            _tagKey = tagname.ToLower();
        }

        #region IComparable<TagModelKey>

        /// <summary>
        /// compare order two view models
        /// </summary>
        /// <param name="other">other key to compare against</param>
        /// <returns>
        /// depending on the order of the view models relative to each other returns:
        /// <list type="bullet">
        /// <item>a negative number if this key comes before the other key</item>
        /// <item>0 if both keys are equivalent</item>
        /// <item>a positive number if this key comes after the other key</item>
        /// </list>
        /// </returns>
        public int CompareTo(TagModelKey other)
        {
            return _tagKey.CompareTo(other._tagKey);
        }

        #endregion IComparable<TagModelKey>

        #region IEquatable<TagModelKey>

        /// <summary>
        /// compare two key objects for equality
        /// </summary>
        /// <param name="other">other key to campare against</param>
        /// <returns>true if both key objects are equivalent; false otherwise</returns>
        public bool Equals(TagModelKey other)
        {
            return _tagKey.Equals(other._tagKey);
        }

        #endregion IEquatable<TagModelKey>

        /// <summary>
        /// Get the hash code of a tag key
        /// </summary>
        /// <returns>hash code based on the tag key</returns>
        public override int GetHashCode()
        {
            return _tagKey.GetHashCode();
        }

        /// <summary>
        /// compare two instances of a tag key for equality.
        /// </summary>
        /// <param name="obj">other tag key object</param>
        /// <returns>true if both tag keys are equal, false otherwise</returns>
        public override bool Equals(object obj)
        {
            TagModelKey otherkey = obj as TagModelKey;
            if (otherkey == null)
            {
                return false;
            }
            return _tagKey.Equals(otherkey._tagKey);
        }
    }
}