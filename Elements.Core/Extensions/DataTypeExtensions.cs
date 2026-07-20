using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public static class DataTypeExtensions
    {
        public static void SetBits(this ref byte data, params bool[] bits)
        {
            if(bits.Length > 8)
                throw new ArgumentException("Byte only has 8 bits");

            for (int i = 0; i < bits.Length; i++)
                data.SetBit(i, bits[i]);
        }

        public static void SetBit(this ref byte data, int index, bool set)
        {
            if (index >= 8)
                throw new ArgumentException("Byte only has 8 bits");

            if (set)
                data = (byte)(data | (1 << index));
            else
                data = (byte)(data & ~(1 << index));
        }

        public static bool GetBit(this byte data, int index)
        {
            if (index >= 8)
                throw new ArgumentException("Byte only has 8 bits");

            return (data & (1 << index)) != 0;
        }

        public static string ToMillisecondTimeString(this DateTime datetime)
        {
            return datetime.ToLongTimeString() + "." + datetime.Millisecond.ToString("D3");
        }
    }
}
