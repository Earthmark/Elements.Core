using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Elements.Core
{
    public static class BinaryWriterExtensions
    {
        static Encoding _encoding = new UTF8Encoding(false, true);

        public static void WriteEnumBinary<E>(this BinaryWriter bw, E value)
        {
            if (!typeof(E).IsEnum)
                throw new Exception("Argument must be an enumeration!");

            bw.Write7BitEncoded(unchecked((ulong)((IConvertible)value).ToInt64(null)));
        }

        public static void WriteEnumString<E>(this BinaryWriter bw, E value)
            where E : struct
        {
            if (!typeof(E).IsEnum)
                throw new Exception("Argument must be an enumeration!");

            bw.Write(value.ToString());
        }

        public static void WriteNullable(this BinaryWriter bw, string value)
        {
            if (value == null)
                bw.Write(false);
            else
            {
                bw.Write(true);
                bw.Write(value);
            }
        }

        public static void Write(this BinaryWriter bw, DateTime value) => bw.Write(value.ToBinary());
        public static void Write(this BinaryWriter bw, TimeSpan value) => bw.Write(value.Ticks);

        public static void Write(this BinaryWriter bw, Uri value)
        {
            if (value == null)
                bw.WriteEnumBinary(UriData.Null);
            else
            {
                var str = value.OriginalString;

                if (str == null)
                    bw.WriteEnumBinary(UriData.Null);
                else
                {
                    bw.WriteEnumBinary(value.IsAbsoluteUri ? UriData.Absolute : UriData.Relative);
                    bw.Write(str);
                }
            }
        }

        // Compacted write

        public static void Write7BitEncoded(this BinaryWriter bw, ulong integer)
        {
            do
            {
                // get 7 lowest bits
                byte data = (byte)(integer & 0x7F);

                // shift it by 7 bits
                integer >>= 7;

                // check if there is more to write
                if (integer > 0)
                    data |= 0x80;   // set the MSB to 1 to indicate there's more coming

                bw.Write(data);

            } while (integer > 0);
        }

        internal struct PatternRecord
        {
            public int startIndex;
            public int endIndex;
            public bool isSequence;
        }

        public static bool IsImplicitlyWriteable(Type type)
        {
            // data primitives and few extra types are implicitly writeable
            if (type.IsPrimitive || type == typeof(string) || type == typeof(Uri))
                return true;

            // non-value types aren't implicitly writeable
            if (!type.IsValueType)
                return false;

            // It is a composed value type, check if all of its fields are implicitly writeable as well
            foreach(var f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!IsImplicitlyWriteable(f.FieldType))
                    return false;
            }

            // All fields can be implicitly written, so the whole can be as well
            return true;
        }

        // TODO!!! UNFINISHED
        public static void WriteImplicit(this BinaryWriter writer, object value, bool verify = true)
        {
            throw new NotImplementedException();

            var type = value.GetType();

            // verification is done first to prevent partially written value to corrupt the stream
            if (verify && !IsImplicitlyWriteable(type))
                throw new Exception("Object be implicitly written");

            if (type.IsPrimitive)
            {

            }
            else if (type == typeof(string))
                writer.Write((string)value);
            else if (type == typeof(Uri))
                writer.Write(((Uri)value).OriginalString);
            else
            {
                // Decompose
            }
        }
    }
}
