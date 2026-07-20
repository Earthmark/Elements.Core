using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Renderite.Shared;

namespace Elements.Core
{
    public struct Rect : IEquatable<Rect>
    {
        public float2 position;
        public float2 size;

        public static Rect FromMinMax(in float2 min, in float2 max) => new Rect(min, max - min);

        public Rect(float x, float y, float width, float height)
        {
            this.position = new float2(x, y);
            this.size = new float2(width, height);
        }

        public Rect(in float2 position, in float2 size)
        {
            this.position = position;
            this.size = size;
        }

        public float xmin
        {
            get => position.x;
            set
            {
                size = new float2(size.x + (position.x - value), size.y);
                position = new float2(value, position.y);
            }
        }

        public float xmax
        {
            get => position.x + size.x;
            set => size = new float2(value - position.x, size.y);
        }

        public float ymin
        {
            get => position.y;
            set
            {
                size = new float2(size.x, size.y + (position.y - value));
                position = new float2(position.x, value);
            }
        }

        public float ymax
        {
            get => position.y + size.y;
            set => size = new float2(size.x, value - position.y);
        }

        public float2 ExtentMin
        {
            get => position;
            set => position = value;
        }

        public float2 ExtentMax
        {
            get => position + size;
            set => size = value - position;
        }

        public float2 Center
        {
            get => position + size * 0.5f;
            set => position += value - Center;
        }

        public float4 MinMaxExtents
        {
            get => new float4(position, position + size);
            set
            {
                position = value.xy;
                size = value.zw - value.xy;
            }
        }

        public float x
        {
            get => position.x;
            set => position = new float2(value, position.y);
        }

        public float y
        {
            get => position.y;
            set => position = new float2(position.x, value);
        }

        public float width
        {
            get => size.x;
            set => size = new float2(value, size.y);
        }

        public float height
        {
            get => size.y;
            set => size = new float2(size.x, value);
        }

        public float2 GetExtent(int index, RectOrientation orientation = RectOrientation.Default)
        {
            switch (orientation)
            {
                case RectOrientation.CounterClockwise90:
                    index += 1;
                    index %= 4;
                    break;

                case RectOrientation.UpsideDown180:
                    index += 2;
                    index %= 4;
                    break;

                case RectOrientation.Clockwise90:
                    index += 3;
                    index %= 4;
                    break;
            }

            switch (index)
            {
                case 0:
                    return position;

                case 1:
                    return new float2(position.x, ymax);

                case 2:
                    return position + size;

                case 3:
                    return new float2(xmax, position.y);

                default:
                    throw new ArgumentOutOfRangeException("index");
            }
        }

        public float2 GetPoint(in float2 normalizedPosition) => position + size * normalizedPosition;

        public Rect Encapsulate(Rect other) => FromMinMax(MathX.Min(ExtentMin, other.ExtentMin), MathX.Max(ExtentMax, other.ExtentMax));

        public IntRect RoundToIntConservative()
        {
            var min = MathX.FloorToInt(ExtentMin);
            var max = MathX.CeilToInt(ExtentMax);

            return IntRect.FromMinMax(min, max);
        }

        public bool IsPointInside(in float2 point)
        {
            var offsetPoint = point - position;

            if (offsetPoint.x < 0 || offsetPoint.y < 0)
                return false;

            if (offsetPoint.x > width || offsetPoint.y > height)
                return false;

            return true;
        }

        public bool Intersects(Rect other) => MathX.DoIntervalsIntersect(xmin, xmax, other.xmin, other.xmax)
            && MathX.DoIntervalsIntersect(ymin, ymax, other.ymin, other.ymax);

        public float2 GetNormalizedPoint(in float2 point) => (point - position) / size;

        public Rect Translate(in float2 offset) => new Rect(position + offset, size);
        public Rect AddPadding(float padding)
        {
            padding = MathX.Min(padding, size.x * 0.5f, size.y * 0.5f);
            return new Rect(position + padding, size - padding * 2);
        }

        public float Distance(in float2 point)
        {
            var dx = MathX.Max(xmin - point.x, 0, point.x - xmax);
            var dy = MathX.Max(ymin - point.y, 0, point.y - ymax);

            return MathX.Sqrt(dx * dx + dy * dy);
        }

        public Rect TranslateAndScale(in float2 offset, in float2 scale) => new Rect(position + offset, size * scale);

        public Rect Clip(Rect mask) => Rect.FromMinMax(MathX.Max(ExtentMin, mask.ExtentMin), MathX.Min(ExtentMax, mask.ExtentMax));

        public Rect CenterBySize() => new Rect(position - size * 0.5f, size);

        public static bool operator==(/*in*/ Rect a, /*in*/ Rect b)
        {
            return a.position == b.position && a.size == b.size;
        }

        public static bool operator !=(/*in*/ Rect a, /*in*/ Rect b)
        {
            return a.position != b.position || a.size != b.size;
        }

        public static Rect operator *(/*in*/ Rect rect, float n)
        {
            return new Rect(rect.position * n, rect.size * n);
        }

        public static Rect operator /(/*in*/ Rect rect, float n)
        {
            var inv = 1f / n;
            return new Rect(rect.position * inv, rect.size * inv);
        }

        public static Rect operator *(/*in*/ Rect rect, in float2 v)
        {
            return new Rect(rect.position * v, rect.size * v);
        }

        public static Rect operator /(/*in*/ Rect rect, in float2 v)
        {
            var inv = 1f / v;
            return new Rect(rect.position * inv, rect.size * inv);
        }

        public static explicit operator float4(/*in*/ Rect rect)
        {
            return new float4(rect.position, rect.size);
        }

        public static explicit operator Rect(in float4 v)
        {
            return new Rect(v.xy, v.zw);
        }

        public static implicit operator RenderRect(Rect rect) => new RenderRect(rect.x, rect.y, rect.width, rect.height);
        public static implicit operator Rect(RenderRect rect) => new Rect(rect.x, rect.y, rect.width, rect.height);

        public static Rect Lerp(Rect a, Rect b, float lerp)
        {
            if (lerp <= 0f)
                return a;

            if (lerp >= 1f)
                return b;

            return LerpUnclamped(a, b, lerp);
        }

        public static Rect LerpUnclamped(Rect a, Rect b, float lerp) => new Rect(MathX.LerpUnclamped(a.position, b.position, lerp), MathX.LerpUnclamped(a.size, b.size, lerp));

        public override bool Equals(object obj)
        {
            if (obj is Rect)
                return this == (Rect)obj;
            else
                return false;
        }

        public bool Equals(Rect other) => this == other;

        public override string ToString()
        {
            return $"[X={xmin}; Y={ymin}; W={width}; H={height}]";
        }

        public override int GetHashCode()
        {
            var hashCode = 1804577526;
            hashCode = hashCode * -1521134295 + position.GetHashCode();
            hashCode = hashCode * -1521134295 + size.GetHashCode();
            return hashCode;
        }
    }

    public static class RectExtensions
    {
        public static DataTreeDictionary Save(this Rect v)
        {
            var dict = new DataTreeDictionary();

            dict.Add("X", v.xmin);
            dict.Add("Y", v.ymin);
            dict.Add("Width", v.width);
            dict.Add("Height", v.height);

            return dict;
        }

        public static Rect LoadRect(this DataTreeNode node)
        {
            var dict = (DataTreeDictionary)node;

            return new Rect(
                dict["X"].LoadFloat(),
                dict["Y"].LoadFloat(),
                dict["Width"].LoadFloat(),
                dict["Height"].LoadFloat() );
        }

        public static void Write(this BinaryWriter wr, Rect rect)
        {
            wr.Write(rect.position);
            wr.Write(rect.size);
        }

        public static Rect ReadRect(this BinaryReader rd)
        {
            return new Rect(rd.Read2D_Single(), rd.Read2D_Single());
        }
    }
}
