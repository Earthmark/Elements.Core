using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public static class SpanExtensions
    {
        public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> str)
        {
            while (str.Length > 0 && char.IsWhiteSpace(str[0]))
                str = str.Slice(1);

            return str;
        }

        public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> str)
        {
            while (str.Length > 0 && char.IsWhiteSpace(str[str.Length-1]))
                str = str.Slice(0, str.Length-1);

            return str;
        }

        public static ReadOnlySpan<char> Trim(this ReadOnlySpan<char> str)
        {
            str = str.TrimStart();
            str = str.TrimEnd();

            return str;
        }
    }
}
