using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
using System.Diagnostics;
using Renderite.Shared;

namespace Elements.Core
{

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public struct BoundingBox : IEquatable<BoundingBox>
    {
        public const int VERTEX_POINT_COUNT = 8;

        [JsonIgnore]
        public float3 min;
        [JsonIgnore]
        public float3 max;

        // Json serialization
        [JsonPropertyName("min")]
        [JsonProperty(PropertyName = "min")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public float3 MinExtent { readonly get => min; set => min = value; }

        [JsonPropertyName("max")]
        [JsonProperty(PropertyName = "max")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public float3 MaxExtent { readonly get => max; set => max = value; }

        [JsonIgnore]
        public readonly bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => min.x > max.x || min.y > max.y || min.z > max.z;
        }

        [JsonIgnore]
        public readonly bool IsInfiniteOnX => float.IsNegativeInfinity(min.x) && float.IsPositiveInfinity(max.x);
        [JsonIgnore]
        public readonly bool IsInfiniteOnY => float.IsNegativeInfinity(min.y) && float.IsPositiveInfinity(max.y);
        [JsonIgnore]
        public readonly bool IsInfiniteOnZ => float.IsNegativeInfinity(min.z) && float.IsPositiveInfinity(max.z);

        [JsonIgnore]
        public readonly bool IsInfinite => IsInfiniteOnX || IsInfiniteOnY || IsInfiniteOnZ;

        [JsonIgnore]
        public readonly bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (IsEmpty)
                    return false;

                var size = Size;

                return !size.IsInfinity && !size.IsNaN;
            }
        }

        [JsonIgnore]
        public float3 Center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => (max + min) * 0.5f;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var _halfSize = Size * 0.5f;

                min = value - _halfSize;
                max = value + _halfSize;
            }
        }

        [JsonIgnore]
        public float3 Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                // If it's an empty bounding box, return size zero
                if (max == float3.MinValue)
                    return float3.Zero;

                return max - min;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                var _center = Center;

                // halve the size
                value *= 0.5f;

                min = _center - value;
                max = _center + value;
            }
        }

        // setups a center-less and size-less bounding box
        // this makes sure that the center is set correctly on the first AddPoint
        public void MakeEmpty()
        {
            min = float3.MaxValue;
            max = float3.MinValue;
        }

        public static BoundingBox Empty()
        {
            return new BoundingBox(float3.MaxValue, float3.MinValue);
        }

        public static BoundingBox Infinite()
        {
            return new BoundingBox(float3.NegativeInfinity, float3.PositiveInfinity);
        }

        public static BoundingBox CenterSize(in float3 center, in float3 size)
        {
            var halfSize = size * 0.5f;

            return new BoundingBox(center - halfSize, center + halfSize);
        }

        public static BoundingBox CenterRadius(in float3 center, float radius)
        {
            return new BoundingBox(center - radius, center + radius);
        }

        public static BoundingBox FromPoints(in float3 point0, in float3 point1)
        {
            var bounds = new BoundingBox(point0);
            bounds.Encapsulate(point1);
            return bounds;
        }

        public static BoundingBox FromPoints(in float3 point0, in float3 point1, in float3 point2)
        {
            var bounds = new BoundingBox(point0);
            bounds.Encapsulate(point1);
            bounds.Encapsulate(point2);
            return bounds;
        }

        public static BoundingBox FromPoints(params float3[] points)
        {
            var bounds = new BoundingBox(points[0]);
            for(int i = 1; i < points.Length; i++)
                bounds.Encapsulate(points[i]);
            return bounds;
        }

        public static BoundingBox FromBoundingSphere(BoundingSphere sphere) => CenterSize(sphere.center, float3.One * sphere.radius * 2);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encapsulate(BoundingBox box)
        {
            if (box.IsEmpty)
                return;

            Encapsulate(box.min);
            Encapsulate(box.max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encapsulate(Rect rect)
        {
            Encapsulate(rect.ExtentMin);
            Encapsulate(rect.ExtentMax);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encapsulate(in float3 point)
        {
            min = MathX.Min(point, min);
            max = MathX.Max(point, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encapsulate(in float3 point, float size)
        {
            var halfSize = size * 0.5f;

            min = MathX.Min(point - halfSize, min);
            max = MathX.Max(point + halfSize, max);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Encapsulate(BoundingSphere sphere)
        {
            Encapsulate(sphere.center - sphere.radius);
            Encapsulate(sphere.center + sphere.radius);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Expand(in float3 size)
        {
            var halfSize = size * 0.5f;

            min -= halfSize;
            max += halfSize;
        }

        public readonly BoundingBox Transform(in float4x4 transform)
        {
            if (!IsValid)
                return this;

            BoundingBox translated = BoundingBox.Empty();

            for(int i = 0; i < VERTEX_POINT_COUNT; i++)
                translated.Encapsulate(transform * GetVertexPoint(i));

            return translated;
        }

        public BoundingBox(in float3 initialPoint)
        {
            min = max = initialPoint;
        }

        public BoundingBox(in float3 min, in float3 max)
        {
            this.min = min;
            this.max = max;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Intersects(BoundingBox other) => Intersects(ref other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Intersects(ref BoundingBox other)
        {
            if (max.x < other.min.x)
                return false;

            if (max.y < other.min.y)
                return false;

            if (max.z < other.min.z)
                return false;

            if (min.x > other.max.x)
                return false;

            if (min.y > other.max.y)
                return false;

            if (min.z > other.max.z)
                return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in float3 point)
        {
            return point.x >= min.x && point.y >= min.y && point.z >= min.z &&
                point.x <= max.x && point.y <= max.y && point.z <= max.z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in float3 point, float radius)
        {
            if (max.x < point.x - radius)
                return false;

            if (max.y < point.y - radius)
                return false;

            if (max.z < point.z - radius)
                return false;

            if (min.x > point.x + radius)
                return false;

            if (min.y > point.y + radius)
                return false;

            if (min.z > point.z + radius)
                return false;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Contains(in float3 point, in float3 size)
        {
            var halfSize = size * 0.5f;

            if (max.x < point.x - halfSize.x)
                return false;

            if (max.y < point.y - halfSize.y)
                return false;

            if (max.z < point.z - halfSize.z)
                return false;

            if (min.x > point.x + halfSize.x)
                return false;

            if (min.y > point.y + halfSize.y)
                return false;

            if (min.z > point.z + halfSize.z)
                return false;

            return true;
        }

        public override string ToString()
        {
            if (IsEmpty)
                return "Empty Bounds";

            return $"Center: {Center} Size: {Size} -- Min: {min} Max: {max}";
        }

        public readonly bool Equals(BoundingBox other)
        {
            return this == other;
        }

        public static bool operator==(BoundingBox a, BoundingBox b)
        {
            return a.min == b.min && a.max == b.max;
        }

        public static bool operator !=(BoundingBox a, BoundingBox b)
        {
            return a.min != b.min || a.max != b.max;
        }

        public readonly float3 GetVertexPoint(int index)
        {
            bool _x = (index & 1) != 0;
            bool _y = (index & 2) != 0;
            bool _z = (index & 4) != 0;

            return new float3(
                _x ? min.x : max.x,
                _y ? min.y : max.y,
                _z ? min.z : max.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetOppositeVertexIndex(int index) => (~index) & 0x7;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetOppositeFaceIndex(int index) => (index + 3) % 6;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetOppositeEdgeIndex(int index)
        {
            int xOff = index / 4;
            int idx = index % 4;

            if(xOff == 1)
            {
                idx = (~idx) & 0x03;
                return 4 + idx;
            }
            else
            {
                // flip the least significant bit, swapping -Y to Y or -Z to Z and vice versa
                idx = (idx & 0x02) | (~idx & 0x01);
                if (xOff == 0)
                    return 8 + idx;
                else
                    return idx;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool3 GetFaceMask(int index)
        {
            var faceIndex = index % 3;
            return new bool3(faceIndex == 0, faceIndex == 1, faceIndex == 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool3 GetEdgeMask(int index)
        {
            int xOff = index / 4;
            int idx = index % 4;

            if(xOff == 1)
                return new bool3(false, true, true);
            else
            {
                bool isZ = (idx & 2) != 0;
                return new bool3(true, !isZ, isZ);
            }
        }

        // Order goes as such: Major side -X, 0, X
        // Other sides -Y, Y, -Z, Z for -X and X
        // Simple binary sequnece of both sides when "X" is 0
        public readonly float3 GetEdgePoint(int index)
        {
            int xOff = index / 4;
            int idx = index % 4;
            float3 _halfSize = Size * 0.5f;

            float x;

            if (xOff == 0)
                x = -_halfSize.x;
            else if (xOff == 1)
                x = 0f;
            else
                x = _halfSize.x;

            if(xOff == 1)
            {
                bool yPos = (idx & 1) != 0;
                bool zPos = (idx & 2) != 0;

                return Center + new float3(x, yPos ? _halfSize.y : -_halfSize.y, zPos ? _halfSize.z : -_halfSize.z);
            }
            else
            {
                bool pos = (idx & 1) != 0;
                bool isZ = (idx & 2) != 0;

                if (isZ)
                    return Center + new float3(x, 0f, pos ? _halfSize.z : -_halfSize.z);
                else
                    return Center + new float3(x, pos ? _halfSize.y : -_halfSize.y, 0f);
            }
        }

        public readonly float3 GetFacePoint(int index)
        {
            float3 point = float3.Zero;

            int axisIndex = index % 3;
            bool positive = index >= 3;

            point = point.SetComponent(Size[axisIndex] * 0.5f, axisIndex);
            if (!positive)
                point *= -1;

            return Center + point;
        }

        public static float3 GetFaceDirection(int index)
        {
            float3 point = float3.Zero;

            int axisIndex = index % 3;
            bool positive = index >= 3;

            point = point.SetComponent(1f, axisIndex);
            if (!positive)
                point *= -1;

            return point;
        }

        public override bool Equals(object obj)
        {
            if (obj is BoundingBox)
                return this == (BoundingBox)obj;

            return false;
        }

        public readonly override int GetHashCode()
        {
            var hashCode = 1537547080;
            hashCode = hashCode * -1521134295 + min.GetHashCode();
            hashCode = hashCode * -1521134295 + max.GetHashCode();
            return hashCode;
        }

        public static implicit operator RenderBoundingBox(BoundingBox bounds) =>
            new RenderBoundingBox(bounds.Center, bounds.Size * 0.5f);

        public static implicit operator BoundingBox(RenderBoundingBox bounds) =>
            new BoundingBox(bounds.center - (float3)bounds.extents, bounds.center + (float3)bounds.extents);
    }

    public static class BoundingBoxExtensions
    {
        public static DataTreeDictionary Save(this BoundingBox v)
        {
            var dict = new DataTreeDictionary();

            dict.Add("Min", v.min.Save());
            dict.Add("Max", v.max.Save());

            return dict;
        }

        public static BoundingBox LoadBoundingBox(this DataTreeNode node)
        {
            var dict = (DataTreeDictionary)node;

            return new BoundingBox(dict["Min"].LoadFloat3(), dict["Max"].LoadFloat3());
        }

        public static void Write(this BinaryWriter wr, BoundingBox bounds)
        {
            wr.Write(bounds.min);
            wr.Write(bounds.max);
        }

        public static BoundingBox ReadBoundingBox(this BinaryReader rd)
        {
            return new BoundingBox(rd.Read3D_Single(), rd.Read3D_Single());
        }
    }
}
