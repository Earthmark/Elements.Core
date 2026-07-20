using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public static class UnitFormatting
    {
        // Helpers, TODO!!! Cleanup and move somewhere else?
        static string[] suffixes = new string[] { "B", "kB", "MB", "GB", "TB", "PB" };

        public static string FormatBytes(double bytes, int decimalPlaces = 2)
        {
            string sign = bytes < 0 ? "-" : "";
            bytes = MathX.Abs(bytes);

            foreach (var s in suffixes)
            {
                if (bytes < 1024 || s == suffixes[suffixes.Length-1])
                    return sign + bytes.ToString($"F{decimalPlaces}") + " " + s;

                bytes /= 1024;
            }

            return null;
        }
    }
}
