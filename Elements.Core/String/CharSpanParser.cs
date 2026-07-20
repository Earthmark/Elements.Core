using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public static class CharSpanParser
    {
        public static bool TryParseInteger(ReadOnlySpan<char> parameter, out int value)
        {
            bool started = false;

            value = 0;

            int number = 0;

            for (int i = 0; i < parameter.Length; i++)
            {
                var ch = parameter[i];

                if (char.IsWhiteSpace(ch) && !started)
                    continue;

                if (!char.IsDigit(ch))
                    return false;

                int digit = (ch - '0');

                number *= 10;
                number += digit;
            }

            value = number;

            return true;
        }
    }
}
