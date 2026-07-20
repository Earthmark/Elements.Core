using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public readonly struct LocaleString : IComparable<LocaleString>, IEquatable<LocaleString>
    {
        public readonly string content;
        public readonly string format;
        public readonly bool isLocaleKey;
        public readonly bool isContinuous;
        public readonly Dictionary<string, object> arguments;

        public LocaleString(string content, string format, bool isLocaleKey, bool isContinuous, Dictionary<string, object> arguments)
        {
            this.content = content;
            this.format = format;
            this.isLocaleKey = isLocaleKey;
            this.isContinuous = isContinuous;
            this.arguments = arguments;
        }

        public LocaleString SetFormat(string format) => new LocaleString(content, format, isLocaleKey, isContinuous, arguments);

        public int CompareTo(LocaleString other) => string.Compare(content, other.content);

        public bool Equals(LocaleString other) => isLocaleKey == other.isLocaleKey && isContinuous == other.isContinuous && content == other.content && format == other.format && arguments == other.arguments;

        public override bool Equals(object obj)
        {
            if (obj is LocaleString other)
                return Equals(other);

            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 2108858224;

            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(content);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(format);
            hashCode = hashCode * -1521134295 + isLocaleKey.GetHashCode();
            hashCode = hashCode * -1521134295 + isContinuous.GetHashCode();

            if (arguments != null)
                hashCode = hashCode * -1521134295 + arguments.GetHashCode();

            return hashCode;
        }

        public override string ToString()
        {
            if (!isLocaleKey)
                return content;
            else
                return $"Key: {content}, Format: {format}, Continuous: {isContinuous}, Args: {arguments?.Count ?? 0}";
        }

        public static bool operator ==(in LocaleString left, LocaleString right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in LocaleString left, LocaleString right)
        {
            return !(left == right);
        }

        public static implicit operator LocaleString(string baseString) => new LocaleString(baseString, null, false, false, null);
    }

    public static class LocaleHelper
    {
        public static LocaleString AsLocaleKey(this string str, string argName, object argField)
        {
            return str.AsLocaleKey(null, (argName, argField));
        }

        public static LocaleString AsLocaleKey(this string str, string format, string argName, object argField)
        {
            return str.AsLocaleKey(format, (argName, argField));
        }

        public static LocaleString AsLocaleKey(this string str, params ValueTuple<string, object>[] arguments)
        {
            return str.AsLocaleKey(null, arguments);
        }

        public static LocaleString AsLocaleKey(this string str, string format, params ValueTuple<string, object>[] arguments)
        {
            var dict = new Dictionary<string, object>();

            foreach (var arg in arguments)
                dict.Add(arg.Item1, arg.Item2);

            return str.AsLocaleKey(format, true, dict);
        }

        public static LocaleString AsLocaleKey(this string str, bool continuous, Dictionary<string, object> arguments = null) => new LocaleString(str, null, true, continuous, arguments);
        public static LocaleString AsLocaleKey(this string str, string format = null, bool continuous = true, Dictionary<string, object> arguments = null) => new LocaleString(str, format, true, continuous, arguments);
    }
}
