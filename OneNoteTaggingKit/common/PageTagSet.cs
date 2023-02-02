using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WetHatLab.OneNote.TaggingKit.common
{
    /// <summary>
    ///     A consolidated set of page tags.
    /// </summary>
    /// <remarks>
    ///     The term _consolidated_ relates to a special property that allows
    ///     <see cref="PageTag"/> objects different types with the same key to
    ///     be represented by their highest ranked type.
    /// </remarks>
    public class PageTagSet : IEnumerable<PageTag>
    {
        /// <summary>
        /// The supported taglist separators
        /// </summary>
        public readonly static char[] sTagListSeparators = new char[] { ',' };

        Dictionary<string, PageTag> _pagetags = new Dictionary<string, PageTag>();

        /// <summary>
        /// Split a comma separated list of tags into a collection of individual tags.
        /// </summary>
        /// <param name="taglist">The comma separated list of tagnames. HTML markup is not allowed</param>
        /// <returns>Collection if individiual tags.</returns>
        public static IEnumerable<string> SplitTaglist(string taglist) {
            return from t in taglist.Split(sTagListSeparators, StringSplitOptions.RemoveEmptyEntries)
                   select t.Trim();
        }

        /// <summary>
        ///     Determines if a given set of tags is a subset of this tag set.
        /// </summary>
        /// <param name="tags">
        ///     A set of tags to test the containment for.
        /// </param>
        /// <returns>`true` if all given tags are contained in this set.</returns>
        public bool ContainsAll(IEnumerable<PageTag> tags) {
            return tags.All((t) => _pagetags.ContainsKey(t.Key));
        }

        /// <summary>
        ///     Determines if a given set of tags has tags in common with this tag set.
        /// </summary>
        /// <param name="tags">
        ///     A set of tags to test.
        /// </param>
        /// <returns>`true` if the two tag sets have common tags.</returns>
        public bool ContainsAny(IEnumerable<PageTag> tags) {
            return tags.Any((t) => _pagetags.ContainsKey(t.Key));
        }

        /// <summary>
        /// Initialize a page tag set with a single page tag.
        /// </summary>
        /// <param name="tag">A page tags.</param>
        public PageTagSet(PageTag tag) {
            Add(tag);
        }

        /// <summary>
        /// Initialize a page tag set with a collection of page tags.
        /// </summary>
        /// <param name="tags">Collection of page tags.</param>
        public PageTagSet(IEnumerable<PageTag> tags) {
            UnionWith(tags);
        }

        /// <summary>
        /// Initialize a new instance from a list of tag names.
        /// </summary>
        /// <remarks>
        /// This function does **not** handle HTML markup in the taglist.
        /// </remarks>
        /// <param name="tagnames">List of tag names.</param>
        /// <param name="format">The tag formatting to apply.</param>
        public PageTagSet(IEnumerable<string> tagnames, TagFormat format)
            :this(Parse(tagnames,format)) {
        }

        /// <summary>
        /// Initialize a new instance from comma separated list of tag names.
        /// </summary>
        /// <remarks>
        /// This function does **not** handle HTML markup in the taglist.
        /// </remarks>
        /// <param name="taglist">Comma separated list of tag names.</param>
        /// <param name="format">The tag formatting to apply.</param>
        public PageTagSet(string taglist, TagFormat format)
        {
            UnionWith(Parse(taglist, format));
        }

        /// <summary>
        /// Initialize an empty set of page tags.
        /// </summary>
        public PageTagSet() { }

        /// <summary>
        /// Add a page tag to the set.
        /// </summary>
        /// <remarks>
        ///     If a tag with the same key is already in the set a conflict
        ///     resolution algorithm decides if the given tag should replace the
        ///     tag in the set.
        /// </remarks>
        /// <param name="pagetag">Page tag to add.</param>
        public void Add(PageTag pagetag) {
            lock (_pagetags) {
                PageTag found;
                if (_pagetags.TryGetValue(pagetag.Key, out found)) {
                    _pagetags[pagetag.Key] = PageTag.Choose(found, pagetag);
                } else {
                    _pagetags[pagetag.Key] = pagetag;
                }
            }
        }

        /// <summary>
        /// Parse a collection of tag names into <see cref="PageTag"/> instances.
        /// </summary>
        /// <param name="tagnames">
        ///     Collection of plain text tag names. No HTML markup allowed.
        /// </param>
        /// <param name="format">The tag formatting to apply.</param>
        public static IEnumerable<PageTag> Parse(IEnumerable<string> tagnames, TagFormat format) {
            switch (format) {
                case TagFormat.AsEntered:
                    foreach (var tagname in tagnames) {
                        yield return new PageTag(tagname, PageTagType.Unknown);
                    }
                    break;
                case TagFormat.Capitalized:
                    foreach (var tagname in tagnames) {
                        var pt = new PageTag(tagname, PageTagType.Unknown);
                        if (pt.TagType == PageTagType.PlainTag) {
                            pt = new PageTag(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(pt.BaseName), PageTagType.PlainTag);
                        }
                        yield return pt;
                    }
                    break;
                case TagFormat.HashTag:
                    foreach (var tagname in tagnames) {
                        var pt = new PageTag(tagname, PageTagType.Unknown);
                        if (pt.TagType == PageTagType.PlainTag) {
                            // switch over to hashtag
                            pt.TagType = PageTagType.HashTag;
                        }
                       yield return pt;
                    }
                    break;
            }
        }

        /// <summary>
        /// Parse a collection of tag names into <see cref="PageTag"/> instances.
        /// </summary>
        /// <param name="taglist">
        ///     Comma separated list of plain text tag names. No HTML markup allowed.
        /// </param>
        /// <param name="format">The tag formatting to apply.</param>
        public static IEnumerable<PageTag> Parse(string taglist, TagFormat format) {
            return Parse(SplitTaglist(taglist), format);
        }

        /// <summary>
        /// Remove a page tag from the set.
        /// </summary>
        /// <param name="key">The page tag key</param>
        /// <returns>
        ///     The removed page tag. `null` if no tag with the given key was in
        ///     the set.
        /// </returns>
        public PageTag Remove(string key) {
            lock (_pagetags) {
                PageTag found;
                if (_pagetags.TryGetValue(key, out found)) {
                    _pagetags.Remove(key);
                }
                return found;
            }
        }

        /// <summary>
        ///     Determine if this set contains a <see cref="PageTag"/> object
        ///     with the given key.
        /// </summary>
        /// <param name="key">
        ///     A page tag key
        /// </param>
        /// <returns>
        ///     `true` if the set contains a page tag with the specified key;
        ///     `false` otherwise.
        /// </returns>
        public bool ContainsKey(string key) => _pagetags.ContainsKey(key);

        /// <summary>
        ///     Determine if this set contains a <see cref="PageTag"/> object
        ///     with the same key.
        /// </summary>
        /// <param name="tag">
        ///     A page tag.
        /// </param>
        /// <returns>
        ///     `true` if the set contains a page tag with the same key as the
        ///     given ta.;
        ///     `false` otherwise.
        /// </returns>
        public bool Contains(PageTag tag) => ContainsKey(tag.Key);

        /// <summary>
        /// Compute the set union of the tags in this set with a specified
        /// collection of page tags.
        /// </summary>
        /// <remarks>
        ///     The content of this set is modified to represent the result of
        ///     the set union.
        /// </remarks>
        /// <param name="pagetags">A collection of page tags.</param>
        public void UnionWith(IEnumerable<PageTag> pagetags) {
            lock (_pagetags) {
                foreach (var t in pagetags) { Add(t); }
            }
        }

        /// <summary>
        /// Remove all page tags from this set which are also specified
        /// in a specified page tag collection.
        /// </summary>
        /// <param name="pagetags">Collection of page tags.</param>
        public void ExceptWith(IEnumerable<PageTag> pagetags) {
            lock (_pagetags) {
                foreach (var t in pagetags) { _ = Remove(t.Key); }
            }
        }

        /// <summary>
        /// Reset to a given collection of page tags.
        /// </summary>
        /// <param name="pagetags">Collection of page tags.</param>
        public void Reset(IEnumerable<PageTag> pagetags) {
            lock (_pagetags) {
                _pagetags.Clear();
                foreach (var t in pagetags) { Add(t); }
            }
        }

        /// <summary>
        /// Predicate to determine if the set is empty.
        /// </summary>
        public bool IsEmpty => _pagetags.Count == 0;

        /// <summary>
        /// Get the number of tags in the set.
        /// </summary>
        public int Count => _pagetags.Count;

        /// <summary>
        /// Get the tag set as comma separated list.
        /// </summary>
        /// <param name="separator">
        ///     Optional separator string to separate the individual tags with.
        ///     Defaults to comma ','.
        /// </param>
        /// <returns>List of tags.</returns>
        public string ToString(string separator = ",") => string.Join(separator, from p in this
                                                                                 orderby p.Key ascending
                                                                                 select p.DisplayName);
        #region IEnumerable<PageTag>
        /// <summary>
        /// Get the typed page tag enumerator
        /// </summary>
        /// <returns>The typed page tag enumerator</returns>
        public IEnumerator<PageTag> GetEnumerator() => _pagetags.Values.GetEnumerator();

        /// <summary>
        /// Get generic page tag enumerator
        /// </summary>
        /// <returns>The generic page tag enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator() => _pagetags.Values.GetEnumerator();
        #endregion IEnumerable<PageTag>
    }
}
