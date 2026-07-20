using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public static class PrimitiveCodeHelper
    {
        public static string ToCodeLiteral(this bool value) => value ? "true" : "false";
        public static string ToCodeLiteral(this byte value) => $"(byte){value}U";
        public static string ToCodeLiteral(this sbyte value) => $"(sbyte){value}";
        public static string ToCodeLiteral(this ushort value) => $"(ushort){value}U";
        public static string ToCodeLiteral(this short value) => $"(short){value}";
        public static string ToCodeLiteral(this int value) => value.ToString();
        public static string ToCodeLiteral(this uint value) => $"{value}U";
        public static string ToCodeLiteral(this long value) => $"{value}L";
        public static string ToCodeLiteral(this ulong value) => $"{value}UL";
        public static string ToCodeLiteral(this float value) => $"{value.ToString("R", CultureInfo.InvariantCulture)}f";
        public static string ToCodeLiteral(this double value) => $"{value.ToString("R", CultureInfo.InvariantCulture)}";
        public static string ToCodeLiteral(this decimal value) => $"{value.ToString("R", CultureInfo.InvariantCulture)}M";
    }
}
