using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Renderite.Shared;

namespace Elements.Core
{
    public struct IntRect : IEquatable<IntRect>
    {
        public int x, y, width, height;

        public IntRect(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public IntRect(in int2 position, in int2 size)
        {
            this.x = position.x;
            this.y = position.y;
            this.width = size.x;
            this.height = size.y;
        }

        public IntRect(in int4 rect)
        {
            this.x = rect.x;
            this.y = rect.y;
            this.width = rect.z;
            this.height = rect.w;
        }

        public static IntRect FromMinMax(in int2 min, in int2 max) => new IntRect(min, max - min);
        public static IntRect Parse(string str)
        {
            if (TryParse(str, out IntRect rect))
                return rect;

            throw new FormatException("Invalid IntRect format");
        }

        public static bool TryParse(string str, out IntRect rect)
        {
            str = str.Trim();

            if(str[0] != '[' && str[str.Length-1] != ']')
            {
                rect = default;
                return false;
            }

            str = str.Substring(1, str.Length - 2);

            var parts = str.Split(new char[] { ';' } , StringSplitOptions.RemoveEmptyEntries);

            if(parts.Length != 4)
            {
                rect = default;
                return false;
            }

            int x, y, w, h;

            if (ParsePart(parts[0], "X=", out x) &&
                ParsePart(parts[1], "Y=", out y) &&
                ParsePart(parts[2], "W=", out w) &&
                ParsePart(parts[3], "H=", out h))
            {
                rect = new IntRect(x, y, w, h);
                return true;
            }

            rect = default;
            return false;
        }

        static bool ParsePart(string part, string name, out int value)
        {
            var index = part.IndexOf(name);

            if(index < 0)
            {
                value = default;
                return false;
            }

            return int.TryParse(part.Substring(index + name.Length), out value);
        }

        public int xmin
        {
            readonly get => x;
            set
            {
                var oldXmax = xmax;
                x = value;
                xmax = oldXmax;
            }
        }

        public int xmax
        {
            readonly get => x + width;
            set => width = value - x;
        }

        public int ymin
        {
            readonly get => y;
            set
            {
                var oldYmax = ymax;
                y = value;
                ymax = oldYmax;
            }
        }

        public int ymax
        {
            readonly get => y + height;
            set => height = value - y;
        }

        public int2 ExtentMin
        {
            readonly get => new int2(xmin, ymin);
            set
            {
                xmin = value.x;
                ymin = value.y;
            }
        }

        public int2 ExtentMax
        {
            readonly get => new int2(xmax, ymax);
            set
            {
                xmax = value.x;
                ymax = value.y;
            }
        }

        public int2 Center
        {
            readonly get => new int2(x + width / 2, y + height / 2);
            set
            {
                var off = value - Center;
                ExtentMin += off;
            }
        }

        public int2 Position
        {
            readonly get => new int2(x, y);
            set
            {
                x = value.x;
                y = value.y;
            }
        }

        public int2 Size
        {
            readonly get => new int2(width, height);
            set
            {
                width = value.x;
                height = value.y;
            }
        }

        public readonly bool IsPointInside(in int2 point)
        {
            var offsetPoint = point - ExtentMin;

            if (offsetPoint.x < 0 || offsetPoint.y < 0)
                return false;

            if (offsetPoint.x >= width || offsetPoint.y >= height)
                return false;

            return true;
        }

        public readonly float2 GetPoint(in float2 normalizedPosition) => Position + Size * normalizedPosition;
        public readonly float2 GetNormalizedPoint(in int2 point) => (point - Position) / (float2)Size;
        public readonly float2 GetNormalizedPoint(in float2 point) => (point - Position) / (float2)Size;

        public readonly bool Intersects(IntRect other) => MathX.DoIntervalsIntersect(xmin, xmax, other.xmin, other.xmax)
            && MathX.DoIntervalsIntersect(ymin, ymax, other.ymin, other.ymax);

        public readonly IntRect Translate(in int2 offset) => new IntRect(Position + offset, Size);

        public static bool operator ==(/*in*/ IntRect a, /*in*/ IntRect b)
        {
            return a.x == b.x && a.y == b.y && a.width == b.width && a.height == b.height;
        }

        public static bool operator !=(/*in*/ IntRect a, /*in*/ IntRect b)
        {
            return a.x != b.x || a.y != b.y || a.width != b.width || a.height != b.height;
        }

        public static IntRect operator *(/*in*/ IntRect IntRect, int n)
        {
            return new IntRect(IntRect.x * n, IntRect.y * n, IntRect.width * n, IntRect.height * n);
        }

        public static IntRect operator /(/*in*/ IntRect IntRect, int n)
        {
            return new IntRect(IntRect.x / n, IntRect.y / n, IntRect.width / n, IntRect.height / n);
        }

        public static IntRect operator *(/*in*/ IntRect IntRect, /*in*/ int2 v)
        {
            return new IntRect(IntRect.x * v.x, IntRect.y * v.y, IntRect.width * v.x, IntRect.height * v.y);
        }

        public static IntRect operator /(/*in*/ IntRect IntRect, /*in*/ int2 v)
        {
            return new IntRect(IntRect.x / v.x, IntRect.y / v.y, IntRect.width / v.x, IntRect.height / v.y);
        }

        public static explicit operator int4(/*in*/ IntRect IntRect)
        {
            return new int4(IntRect.x, IntRect.y, IntRect.width, IntRect.height);
        }

        public static explicit operator IntRect(in int4 v)
        {
            return new IntRect(v.x, v.y, v.z, v.w);
        }

        public static explicit operator IntRect(Rect rect)
        {
            return new IntRect((int)rect.x, (int)rect.y, (int)rect.width, (int)rect.height);
        }

        public static explicit operator Rect(IntRect rect) => new Rect(rect.x, rect.y, rect.width, rect.height);

        public static implicit operator RenderIntRect(IntRect intrect) => new RenderIntRect(intrect.x, intrect.y, intrect.width, intrect.height);
        public static implicit operator IntRect(RenderIntRect intBounds) => new IntRect(intBounds.x, intBounds.y, intBounds.width, intBounds.height);

        public readonly override bool Equals(object obj)
        {
            if (obj is IntRect)
                return this == (IntRect)obj;
            else
                return false;
        }

        public readonly bool Equals(IntRect other)
        {
            return this == other;
        }

        public override string ToString()
        {
            return $"[X={x}; Y={y}; W={width}; H={height}]";
        }

        public readonly override int GetHashCode()
        {
            var hashCode = -1222528132;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + width.GetHashCode();
            hashCode = hashCode * -1521134295 + height.GetHashCode();
            return hashCode;
        }
    }

    public static class IntRectExtensions
    {
        public static DataTreeDictionary Save(this IntRect v)
        {
            var dict = new DataTreeDictionary();

            dict.Add("X", v.x);
            dict.Add("Y", v.y);
            dict.Add("Width", v.width);
            dict.Add("Height", v.height);

            return dict;
        }

        public static IntRect LoadIntRect(this DataTreeNode node)
        {
            var dict = (DataTreeDictionary)node;

            return new IntRect(
                dict["X"].LoadInt(),
                dict["Y"].LoadInt(),
                dict["Width"].LoadInt(),
                dict["Height"].LoadInt());
        }

        public static void Write(this BinaryWriter wr, IntRect IntRect)
        {
            wr.Write(IntRect.x);
            wr.Write(IntRect.y);
            wr.Write(IntRect.width);
            wr.Write(IntRect.height);
        }

        public static IntRect ReadIntRect(this BinaryReader rd)
        {
            return new IntRect(rd.ReadInt32(), rd.ReadInt32(), rd.ReadInt32(), rd.ReadInt32());
        }
    }
}
