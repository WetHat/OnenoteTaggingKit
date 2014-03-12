using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WetHatLab.OneNote.TaggingKit.common.ui
{
    public class TagModelKey : IEquatable<TagModelKey>, IComparable<TagModelKey>
    {
        private string _tagKey;

        internal TagModelKey(string tagname)
        {
            _tagKey = tagname.ToLower();
        }

        internal string TagKey
        {
            get
            {
                return _tagKey;
            }
        }
        #region IComparable<TagModelKey>
        public int CompareTo(TagModelKey other)
        {
            return _tagKey.CompareTo(other.TagKey);
        }
        #endregion IComparable<string>

        #region IEquatable<TagModelKey>
        public bool Equals(TagModelKey other)
        {
            return _tagKey.Equals(other.TagKey);
        }
        #endregion

        public override int GetHashCode()
        {
            return _tagKey.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            TagModelKey otherkey = obj as TagModelKey;
            if (otherkey == null)
            {
                return false;
            }
            return _tagKey.Equals(otherkey.TagKey);
        } 
    }
}
