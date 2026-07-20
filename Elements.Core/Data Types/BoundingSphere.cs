using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using JsonIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
using System.Diagnostics;

namespace Elements.Core
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public struct BoundingSphere
    {
        [JsonIgnore]
        public bool IsEmpty => radius < 0;

        public float3 center;
        public float radius;

        [JsonIgnore]
        public bool IsValid => !center.IsNaN && !center.IsInfinity && !float.IsNaN(radius) && !float.IsInfinity(radius);

        public bool IsContained(in float3 point) => MathX.DistanceSqr(center, point) <= (radius * radius);

        #region SERIALIZATION

        [JsonPropertyName("center")]
        [JsonProperty(PropertyName = "center")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public float3 Center { get => center; set => center = value; }

        [JsonPropertyName("radius")]
        [JsonProperty(PropertyName = "radius")]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public float Radius { get => radius; set => radius = value; }

        #endregion

        public BoundingSphere(in float3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public bool Intersects(BoundingSphere other)
        {
            var delta = center - other.center;
            var distSqr = delta.SqrMagnitude;

            var minDist = radius + other.radius;
            var minDistSqr = minDist * minDist;

            return distSqr < minDistSqr;
        }

        public void Encapsulate(in float3 point)
        {
            if(IsEmpty)
            {
                radius = 0;
                center = point;
                return;
            }

            var dist = MathX.DistanceSqr(center, point);

            if(dist > radius * radius)
            {
                var dir = (point - center).Normalized;
                var p0 = center - dir * radius;
                var p1 = point;

                center = (p0 + p1) * 0.5f;
                radius = MathX.Distance(p0, p1) * 0.5f;
            }
        }

        public void Encapsulate(BoundingSphere sphere)
        {
            if(IsEmpty)
            {
                center = sphere.center;
                radius = sphere.radius;
            }

            var dir = sphere.center - center;

            dir = dir.GetNormalized(out float centerDistance);

            // if the spheres are at the same point, skip
            if (dir.SqrMagnitude < 1e-6f)
            {
                if (sphere.radius > radius)
                {
                    center = sphere.center;
                    radius = sphere.radius;
                }

                return;
            }

            // check if the sphere is fully contained within
            if (radius > sphere.radius)
            {
                // fully contained, can skip
                if (centerDistance + sphere.radius <= radius)
                    return;
            }
            else
            {
                // it's fully contained in the other sphere, can just take its identity
                if(centerDistance + radius <= sphere.radius)
                {
                    center = sphere.center;
                    radius = sphere.radius;
                }
            }

            var point0 = center - dir * radius;
            var point1 = sphere.center + dir * sphere.radius;

            center = (point0 + point1) * 0.5f;
            radius = MathX.Distance(point0, point1) * 0.5f;
        }

        public static BoundingSphere FromTwoPoints(in float3 point0, in float3 point1)
        {
            return new BoundingSphere((point0 + point1) * 0.5f, MathX.Distance(point0, point1) * 0.5f);
        }

        public static BoundingSphere RitterBoundingSphere(List<float3> points)
        {
            if (points.Count == 0)
                throw new Exception("Collections contains no points");
            if (points.Count == 1)
                return new BoundingSphere(points[0], 0f);
            if (points.Count == 2)
                return FromTwoPoints(points[0], points[1]);

            float3 a = points[0];

            float3 b = default;
            float maxDistance = 0f;
            for(int i = 1; i < points.Count; i++)
            {
                var dist = MathX.Distance(points[i], a);
                if(dist > maxDistance)
                {
                    b = points[i];
                    maxDistance = dist;
                }
            }

            float3 c = default;
            maxDistance = 0f;
            for (int i = 0; i < points.Count; i++)
            {
                var dist = MathX.Distance(points[i], b);
                if (dist > maxDistance)
                {
                    c = points[i];
                    maxDistance = dist;
                }
            }

            var bounds = FromTwoPoints(b, c);

            // encapsulate all the points, this will expand the sphere in case some are missed
            for (int i = 0; i < points.Count; i++)
                bounds.Encapsulate(points[i]);

            return bounds;
        }

        public static BoundingSphere FromBoundingBox(BoundingBox box)
        {
            if (!box.IsValid)
                return new BoundingSphere(float3.One * float.PositiveInfinity, float.NegativeInfinity);

            return new BoundingSphere(box.Center, (box.max - box.Center).Magnitude);
        }

        public static BoundingSphere Empty() => new BoundingSphere(float3.Zero, -1);

        public override string ToString()
        {
            return $"Center: {center}, Radius: {radius}";
        }
    }
}
