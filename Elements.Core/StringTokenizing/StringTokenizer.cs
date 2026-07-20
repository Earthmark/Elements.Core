using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public static class StringTokenizer
    {
        public static StringToken Tokenize(string str, HashSet<string> allowedTags = null)
        {
            if (string.IsNullOrEmpty(str))
                return new StringContent("");

            StringToken firstToken = null;
            StringToken lastToken = null;

            int index = 0;

            while (index < str.Length)
            {
                var token = GetNextToken(str, ref index, allowedTags);

                if (firstToken == null)
                    firstToken = token;

                if (lastToken != null)
                    lastToken.Next = token;

                lastToken = token;
            }

            return firstToken;
        }

        static StringToken GetNextToken(string str, ref int index, HashSet<string> allowedTags)
        {
            StringSegmentToken segment;

            // Determine what kind of token it is
            if (IsStartingTag(str, index, allowedTags))
            {
                bool closing = str[index + 1] == '/';
                int endTagIndex = str.IndexOf('>', index);

                if (endTagIndex == -1)
                {
                    // no closing tag found, just consider it content
                    segment = new StringSegmentToken(str, index, str.Length - index);
                    index = str.Length;
                    return new StringContent(segment);
                }

                int innerStartIndex = index + (closing ? 2 : 1);
                int innerLength = endTagIndex - innerStartIndex;

                segment = new StringSegmentToken(str, innerStartIndex, innerLength);

                index = endTagIndex + 1;

                if (closing)
                    return new StringClosingTag(segment);
                else
                {
                    // check if the segment contains param info
                    int equalsIndex = segment.IndexOf('=');

                    if (equalsIndex < 0)
                        return new StringOpeningTag(segment);

                    var spaceIndex = segment.IndexOf(' ');
                    var tagEndIndex = equalsIndex;

                    if (spaceIndex >= 0 && spaceIndex < equalsIndex)
                        tagEndIndex = spaceIndex;

                    // has param, split
                    var tag = segment.SubSegment(0, tagEndIndex);
                    var param = segment.SubSegment(equalsIndex + 1);

                    return new StringOpeningTag(tag, param);
                }
            }
            else
            {
                // search for an opening tag
                int openTagIndex = GetNextOpenTagIndex(str, index, allowedTags);

                int contentLength = (openTagIndex < 0) ? (str.Length - index) : (openTagIndex - index);

                segment = new StringSegmentToken(str, index, contentLength);

                index += contentLength;

                return new StringContent(segment);
            }
        }

        static int GetNextOpenTagIndex(string str, int index, HashSet<string> allowedTags)
        {
            int openTagIndex = index - 1;

            do
            {
                openTagIndex = str.IndexOf('<', openTagIndex + 1);

                if (openTagIndex < 0)
                    return -1;

            } while (!IsStartingTag(str, openTagIndex, allowedTags));

            return openTagIndex;
        }

        static bool IsStartingTag(string str, int index, HashSet<string> allowedTags)
        {
            if (str[index] != '<')
                return false;

            var endTagIndex = str.IndexOf('>', index);

            if (endTagIndex == -1)
                return false;

            if (allowedTags == null)
                return true;

            // check if it's allowed tag
            var paramsIndex = str.IndexOf('=', index);

            if (paramsIndex >= 0 && paramsIndex < endTagIndex)
                endTagIndex = paramsIndex;

            var spaceIndex = str.IndexOf(' ', index);

            if (spaceIndex >= 0 && spaceIndex < endTagIndex)
                endTagIndex = spaceIndex;

            index++;

            if (str[index] == '/')
                index++;

            var tag = str.Substring(index, endTagIndex - index);

            return allowedTags.Contains(tag);
        }
    }
}
