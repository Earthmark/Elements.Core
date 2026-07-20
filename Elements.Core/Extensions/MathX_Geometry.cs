using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public static partial class MathX
    {
        // Based on https://rosettacode.org/wiki/Find_the_intersection_of_a_line_with_a_plane#C.23
        public static float3 RayPlaneIntersection(in float3 rayOrigin, in float3 rayDirection, in float3 planePoint, in float3 planeNormal)
        {
            var diff = rayOrigin - planePoint;
            var prod1 = Dot(diff, planeNormal);
            var prod2 = Dot(rayDirection, planeNormal);
            var prod3 = prod1 / prod2;

            return rayOrigin - rayDirection * prod3;
        }

        // Based on https://gamedev.stackexchange.com/questions/96459/fast-ray-sphere-collision-code
        public static float3? RaySphereIntersection(in float3 center, float radius, in float3 rayOrigin, in float3 rayDirection)
        {
            var m = rayOrigin - center;
            var b = Dot(m, rayDirection);
            var c = Dot(m, m) - radius * radius;

            if (c > 0 && b > 0)
                return null;

            var discr = b * b - c;

            if (discr < 0)
                return null;

            var discrSqrt = Sqrt(discr);

            var t0 = -b - discrSqrt;
            var t1 = -b + discrSqrt;

            // both points are behind
            if (t0 < 0 && t1 < 0)
                return null;

            float t;

            if (t0 > 0 && t1 > 0)
                t = (t0 < t1) ? t0 : t1;
            else
                t = (t1 < 0) ? t0 : t1;

            return rayOrigin + t * rayDirection;
        }

        public static void FindSphereForTangent(in float3 linePoint, in float3 lineDirection, float3 sphereCenter,
            out float3 tangentPoint, out float radius)
        {
            // move the sphere center to the coordinate system of the line point
            sphereCenter -= linePoint;

            // project the sphere center onto the line
            tangentPoint = MathX.Project(sphereCenter, lineDirection);

            // compute distance
            // TODO!!! More efficient way to do this perhaps?
            radius = MathX.Distance(sphereCenter, tangentPoint);

            // move it back by the line point offset
            tangentPoint += linePoint;
        }

        public static floatQ ComputeRotationAtTargetPoint(in float3 pivot, in float3 point, float3 direction, in float3 targetPoint)
        {
            direction = direction.Normalized;

            FindSphereForTangent(point, direction, pivot, out float3 tangentPoint, out float tangentDistance);

            var targetPointDistance = MathX.Distance(pivot, targetPoint);

            var intersection = RaySphereIntersection(pivot, targetPointDistance, tangentPoint, direction);

            if (intersection == null)
                return floatQ.Identity;

            var intersectionDir = (intersection.Value - pivot).Normalized;
            var targetPointDir = (targetPoint - pivot).Normalized;

            return floatQ.FromToRotation(intersectionDir, targetPointDir);
        }

        public static float3 ClosestPointOnLine(in float3 linePoint0, in float3 linePoint1, in float3 point)
        {
            var vector = linePoint1 - linePoint0;

            var dir = vector.GetNormalized(out float length);

            // If the line is pretty much a point, it's just a point at that point
            if (Approximately(length, 0f))
                return linePoint0;

            var offsetPoint = point - linePoint0;

            // Align the point to the line
            var dot = Dot(dir, offsetPoint);

            if (dot <= 0)
                return linePoint0;

            if (dot >= length)
                return linePoint1;

            return linePoint0 + (dir * dot);
        }

        // Based on https://stackoverflow.com/questions/2824478/shortest-distance-between-two-line-segments
        public static bool ClosestPointsBetweenLines(in float3 linePoint0, in float3 lineDir0, in float3 linePoint1, in float3 lineDir1,
            out float3 point0, out float3 point1)
        {
            var a = lineDir0.Normalized;
            var b = lineDir1.Normalized;

            var cross = Cross(a, b);
            var crossMag = cross.Magnitude;
            var denom = crossMag * crossMag;

            // lines are parallel
            if(Approximately(denom, 0))
            {
                point0 = default;
                point1 = default;

                return false;
            }

            var t = linePoint1 - linePoint0;

            var detA = new float3x3(t, b, cross).Determinant;
            var detB = new float3x3(t, a, cross).Determinant;

            var t0 = detA / denom;
            var t1 = detB / denom;

            point0 = linePoint0 + a * t0;
            point1 = linePoint1 + b * t1;

            return true;
        }

        public static bool TryComputeTriangleNormal(in float3 p0, in float3 p1, in float3 p2, out float3 normal)
        {
            var b = p0;
            var v0 = p1 - b;
            var v1 = p2 - b;

            var cross = MathX.Cross(v0, v1);

            var sqrMagnitude = cross.SqrMagnitude;

            if (sqrMagnitude <= 1e-12f)
            {
                normal = default;
                return false;
            }

            var mag = MathX.Sqrt(sqrMagnitude);
            var invMag = 1f / mag;

            normal = cross * invMag;

            return true;
        }

        public static float2 PointOnCircle(float normalizedPosition)
        {
            var angle = normalizedPosition * TAU;
            return new float2(Sin(angle), Cos(-angle));
        }

        public static float3 PointOnUVSphere(in float2 uv, float radius) => OrientationOnUVSphere(uv) * new float3(0, 0, radius);

        public static floatQ OrientationOnUVSphere(in float2 uv)
        {
            return floatQ.AxisAngleRad(float3.Up, uv.x * TAU) 
                * floatQ.AxisAngleRad(float3.Right, uv.y * PI - HALF_PI);
        }

        public static float? FindRayToLineIntersectionDistance(in float2 origin, in float2 direction, in float2 point0, in float2 point1)
        {
            // https://stackoverflow.com/questions/14307158/how-do-you-check-for-intersection-between-a-line-segment-and-a-line-ray-emanatin
            var dir = direction.Normalized;

            var v1 = origin - point0;
            var v2 = point1 - point0;
            var v3 = new float2(-dir.y, dir.x);

            var dot = Dot(v2, v3);

            if (Abs(dot) < 0.000001f)
                return null;

            var t1 = ((v2.x * v1.y) - (v2.y * v1.x)) / dot;
            var t2 = Dot(v1, v3) / dot;

            if (t1 >= 0f && (t2 >= 0f && t2 <= 1f))
                return t1;

            return null;
        }

        public static float2? FindRayToLineIntersection(in float2 origin, in float2 direction, in float2 point0, in float2 point1)
        {
            var distance = FindRayToLineIntersectionDistance(origin, direction, point0, point1);

            if (distance == null)
                return null;

            return origin + direction.Normalized * distance.Value;
        }

        public static float2? FindRayRectangleIntersection(Rect rect, in float2 origin, in float2 direction)
        {
            var dist0 = FindRayToLineIntersectionDistance(origin, direction, rect.GetExtent(0), rect.GetExtent(1));
            var dist1 = FindRayToLineIntersectionDistance(origin, direction, rect.GetExtent(1), rect.GetExtent(2));
            var dist2 = FindRayToLineIntersectionDistance(origin, direction, rect.GetExtent(2), rect.GetExtent(3));
            var dist3 = FindRayToLineIntersectionDistance(origin, direction, rect.GetExtent(3), rect.GetExtent(0));

            float? minDist = dist0;

            if (minDist == null || dist1 < minDist)
                minDist = dist1;

            if (minDist == null || dist2 < minDist)
                minDist = dist2;

            if (minDist == null || dist3 < minDist)
                minDist = dist3;

            if (minDist == null)
                return null;

            return origin + direction.Normalized * minDist.Value;
        }

        public static int FindLineCircleIntersections(in float2 circlePos, float radius,
            in float2 point0, in float2 point1, out float2 intersection0, out float2 intersection1)
        {
            // http://csharphelper.com/blog/2014/09/determine-where-a-line-intersects-a-circle-in-c/

            float cx = circlePos.x;
            float cy = circlePos.y;

            float dx, dy, A, B, C, det, t;

            dx = point1.x - point0.x;
            dy = point1.y - point0.y;

            A = dx * dx + dy * dy;
            B = 2 * (dx * (point0.x - cx) + dy * (point0.y - cy));
            C = (point0.x - cx) * (point0.x - cx) +
                (point0.y - cy) * (point0.y - cy) -
                radius * radius;

            det = B * B - 4 * A * C;

            if ((A <= 0) || (det < 0))
            {
                // No real solutions.
                //intersection0 = intersection1 = new Vector2(0f, 0f);
                intersection0 = new float2(float.NaN, float.NaN);
                intersection1 = new float2(float.NaN, float.NaN);
                return 0;
            }
            else if (det == 0)
            {
                // One solution.
                t = -B / (2 * A);
                intersection0 = new float2(point0.x + t * dx, point0.y + t * dy);
                intersection1 = new float2(float.NaN, float.NaN);
                return 1;
            }
            else
            {
                // Two solutions.
                t = (float)((-B + Sqrt(det)) / (2 * A));
                intersection0 = new float2(point0.x + t * dx, point0.y + t * dy);

                t = (float)((-B - Sqrt(det)) / (2 * A));
                intersection1 = new float2(point0.x + t * dx, point0.y + t * dy);
                return 2;
            }
        }

        public static float3 ClosestPointOnConeSurface(in float3 coneCenter, in floatQ coneOrientation, float coneHeight, float coneBaseRadius, in float3 point) =>
            ClosestPointOnConeSurface(coneCenter, coneOrientation, coneHeight, coneBaseRadius, point, out _);

        public static float3 ClosestPointOnConeSurface(in float3 coneCenter, in floatQ coneOrientation,
            float coneHeight, float coneBaseRadius, float3 point,
            out bool isInside)
        {
            // First align everything into origin, so the cone is right at the center and upright
            point -= coneCenter;
            point = coneOrientation.Inverted * point;

            var coneCenterOffset = new float3(0f, coneHeight * 0.5f, 0f);

            // Make sure that the cone base is at the bottom
            point += coneCenterOffset;

            // Project it on the base
            var baseProjectedPoint = point.x_z;
            var basePoint = baseProjectedPoint.Normalized * coneBaseRadius;

            if(point.y < 0)
            {
                // it's below the base, check the distance to a line coming from the center to the base
                point = ClosestPointOnLine(float3.Zero, basePoint, point);

                // It's below the base, so we know for sure it's not inside
                isInside = false;
            }
            else
            {
                // We check against the side
                point = ClosestPointOnLine(basePoint, new float3(0f, coneHeight, 0f), point);

                // We have to figure out if it's inside or not
                var surfaceBaseProjectedPoint = point.x_z;

                isInside = baseProjectedPoint.SqrMagnitude < surfaceBaseProjectedPoint.SqrMagnitude;
            }

            point -= coneCenterOffset;
            point = coneOrientation * point;
            point += coneCenter;

            return point;
        }

        public static float DistanceFromCone(in float3 coneCenter, in floatQ coneOrientation, float coneHeight, float coneBaseRadius, in float3 point)
        {
            var surfacePoint = ClosestPointOnConeSurface(coneCenter, coneOrientation, coneHeight, coneBaseRadius, point, out var isInside);

            if (isInside)
                return 0f;

            return MathX.Distance(point, surfacePoint);
        }

        public static float3 ClosestPointOnSphericalSector(in float3 center, in float3 direction, float radius, float angle,
            float3 point, out bool isInside)
        {
            // First align everything into origin, so the cone is right at the center and upright
            point -= center;

            var actualAngle = MathX.Angle(direction, point);

            if(actualAngle <= angle)
            {
                point = point.GetNormalized(out float actualRadius);
                // We are within the sector, so just clamp the point to the surface of it
                point *= radius;
                point += center;

                isInside = actualRadius <= radius;

                return point;
            }

            // The point is outside, so we need to find point on a line between the center and the outer line

            // It's not, so we need to construct a line. To do this, figure out a point at the "rim" of the
            // sphere sector
            var normalizedPoint = point.Normalized;
            var extentPoint = MathX.Slerp(direction, normalizedPoint, angle / actualAngle);
            extentPoint *= radius;

            point = ClosestPointOnLine(float3.Zero, extentPoint, point);
            point += center;

            // it's outside of the sector, so it's never going to be inside
            isInside = false;

            return point;
        }

        public static float DistanceFromSphericalSector(in float3 center, in float3 direction, float radius, float angle,
            in float3 point)
        {
            var surfacePoint = ClosestPointOnSphericalSector(center, direction, radius, angle, point, out var isInside);

            if (isInside)
                return 0f;

            return MathX.Distance(point, surfacePoint);
        }

        public static float3 ClosestPointOnSquare(in float3 point, in float3 rectangleOrigin, in floatQ rectangleOrientation, in float2 rectangleSideLength)
        {
            var adjustedPoint = point - rectangleOrigin;
            adjustedPoint = rectangleOrientation.Inverted * adjustedPoint;

            var halfSize = rectangleSideLength * 0.5f;
            var rectanglePoint = adjustedPoint.xz;
            var rectangleDistanceSign = MathX.Sign(rectanglePoint);

            rectanglePoint = MathX.Abs(rectanglePoint);
            rectanglePoint = MathX.Min(rectanglePoint, halfSize);
            rectanglePoint *= rectangleDistanceSign;

            var closestPoint = rectanglePoint.x_y;
            closestPoint = rectangleOrientation * closestPoint;
            closestPoint += rectangleOrigin;

            return closestPoint;
        }

        public static float DistanceFromSquare(in float3 point, in float3 rectangleOrigin, in floatQ rectangleOrientation, in float2 rectangleSideLength)
        {
            var closestPoint = ClosestPointOnSquare(point, rectangleOrigin, rectangleOrientation, rectangleSideLength);

            return MathX.Distance(point, closestPoint);
        }

        public static float3 ClosestPointOnBox(in float3 point, in float3 boxCenter, in floatQ boxOrientation, in float3 boxSize)
        {
            var adjustedPoint = point - boxCenter;
            adjustedPoint = boxOrientation.Inverted * adjustedPoint;

            float closestDistance = float.MaxValue;
            float3 closestPoint = float3.NaN;

            void UpdatePoint(float3 faceOffset, floatQ faceOrientation, float2 faceSize)
            {
                var point = ClosestPointOnSquare(adjustedPoint, faceOffset, faceOrientation, faceSize);
                var distance = MathX.Distance(adjustedPoint, point);

                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = point;
                }
            }

            var halfSize = boxSize * 0.5f;

            if (adjustedPoint.x < 0)
                UpdatePoint(new float3(-halfSize.x, 0, 0), floatQ.LookRotation(float3.Up, float3.Left), boxSize.zy);
            else
                UpdatePoint(new float3(halfSize.x, 0, 0), floatQ.LookRotation(float3.Up, float3.Right), boxSize.zy);

            if (adjustedPoint.y < 0)
                UpdatePoint(new float3(0, -halfSize.y, 0), floatQ.LookRotation(float3.Forward, float3.Down), boxSize.xz);
            else
                UpdatePoint(new float3(0, halfSize.y, 0), floatQ.LookRotation(float3.Forward, float3.Up), boxSize.xz);

            if (adjustedPoint.z < 0)
                UpdatePoint(new float3(0, 0, -halfSize.z), floatQ.LookRotation(float3.Up, float3.Backward), boxSize.xy);
            else
                UpdatePoint(new float3(0, 0, halfSize.z), floatQ.LookRotation(float3.Up, float3.Forward), boxSize.xy);

            closestPoint = boxOrientation * closestPoint;
            closestPoint += boxCenter;

            return closestPoint;
        }
    }
}
