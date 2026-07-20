using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace Elements.Core
{
    public static class StringHelper
    {
        public static string SimpleEncrypt(this string text, byte[] key, byte[] iv)
        {
            using (var algorithm = DES.Create())
            using (var transform = algorithm.CreateEncryptor(key, iv))
            {
                byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
                byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);

                return Convert.ToBase64String(outputBuffer);
            }
        }

        public static string SimpleDecrypt(this string text, byte[] key, byte[] iv)
        {
            using (var algorithm = DES.Create())
            using (var transform = algorithm.CreateDecryptor(key, iv))
            {
                byte[] inputbuffer = Convert.FromBase64String(text);
                byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
                return Encoding.Unicode.GetString(outputBuffer);
            }
        }

        public static string ClampLength(this string text, int maxLength)
        {
            if (text == null)
                return null;

            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength);
        }

        // https://stackoverflow.com/questions/50761133/how-to-check-for-invalid-utf-8-characters
        public static bool IsValidUnicode(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                var uc = char.GetUnicodeCategory(str, i);

                if (uc == UnicodeCategory.Surrogate)
                {
                    // Unpaired surrogate, like  "😵"[0] + "A" or  "😵"[1] + "A"
                    return false;
                }
                else if (uc == UnicodeCategory.OtherNotAssigned)
                {
                    // \uF000 or \U00030000
                    return false;
                }

                // Correct high-low surrogate, we must skip the low surrogate
                // (it is correct because otherwise it would have been a 
                // UnicodeCategory.Surrogate)
                if (char.IsHighSurrogate(str, i))
                {
                    i++;
                }
            }

            return true;
        }

        // Based on: http://archives.miloush.net/michkap/archive/2007/05/14/2629747.html
        public static string RemoveDiacritics(this string text)
        {
            string stFormD = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        public static string RemoveNonASCII(this string text)
        {
            var str = new StringBuilder();

            foreach (var ch in text)
                if (ch < 128)
                    str.Append(ch);

            return str.ToString();
        }

        public static string BeautifyName(this string name)
        {
            if (name == null)
                return null;

            var str = Pool.BorrowStringBuilder();

            bool lastWhitespace = true;
            bool lastUppercase = true;
            bool lastDigit = false;

            for (int i = 0; i < name.Length; i++)
            {
                bool hasNext = (i + 1) < name.Length;

                var ch = name[i];

                if (ch == '_' || ch == '-' || ch == ' ')
                {
                    str.Append(' ');

                    lastWhitespace = true;
                    lastUppercase = false;
                    lastDigit = false;

                    continue;
                }

                if (char.IsDigit(ch))
                {
                    // don't split digits
                    if (lastDigit)
                    {
                        str.Append(ch);

                        lastUppercase = false;
                        lastWhitespace = false;

                        continue;
                    }
                    else if (hasNext && char.IsUpper(name[i + 1]))
                    {
                        str.Append(' ');
                        str.Append(ch);
                        str.Append(name[i + 1]);

                        i++;

                        lastUppercase = false;
                        lastWhitespace = false;
                        lastDigit = false;

                        continue;
                    }

                    lastDigit = true;
                    lastUppercase = false;
                    lastWhitespace = false;

                    str.Append(ch);

                    continue;
                }

                bool isUpper = char.IsUpper(ch);

                if (isUpper && !lastUppercase && !lastWhitespace)
                {
                    str.Append(' ');
                    str.Append(ch);

                    lastWhitespace = false;
                    lastUppercase = true;
                    lastDigit = false;

                    continue;
                }

                if (lastWhitespace)
                    str.Append(char.ToUpper(ch));
                else
                    str.Append(ch);

                lastWhitespace = false;
                lastUppercase = isUpper;
                lastDigit = false;
            }

            var _str = str.ToString();

            Pool.Return(ref str);

            return _str.ToString();
        }

        public static Dictionary<string, string> ParseQueryString(string query)
        {
            var dict = new Dictionary<string, string>();

            StringBuilder str = new StringBuilder();
            string identifier = null;

            for (int i = 0; i < query.Length; i++)
            {
                var ch = query[i];

                if (i == 0 && ch == '?')
                    continue;

                if (identifier == null)
                {
                    if (ch == '=')
                    {
                        identifier = str.ToString();
                        str.Clear();
                    }
                    else
                        str.Append(ch);
                }
                else
                {
                    // already got identifier
                    if (ch == '&')
                    {
                        // finish the parsing
                        dict.Add(identifier, str.ToString());
                        str.Clear();
                        identifier = null;
                    }
                    else
                        str.Append(ch);
                }
            }

            if (identifier != null)
                dict.Add(identifier, str.ToString());

            return dict;
        }

        public static string ToURLBase64(byte[] data) => Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_').Replace("=", "");

        public static List<string> ParseArguments(string str)
        {
            var args = new List<string>();

            if (string.IsNullOrWhiteSpace(str))
                return args;

            var arg = new StringBuilder();

            void SubmitArg()
            {
                if (arg.Length > 0)
                {
                    if (arg.Length >= 2 && arg[0] == '"' && arg[arg.Length - 1] == '"')
                    {
                        arg.Remove(0, 1);
                        arg.Length -= 1;
                    }

                    args.Add(arg.ToString());
                    arg.Clear();
                }
            }

            for (int i = 0; i < str.Length; i++)
            {
                var ch = str[i];

                if (char.IsWhiteSpace(ch))
                {
                    if (arg.Length == 0)
                        continue;

                    if (arg.Length >= 2 && arg[0] == '"' && arg[arg.Length - 1] != '"')
                    {
                        arg.Append(ch);
                        continue;
                    }
                    else
                        SubmitArg();
                }
                else
                    arg.Append(ch);
            }

            SubmitArg();

            return args;
        }

        public static int GetNextWordBoundary(this string str, int index)
        {
            if (str == null)
                return 0;

            if (index == str.Length)
                return index;

            bool isInWord = true;

            for (; index < str.Length; index++)
            {
                var ch = str[index];

                if (char.IsLetterOrDigit(ch))
                {
                    if (isInWord)
                        continue;
                    else
                        break;
                }
                else
                    isInWord = false;
            }

            return index;
        }

        public static int GetPreviousWordBoundary(this string str, int index)
        {
            if (str == null)
                return 0;

            bool isInWord = false;

            for (; index > 0; index--)
            {
                var ch = str[index - 1];
                var isWord = char.IsLetterOrDigit(ch);

                if (!isInWord)
                {
                    if (isWord)
                        isInWord = true;

                    continue;
                }
                else if (!isWord)
                    break;
            }

            return index;
        }

        public static int GetLineStart(this string str, int index)
        {
            if (str == null)
                return 0;

            index = MathX.Min(index, str.Length - 1);

            if (index <= 0)
                return 0;

            do
            {
                index--;
            } while (index > 0 && str[index] != '\n');

            if (str[index] == '\n')
                index++;

            return index;
        }

        public static int GetNextLineStart(this string str, int index)
        {
            if (str == null)
                return 0;

            while (index < str.Length && str[index] != '\n')
                index++;

            return index + 1;
        }

        public static string Capitalize(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if (str.Length == 1)
                return str.ToUpper();

            return char.ToUpper(str[0]) + str.Substring(1);
        }

        /// <summary>
        /// Count the occurences of pattern within the string text.
        /// </summary>
        /// <param name="text">Text to search</param>
        /// <param name="pattern">Pattern to search for</param>
        /// <returns>Count of occurences of pattern within text.</returns>
        public static int CountOcurrences(string text, string pattern, StringComparison mode = StringComparison.Ordinal)
        {
            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i, mode)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }
    }
}
