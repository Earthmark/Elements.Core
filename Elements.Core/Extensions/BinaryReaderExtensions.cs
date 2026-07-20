using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Elements.Core
{
    public static class BinaryReaderExtensions
    {
        public static E ReadEnumBinary<E>(this BinaryReader br)
        {
            if (!typeof(E).IsEnum)
                throw new Exception("Argument must be an enumeration!");

            // read the ulong
            ulong val = br.Read7BitEncoded();

            return EnumUtil.UInt64ToEnum<E>(val);
        }

        public static E ReadEnumString<E>(this BinaryReader br)
            where E : struct
        {
            if (!typeof(E).IsEnum)
                throw new Exception("Argument must be an enumeration!");

            // read the ulong
            string str = br.ReadString();

            if (!Enum.TryParse<E>(str, out E val))
                throw new Exception("Couldn't parse enum value: " + str);

            return val;
        }

        public static string ReadNullableString(this BinaryReader br)
        {
            var isNull = !br.ReadBoolean();

            if (isNull)
                return null;
            else
                return br.ReadString();
        }

        public static DateTime ReadDateTime(this BinaryReader br) => DateTime.FromBinary(br.ReadInt64());

        public static TimeSpan ReadTimeSpan(this BinaryReader br) => new TimeSpan(br.ReadInt64());

        public static Uri ReadUri(this BinaryReader br)
        {
            var dataType = br.ReadEnumBinary<UriData>();

            switch(dataType)
            {
                case UriData.Null:
                    return null;

                case UriData.Absolute:
                    Uri.TryCreate(br.ReadString(), UriKind.Absolute, out var absolute);
                    return absolute;

                case UriData.Relative:
                    Uri.TryCreate(br.ReadString(), UriKind.Relative, out var relative);
                    return relative;

                default:
                    throw new Exception("invalid Uri data type: " + dataType);
            }
        }

        // compacted read
        public static ulong Read7BitEncoded(this BinaryReader br)
        {
            bool readAnother;

            ulong integer = 0;
            int shift = 0;

            do
            {
                byte data = br.ReadByte();

                // check if there's another first
                readAnother = (data & 0x80) != 0;

                integer |= ((ulong)(data & 0x7F)) << shift;

                // increment for the next round (if there is one)
                shift += 7;

            } while (readAnother);

            return integer;
        }

        public static byte[] ReadSequence(this BinaryReader br, byte?[] sequence)
        {
            byte[] readSequence = new byte[sequence.Length];

            int index = 0;
            byte readByte;

            while(br.BaseStream.Position < br.BaseStream.Length)
            {
                readByte = br.ReadByte();

                if (sequence[index] == null || sequence[index].Value == readByte)
                {
                    readSequence[index] = readByte;
                    index++;
                }
                else
                    index = 0; // start over

                if (index == sequence.Length)
                    return readSequence;
            }

            return null;
        }
    }
}
