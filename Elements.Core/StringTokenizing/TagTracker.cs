using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public class TagTracker
    {
        List<StringOpeningTag> _openedTags = new List<StringOpeningTag>();

        public IEnumerable<StringOpeningTag> OpenedTags { get { return _openedTags; } }

        public void OpenTag(StringOpeningTag tag)
        {
            _openedTags.Add(tag);
        }

        public void CloseTag(StringClosingTag tag)
        {
            int index = _openedTags.FindLastIndex(t => t.Tag == tag.Tag);

            if (index < 0)
                throw new Exception($"Tag {tag.Tag} isn't opened!");

            _openedTags.RemoveAt(index);
        }

        public void Update(StringToken token)
        {
            var openTag = token as StringOpeningTag;
            if (openTag != null)
            {
                OpenTag(openTag);
                return;
            }

            var closeTag = token as StringClosingTag;
            if (closeTag != null)
            {
                CloseTag(closeTag);
                return;
            }
        }


        public void AppendAllOpenTags(StringBuilder builder)
        {
            foreach (var tag in OpenedTags)
                tag.AppendTo(builder);
        }

        public void CloseAllTagsAndAppend(StringBuilder builder)
        {
            // close them all in reverse direction
            for (int i = _openedTags.Count - 1; i >= 0; i--)
            {
                // make a closing tag out of the oepened one
                var closingTag = new StringClosingTag(_openedTags[i].Tag);
                closingTag.AppendTo(builder);
            }

            // they're closed now, remove
            _openedTags.Clear();
        }
    }
}
