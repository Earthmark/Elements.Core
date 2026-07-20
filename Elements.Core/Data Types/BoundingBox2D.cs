using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public struct BoundingBox2D : IEnumerable<float2>
    {
        public float2 Min;
        public float2 Max;

        public readonly bool IsEmpty => Min == float2.MaxValue && Max == float2.MinValue;
        public readonly bool IsValid => !IsEmpty && !Size.IsInfinity && !Size.IsNaN;

        public float2 Center
        {
            readonly get => (Max + Min) * 0.5f;
            set
            {
                var _halfSize = Size * 0.5f;

                Min = value - _halfSize;
                Max = value + _halfSize;
            }
        }
        public float2 Size
        {
            readonly get
            {
                // If it's an empty bounding box, return size zero
                if (Max == float2.MinValue)
                    return float2.Zero;

                return Max - Min;
            }
            set
            {
                var _center = Center;

                // halve the size
                value *= 0.5f;

                Min = _center - value;
                Max = _center + value;
            }
        }

        // setups a center-less and size-less bounding box
        // this makes sure that the center is set correctly on the first AddPoint
        public void MakeEmpty()
        {
            Min = float2.MaxValue;
            Max = float2.MinValue;
        }

        public static BoundingBox2D Empty()
        {
            return new BoundingBox2D()
            {
                Min = float2.MaxValue,
                Max = float2.MinValue
            };
        }

        public void Encapsulate(BoundingBox2D box)
        {
            Encapsulate(box.Min);
            Encapsulate(box.Max);
        }

        public void Encapsulate(float2 point)
        {
            Min = MathX.Min(point, Min);
            Max = MathX.Max(point, Max);
        }

        public BoundingBox2D(float2 initialPoint)
        {
            Min = Max = initialPoint;
        }

        public BoundingBox2D(float2 min, float2 max)
        {
            this.Min = min;
            this.Max = max;
        }

        public override string ToString()
        {
            return $"Center: {Center} Size: {Size} -- Min: {Min} Max: {Max}";
        }

        // Enumerable
        public IEnumerator<float2> GetEnumerator()
        {
            yield return Min;
            yield return new float2(Max.x, Min.y);
            yield return Max;
            yield return new float2(Min.x, Max.y);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
