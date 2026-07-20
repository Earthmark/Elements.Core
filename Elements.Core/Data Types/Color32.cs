using Newtonsoft.Json;
using Renderite.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace Elements.Core
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public readonly struct color32 : IEquatable<color32>
    {
        public const float BYTE_TO_FLOAT = 1f / 255f;

        public readonly byte r, g, b, a;

        [JsonPropertyName("r")]
        [JsonProperty(PropertyName = "r")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly byte R => r;

        [JsonPropertyName("g")]
        [JsonProperty(PropertyName = "g")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly byte G => g;

        [JsonPropertyName("b")]
        [JsonProperty(PropertyName = "b")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly byte B => b;

        [JsonPropertyName("a")]
        [JsonProperty(PropertyName = "a")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public readonly byte A => a;

        [System.Text.Json.Serialization.JsonConstructor]
        [Newtonsoft.Json.JsonConstructor]
        public color32(byte r, byte g, byte b, byte a = 255)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public color32(byte gray, byte a = 255)
        {
            this.r = gray;
            this.g = gray;
            this.b = gray;
            this.a = a;
        }

        public color32(in color c)
        {
            this.r = ToByte(c.r);
            this.g = ToByte(c.g);
            this.b = ToByte(c.b);
            this.a = ToByte(c.a);
        }

        public readonly byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                switch (index)
                {
                    case 0: return r;
                    case 1: return g;
                    case 2: return b;
                    case 3: return a;
                    default: throw new IndexOutOfRangeException("Invalid color channel index!");
                }
            }
        }

        public readonly byte this[ColorChannel channel]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                switch (channel)
                {
                    case ColorChannel.R: return r;
                    case ColorChannel.G: return g;
                    case ColorChannel.B: return b;
                    case ColorChannel.A: return a;
                    default: throw new IndexOutOfRangeException("Invalid color channel index!");
                }
            }
        }

        // Comparisons
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in color32 a, in color32 b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in color32 a, in color32 b)
        {
            return a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(color32 other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is color32)
                return Equals((color32)obj);

            return false;
        }

        // Conversions
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator color(in color32 c)
        {
            return new color(c.r * BYTE_TO_FLOAT, c.g * BYTE_TO_FLOAT, c.b * BYTE_TO_FLOAT, c.a * BYTE_TO_FLOAT);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // Explicit, because it loses precision
        public static explicit operator color32(in color c) => new color32(c);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator byte4(in color32 c) => new byte4(c.r, c.g, c.b, c.a);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator color32(in byte4 c) => new color32(c.x, c.y, c.z, c.w);

        // Helpers

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ToByte(float value)
        {
            value *= 255;
            value += 0.5f;

            if (value <= 0)
                return 0;
            else if (value >= 255)
                return 255;
            else
                return (byte)value;
        }

        public readonly string ToHexString(bool alpha = false, string prefix = "#")
        {
            return prefix + r.ToString("X2") + g.ToString("X2") + b.ToString("X2") + (alpha ? a.ToString("X2") : "");
        }

        public static bool FromHexCode(string hex, out color32 color, byte defaultAlpha = 255)
            => FromHexCode(hex, out color, out _, defaultAlpha);

        public static bool FromHexCode(string hex, out color32 color, out bool hasAlpha, byte defaultAlpha = 255)
            => FromHexCode(hex.AsSpan(), out color, out hasAlpha, defaultAlpha);

        public static bool FromHexCode(ReadOnlySpan<char> hex, out color32 color, out bool hasAlpha, byte defaultAlpha = 255)
        {
            hex = hex.Trim();

            if (hex.IsEmpty)
            {
                color = new color32(0, 0, 0, 0);
                hasAlpha = false;

                return false;
            }

            if (hex[0] == '#')
                hex = hex.Slice(1);

            byte r, g, b, a;

            bool success = true;

            if (hex.Length == 3 || hex.Length == 4)
            {
                success &= TryParseHex(hex.Slice(0, 1), out r);
                success &= TryParseHex(hex.Slice(1, 1), out g);
                success &= TryParseHex(hex.Slice(2, 1), out b);

                if (hex.Length == 4)
                {
                    success &= TryParseHex(hex.Slice(3, 1), out a);
                    a *= 0x11;

                    hasAlpha = true;
                }
                else
                {
                    a = defaultAlpha;

                    hasAlpha = false;
                }

                r *= 0x11;
                g *= 0x11;
                b *= 0x11;

                color = new color32(r, g, b, a);

                return success;
            }
            else if (hex.Length == 6 || hex.Length == 8)
            {
                success &= TryParseHex(hex.Slice(0, 2), out r);
                success &= TryParseHex(hex.Slice(2, 2), out g);
                success &= TryParseHex(hex.Slice(4, 2), out b);

                if (hex.Length == 8)
                {
                    success &= TryParseHex(hex.Slice(6, 2), out a);

                    hasAlpha = true;
                }
                else
                {
                    a = defaultAlpha;

                    hasAlpha = false;
                }

                color = new color32(r, g, b, a);

                return success;
            }

            color = new color32(0, 0, 0, 0);
            hasAlpha = false;

            return false;
        }

        public static bool TryParseHex(ReadOnlySpan<char> hex, out byte value)
        {
            value = 0;

            for(int i = 0; i < hex.Length; i++)
            {
                value <<= 4;

                if (HexValue(hex[i], out byte charValue))
                    value |= charValue;
                else
                    return false;
            }

            return true;
        }

        static bool HexValue(char ch, out byte value)
        {
            if (ch >= '0' && ch <= '9')
            {
                value = (byte)(ch - '0');
                return true;
            }

            if(ch >= 'a' && ch <= 'f')
            {
                value = (byte)((ch - 'a') + 0xA);
                return true;
            }

            if(ch >= 'A' && ch <= 'F')
            {
                value = (byte)((ch - 'A') + 0xA);
                return true;
            }

            value = 0;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly color32 Mask(bool4 mask) => Mask(mask, default);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly color32 Mask(bool4 mask, in color32 masked) => new color32(mask.x ? r : masked.r, mask.y ? g : masked.g, mask.z ? b : masked.b, mask.w ? a : masked.a);

        public override string ToString() => $"[{r}, {g}, {b}, {a}]";
        public string ToString(IFormatProvider provider) => $"[{r.ToString(provider)}, {g.ToString(provider)}, {b.ToString(provider)}, {a.ToString(provider)}]";
        public string ToString(string format, IFormatProvider provider) =>
            $"[{r.ToString(format, provider)}, {g.ToString(format, provider)}, {b.ToString(format, provider)}, {a.ToString(format, provider)}]";

        public override int GetHashCode()
        {
            var hashCode = -490236692;
            hashCode = hashCode * -1521134295 + r.GetHashCode();
            hashCode = hashCode * -1521134295 + g.GetHashCode();
            hashCode = hashCode * -1521134295 + b.GetHashCode();
            hashCode = hashCode * -1521134295 + a.GetHashCode();
            return hashCode;
        }

        #region PARSING

        public static color32 Parse(string s) => Parse(s, NumberStyles.Integer);

        public static color32 Parse(string s, IFormatProvider provider) => Parse(s, NumberStyles.Integer, provider);

        public static color32 Parse(string s, NumberStyles style) => Parse(s, style, CultureInfo.InvariantCulture);

        public static color32 Parse(string s, NumberStyles style, IFormatProvider provider)
        {
            var nums = PrimitivesUtility.ExtractElements(s, 4);

            if (nums == null)
                throw new ArgumentException("Could not extract elements");

            return new color32(byte.Parse(nums[0], style, provider),
                byte.Parse(nums[1], style, provider),
                byte.Parse(nums[2], style, provider),
                byte.Parse(nums[3], style, provider));
        }

        public static bool TryParse(string s, out color32 val) => TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out val);

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out color32 val)
        {
            var nums = PrimitivesUtility.ExtractElements(s, 4);

            if (nums == null)
            {
                val = default;
                return false;
            }

            var parsed_x = byte.TryParse(nums[0], style, provider, out byte x);
            var parsed_y = byte.TryParse(nums[1], style, provider, out byte y);
            var parsed_z = byte.TryParse(nums[2], style, provider, out byte z);
            var parsed_w = byte.TryParse(nums[3], style, provider, out byte w);

            if (parsed_x && parsed_y && parsed_z && parsed_w)
            {
                val = new color32(x, y, z, w);
                return true;
            }

            val = default;
            return false;
        }

        #endregion

        #region OPERATORS

        public static bool4 operator >(in color32 a, in color32 b)
        {
            return new bool4(a.r > b.r, a.g > b.g, a.b > b.b, a.a > b.a);
        }
        public static bool4 operator <(in color32 a, in color32 b)
        {
            return new bool4(a.r < b.r, a.g < b.g, a.b < b.b, a.a < b.a);
        }
        public static bool4 operator >=(in color32 a, in color32 b)
        {
            return new bool4(a.r >= b.r, a.g >= b.g, a.b >= b.b, a.a >= b.a);
        }
        public static bool4 operator <=(in color32 a, in color32 b)
        {
            return new bool4(a.r <= b.r, a.g <= b.g, a.b <= b.b, a.a <= b.a);
        }

        // Color with scalar

        public static byte ClampToByte(int value)
        {
            if (value < 0)
                return 0;

            if (value > byte.MaxValue)
                return byte.MaxValue;

            return (byte) value;
        }

        public static byte ClampToByte(float value) => ClampToByte(MathX.RoundToInt(value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator +(in color32 a, byte n)
        {
            return new color32(ClampToByte(a.r + n), ClampToByte(a.g + n), ClampToByte(a.b + n), ClampToByte(a.a + n));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator +(in color32 a, float n)
        {
            return new color32(ClampToByte(a.r + n), ClampToByte(a.g + n), ClampToByte(a.b + n), ClampToByte(a.a + n));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator -(in color32 a, byte n)
        {
            return new color32(ClampToByte(a.r - n), ClampToByte(a.g - n), ClampToByte(a.b - n), ClampToByte(a.a - n));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator +(byte n, in color32 b)
        {
            return new color32(ClampToByte(b.r + n), ClampToByte(b.g + n), ClampToByte(b.b + n), ClampToByte(b.a + n));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator -(byte n, in color32 b)
        {
            return new color32(ClampToByte(n - b.r), ClampToByte(n - b.g), ClampToByte(n - b.b), ClampToByte(n - b.a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator *(byte n, in color32 a)
        {
            return a * n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator *(float n, in color32 a)
        {
            return a * n;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator *(in color32 a, byte n)
        {
            return new color32(ClampToByte(a.r * n), ClampToByte(a.g * n), ClampToByte(a.b * n), ClampToByte(a.a * n));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator *(in color32 a, float n)
        {
            return new color32(ClampToByte(a.r * n), ClampToByte(a.g * n), ClampToByte(a.b * n), ClampToByte(a.a * n));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator /(in color32 a, byte n)
        {
            return new color32(ClampToByte(a.r / n), ClampToByte(a.g / n), ClampToByte(a.b / n), ClampToByte(a.a / n));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator %(in color32 a, byte n)
        {
            return new color32(ClampToByte(a.r % n), ClampToByte(a.g % n), ClampToByte(a.b % n), ClampToByte(a.a % n));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator /(byte n, in color32 b)
        {
            return new color32(ClampToByte(n / b.r), ClampToByte(n / b.g), ClampToByte(n / b.b), ClampToByte(n / b.a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator %(byte n, in color32 b)
        {
            return new color32(ClampToByte(n % b.r), ClampToByte(n % b.g), ClampToByte(n % b.b), ClampToByte(n % b.a));
        }

        // Color with color32

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator +(in color32 a, in color32 b)
        {
            return new color32(ClampToByte(a.r + b.r), ClampToByte(a.g + b.g), ClampToByte(a.b + b.b), ClampToByte(a.a + b.a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator -(in color32 a, in color32 b)
        {
            return new color32(ClampToByte(a.r - b.r), ClampToByte(a.g - b.g), ClampToByte(a.b - b.b), ClampToByte(a.a - b.a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator *(in color32 a, in color32 b)
        {
            return new color32(ClampToByte(a.r * b.r), ClampToByte(a.g * b.g), ClampToByte(a.b * b.b), ClampToByte(a.a * b.a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator /(in color32 a, in color32 b)
        {
            return new color32(ClampToByte(a.r / b.r), ClampToByte(a.g / b.g), ClampToByte(a.b / b.b), ClampToByte(a.a / b.a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 operator %(in color32 a, in color32 b)
        {
            return new color32(ClampToByte(a.r % b.r), ClampToByte(a.g % b.g), ClampToByte(a.b % b.b), ClampToByte(a.a % b.a));
        }

        #endregion

        #region COLORS

        public static color32 Clear => new color32(0, 0, 0, 0);

        public static color32 White => new color32(255);
        public static color32 LightGray => new color32(192);
        public static color32 Gray => new color32(128);
        public static color32 DarkGray => new color32(64);
        public static color32 Black => new color32(0);

        public static color32 Red => new color32(255, 0, 0);
        public static color32 Green => new color32(0, 255, 0);
        public static color32 Blue => new color32(0, 0, 255);

        public static color32 Yellow => new color32(255, 255, 0);
        public static color32 Cyan => new color32(0, 255, 255);
        public static color32 Magenta => new color32(255, 0, 255);

        // Extra color32s
        public static color32 Orange => new color32(1, 128, 0);
        public static color32 Purple => new color32(128, 0, 1);
        public static color32 Lime => new color32(192, 1, 0);
        public static color32 Azure => new color32(0, 128, 255);
        public static color32 Pink => new color32(255, 0, 128);
        public static color32 Brown => new color32(64, 0, 0);

        public static color32 MaxValue => new color32(255, 255);
        public static color32 MinValue => new color32(0, 0);

        #endregion
    }

    public static partial class MathX
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 Lerp(in color32 a, in color32 b, float lerp)
        {
            if (lerp <= 0)
                return a;

            if (lerp >= 1f)
                return b;

            return LerpUnclamped(a, b, lerp);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 LerpUnclamped(in color32 a, in color32 b, float lerp)
        {
            float ak = 1 - lerp;
            float bk = lerp;

            return new color32(
                color32.ClampToByte(a.r * ak + b.r * bk),
                color32.ClampToByte(a.g * ak + b.g * bk),
                color32.ClampToByte(a.b * ak + b.b * bk),
                color32.ClampToByte(a.a * ak + b.a * bk));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 Min(in color32 a, in color32 b)
        {
            return new color32(
                MathX.Min(a.r, b.r),
                MathX.Min(a.g, b.g),
                MathX.Min(a.b, b.b),
                MathX.Min(a.a, b.a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 Max(in color32 a, in color32 b)
        {
            return new color32(
                MathX.Max(a.r, b.r),
                MathX.Max(a.g, b.g),
                MathX.Max(a.b, b.b),
                MathX.Max(a.a, b.a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 Repeat(in color32 c, in color32 length)
        {
            return new color32(
                (byte)MathX.Repeat(c.r, length.r),
                (byte)MathX.Repeat(c.g, length.g),
                (byte)MathX.Repeat(c.b, length.b),
                (byte)MathX.Repeat(c.a, length.a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 Clamp(in color32 c, in color32 min, in color32 max)
        {
            return new color32(
                (byte)MathX.Clamp(c.r, min.r, max.r),
                (byte)MathX.Clamp(c.g, min.g, max.g),
                (byte)MathX.Clamp(c.b, min.b, max.b),
                (byte)MathX.Clamp(c.a, min.a, max.a));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static color32 Clamp(in color32 c, in byte min, in byte max)
        {
            return new color32(
                (byte)MathX.Clamp(c.r, min, max),
                (byte)MathX.Clamp(c.g, min, max),
                (byte)MathX.Clamp(c.b, min, max),
                (byte)MathX.Clamp(c.a, min, max));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanDivide(in color32 dividend, in color32 divisor) =>
            MathX.CanDivide(dividend.r, divisor.r) &&
            MathX.CanDivide(dividend.g, divisor.g) &&
            MathX.CanDivide(dividend.b, divisor.b) &&
            MathX.CanDivide(dividend.a, divisor.a);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanDivideBy(in color32 divisor) =>
            MathX.CanDivideBy(divisor.r) &&
            MathX.CanDivideBy(divisor.g) &&
            MathX.CanDivideBy(divisor.b) &&
            MathX.CanDivideBy(divisor.a);

        public static color32 BezierCurve(in color32 from, in color32 to, in color32 fromTangent, in color32 toTangent, float t)
        {
            var c = (color)MathX.BezierCurve((color)from, (color)to, (color)fromTangent, (color)toTangent, t);
            return (color32)c;
        }
    }

    public static class Color32Extensions
    {
        public static void Write(this BinaryWriter bw, in color32 c)
        {
            for (int i = 0; i < 4; i++)
                bw.Write(c[i]);
        }

        public static color32 ReadColor32(this BinaryReader br)
        {
            return new color32(
                br.ReadByte(),
                br.ReadByte(),
                br.ReadByte(),
                br.ReadByte());
        }
    }
}
