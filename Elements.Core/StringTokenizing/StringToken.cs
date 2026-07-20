using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public abstract class StringToken : IEnumerable<StringToken>
    {
        public StringToken Next;

        // Appends self to the string builder
        public abstract void AppendTo(StringBuilder builder);

        public string GetRawString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var content in EnumerateStringContents())
                content.AppendTo(builder);

            return builder.ToString();
        }

        public string GetString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var token in this)
                token.AppendTo(builder);

            return builder.ToString();
        }

        public string Substring(int startindex)
        {
            int rawLength = GetRawLength();

            return Substring(startindex, rawLength - startindex);
        }

        public string Substring(int startindex, int length)
        {
            var tagTracker = new TagTracker();
            var builder = new StringBuilder();

            int index = 0;
            bool writing = false;

            foreach (var token in this)
            {
                tagTracker.Update(token);

                var content = token as StringContent;

                if (content != null)
                {
                    if (!writing)
                    {
                        // check if it should start writing on this segment
                        if (index + content.Segment.Length > startindex)
                        {
                            writing = true;
                            // first write all currently open tags, preserving formatting
                            tagTracker.AppendAllOpenTags(builder);
                            // compute how many to skip from the first found segment
                            int offset = startindex - index;
                            // create a subsegment, skipping the extra bits before the startindex
                            content = new StringContent(content.Segment.SubSegment(offset));
                        }

                        index += content.Segment.Length;
                    }

                    // Check the condition again, because it might've started writing
                    if (writing)
                    {
                        var segment = content.Segment;
                        // trim the segment to the maximum length in case it goes over
                        // this does nothing (except a few checks) if whole segment can be used
                        segment = segment.SubSegment(0, MathX.Min(segment.Length, length));

                        // append it to the builder
                        builder.Append(segment);

                        length -= segment.Length;

                        if (length == 0)
                            break;
                    }
                }
                else if (writing)
                    token.AppendTo(builder);
            }

            // Finished, if there are any open tags, close them all
            tagTracker.CloseAllTagsAndAppend(builder);

            return builder.ToString();
        }

        public void SpliceAfter(StringToken token)
        {
            // Place this token right after this segment and redirect the end of the chain
            // To the original successive token
            var prevNext = Next;
            Next = token;

            if (prevNext != null)
                token.Last().Next = prevNext;
        }

        public bool SpliceAt(StringToken token, int position)
        {
            int searchPosition = 0;

            // Mark the whole chain as spliced and also find the last in the chain
            StringToken lastToken = null;
            foreach (var splicedToken in token)
            {
                var content = splicedToken as StringContent;
                content.IsSpliced = true;
                lastToken = splicedToken;
            }

            // Search for the splice point
            foreach (var searchToken in this)
            {
                var content = searchToken as StringContent;

                // Skip tokens that aren't contents or have been already spliced from the search
                if (content == null || content.IsSpliced)
                    continue;

                // Check if the splice point is within this content token or right behind it
                if (searchPosition + content.Segment.Length >= position)
                {
                    // It is within, splice it in
                    position -= searchPosition; // offset the position so it is now relative to the content segment

                    if (position < content.Segment.Length)
                    {
                        // Need to split the content token in two and splice between them
                        var before = content.Segment.SubSegment(0, position);
                        var after = content.Segment.SubSegment(position, -1);

                        // Just replace the segment for the found token
                        content.Segment = before;
                        // Splice the new token chain after the shortened content token
                        content.SpliceAfter(token);
                        // Splice the second part of the split content token after the last chain of the
                        // newly spliced chain
                        lastToken.SpliceAfter(new StringContent(after));

                        return true;
                    }
                    else
                    {
                        // Simply splice the new chain after the end of the found content token
                        content.SpliceAfter(token);

                        return true;
                    }
                }

                // just shift the search position by the segment length and continue searching
                searchPosition += content.Segment.Length;
            }

            return false; // didn't manage to splice it in, position is out of bounds
        }

        public int GetRawLength()
        {
            int length = 0;

            foreach (var content in EnumerateStringContents())
                length += content.Segment.Length;

            return length;
        }

        public IEnumerable<StringContent> EnumerateStringContents()
        {
            StringToken currentToken = this;

            while (currentToken != null)
            {
                var content = currentToken as StringContent;
                if (content != null)
                    yield return content;

                currentToken = currentToken.Next;
            }
        }

        public IEnumerator<StringToken> GetEnumerator()
        {
            StringToken currentToken = this;

            while (currentToken != null)
            {
                yield return currentToken;
                currentToken = currentToken.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return GetString();
        }
    }

    public class StringContent : StringToken
    {
        public StringSegmentToken Segment;
        // StringContent that has been spliced into some original string. It'll be skipped when finding
        // splice point for any further splices (until the flag is cleared), which allows splicing multiple
        // strings into original without having to keep track of shifted indexes
        public bool IsSpliced;

        public StringContent(StringSegmentToken content)
        {
            this.Segment = content;
        }

        public StringContent(string str)
        {
            this.Segment = new StringSegmentToken(str);
        }

        public override void AppendTo(StringBuilder builder)
        {
            builder.Append(Segment);
        }
    }

    public class StringOpeningTag : StringToken
    {
        public StringSegmentToken Tag;
        public StringSegmentToken Param;

        public StringOpeningTag(StringSegmentToken tag, StringSegmentToken param = null)
        {
            Tag = tag;
            Param = param;
        }

        public override void AppendTo(StringBuilder builder)
        {
            builder.Append('<');
            builder.Append(Tag);
            if (Param != null)
            {
                builder.Append('=');
                builder.Append(Param);
            }
            builder.Append('>');
        }
    }

    public class StringClosingTag : StringToken
    {
        public StringSegmentToken Tag;

        public StringClosingTag(StringSegmentToken tag)
        {
            Tag = tag;
        }

        public override void AppendTo(StringBuilder builder)
        {
            builder.Append("</");
            builder.Append(Tag);
            builder.Append('>');
        }
    }
}
