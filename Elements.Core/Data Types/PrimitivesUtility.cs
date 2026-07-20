using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public static class PrimitivesUtility
    {
        public static string[] ExtractElements(string s, int requiredCount)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;

            int start = s.IndexOf('[');
            int end = s.IndexOf(']');

            if (start < 0 || end < 0)
                return null;

            // extract substring
            s = s.Substring(start + 1, end - start - 1);

            // split into subelements
            var substrings = s.Split(';');

            if (substrings.Length != requiredCount)
                return null;

            return substrings;
        }

        public static string BitsToString(this ulong value, int bits, string one = "1", string zero = "0")
        {
            StringBuilder str = new StringBuilder(bits);

            ulong mask = 1UL << (bits-1);
            for (int i = 0; i < bits; i++)
            {
                str.Append((value & mask) != 0 ? one : zero);
                mask >>= 1;
            }

            return str.ToString();
        }
    }
}
