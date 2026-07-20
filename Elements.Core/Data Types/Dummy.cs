using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public readonly struct dummy : IEquatable<dummy>, IConvertible, IComparable<dummy>
    {
        public override bool Equals(object obj) => obj is dummy;

        public override string ToString() => "dummy";

        public bool Equals(dummy other) => true;

        public override int GetHashCode() => 0;

        public TypeCode GetTypeCode() => TypeCode.Empty;
        public bool ToBoolean(IFormatProvider provider) => default;
        public char ToChar(IFormatProvider provider) => default;
        public sbyte ToSByte(IFormatProvider provider) => default;
        public byte ToByte(IFormatProvider provider) => default;
        public short ToInt16(IFormatProvider provider) => default;
        public ushort ToUInt16(IFormatProvider provider) => default;
        public int ToInt32(IFormatProvider provider) => default;
        public uint ToUInt32(IFormatProvider provider) => default;
        public long ToInt64(IFormatProvider provider) => default;
        public ulong ToUInt64(IFormatProvider provider) => default;
        public float ToSingle(IFormatProvider provider) => default;
        public double ToDouble(IFormatProvider provider) => default;
        public decimal ToDecimal(IFormatProvider provider) => default;
        public DateTime ToDateTime(IFormatProvider provider) => default;
        public string ToString(IFormatProvider provider) => ToString();
        public object ToType(Type conversionType, IFormatProvider provider) => conversionType.GetDefaultValue();
        public int CompareTo(dummy other) => 0;
    }

    public readonly struct dummy<T> : IEquatable<dummy<T>>, IConvertible, IComparable<dummy<T>>
    {
        public override bool Equals(object obj) => obj is dummy<T>;

        public override string ToString() => $"dummy<{typeof(T)}>";

        public override int GetHashCode() => 0;

        public bool Equals(dummy<T> other) => true;

        public TypeCode GetTypeCode() => TypeCode.Empty;
        public bool ToBoolean(IFormatProvider provider) => default;
        public char ToChar(IFormatProvider provider) => default;
        public sbyte ToSByte(IFormatProvider provider) => default;
        public byte ToByte(IFormatProvider provider) => default;
        public short ToInt16(IFormatProvider provider) => default;
        public ushort ToUInt16(IFormatProvider provider) => default;
        public int ToInt32(IFormatProvider provider) => default;
        public uint ToUInt32(IFormatProvider provider) => default;
        public long ToInt64(IFormatProvider provider) => default;
        public ulong ToUInt64(IFormatProvider provider) => default;
        public float ToSingle(IFormatProvider provider) => default;
        public double ToDouble(IFormatProvider provider) => default;
        public decimal ToDecimal(IFormatProvider provider) => default;
        public DateTime ToDateTime(IFormatProvider provider) => default;
        public string ToString(IFormatProvider provider) => ToString();
        public object ToType(Type conversionType, IFormatProvider provider) => conversionType.GetDefaultValue();
        public int CompareTo(dummy<T> other) => 0;
    }
}
