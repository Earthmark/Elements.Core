using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Elements.Core
{
    public static class StringExtensions
    {
        // From here: http://stackoverflow.com/questions/6219454/efficient-way-to-remove-all-whitespace-from-string
        // Seems to be fastest method
        public static string RemoveWhitespace(this string str)
        {
            return string.Join("",
                str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        // From: https://stackoverflow.com/questions/1615559/convert-a-unicode-string-to-an-escaped-ascii-string

        public static string EscapeUnicodeCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static string UnescapeUnicodeCharacters(this string str)
        {
            return Regex.Replace(
                str,
                @"\\u(?<Value>[a-zA-Z0-9]{4})",
                m => {
                    return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
                });
        }

        public static bool StartsWith(this string str, string subString, int startIndex) => str.IndexOf(subString, startIndex) == startIndex;
    }
}
