using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;

namespace WetHatLab.OneNote.TaggingKit.common
{

    /// <summary>
    /// Representation of a fragment of text which does or des not match a pattern
    /// </summary>
    /// <remarks>instances of this classed are used by <see cref="TextSplitter"/> objects to split
    /// text according to matching patterns</remarks>
    /// <seealso cref="TextSplitter"/>
    public struct TextFragment
    {
        /// <summary>
        /// Get the text fragment
        /// </summary>
        internal string Text { get; private set; }
        /// <summary>
        /// Determine if this fragment is a match to a pattern
        /// </summary>
        /// <value>true if the text fragment matches a pattern; false if not</value>
        internal bool IsMatch { get; private set; }

        /// <summary>
        /// create a new instance of a text fragment
        /// </summary>
        /// <param name="text">text fragment</param>
        /// <param name="ismatch">Match status. true if fragment matches a pattern; false otherwise</param>
        internal TextFragment(string text, bool ismatch)
            : this()
        {
            Text = text;
            IsMatch = ismatch;
        }
    }

    /// <summary>
    /// Split text at pattern matches.
    /// </summary>
    /// <remarks>
    /// The text is split into <see cref="TextFragment"/> sequences including all text fragments
    /// whether they match the pattern or not. See <see cref="SplitText"/> for more details
    /// </remarks>
    public class TextSplitter
    {
        private static Regex escaper = new Regex(@"([\(\)\[\]\{\}\\\.\+])", RegexOptions.Compiled);
        private Regex _pattern;

        /// <summary>
        /// Create a new text splitter instance
        /// </summary>
        /// <param name="pattern">sequence of plain text strings</param>
        /// <param name="splitOptions">regular expression match options</param>
        internal TextSplitter(IEnumerable<string> pattern, RegexOptions splitOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled)
        {
            if (pattern != null)
            {
                string rex = string.Join("|", from p in pattern select escaper.Replace(p,@"\$1"));
                if (rex.Length > 0)
                {
                    _pattern = new Regex(rex, splitOptions);
                }
            }
        }

        /// <summary>
        /// Create a new text splitter instance
        /// </summary>
        /// <param name="pattern">any number of text patterns</param>
        /// <param name="splitOptions">regular expression match options</param>
        internal TextSplitter(params string[] pattern) : this((IEnumerable<string>)pattern)
        {
        }

        /// <summary>
        /// Get the regular expression used for splitting.
        /// </summary>
        /// <value>regular expression used for splitting; null if not spit pattern is set </value>
        internal Regex SplitPattern { get { return _pattern; } }
        /// <summary>
        /// Create a sequence of text fragments describing matching and non-matching fragments
        /// </summary>
        /// <param name="text">text to split</param>
        /// <returns>sequence of text fragments</returns>
        internal IList<TextFragment> SplitText(string text)
        {
            List<TextFragment> fragments = new List<TextFragment>(10);

            if (string.IsNullOrEmpty(text))
            {
                return fragments;
            }

            MatchCollection matches;
            if (_pattern == null
                || (matches = _pattern.Matches(text)) == null
                || matches.Count == 0)
            {
                fragments.Add(new TextFragment(text, false));
                return fragments;
            }

            int afterLastHighlight = 0;
            foreach (Match m in matches)
            {
                // create a plain run between the last highlight and this highlight
                if (m.Index > afterLastHighlight)
                {
                    fragments.Add(new TextFragment(text.Substring(afterLastHighlight, m.Index - afterLastHighlight), false));
                }
                // add a matching fragment
                fragments.Add(new TextFragment(text.Substring(m.Index, m.Length), true));
                afterLastHighlight = m.Index + m.Length;
            }

            // add remaining plain text
            if (afterLastHighlight < text.Length)
            {
                fragments.Add(new TextFragment(text.Substring(afterLastHighlight), false));
            }
            return fragments;
        }
    }
    internal static class TextSplitterExtensions
    {
        internal static bool IsHighlighted(this IEnumerable<TextFragment> highlightedText)
        {
            return (from f in highlightedText where f.IsMatch select f).FirstOrDefault().IsMatch;
        }
    }
}
