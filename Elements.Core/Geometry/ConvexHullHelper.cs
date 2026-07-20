using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    // Ported from BEPU
    public static class ConvexHullHelper
    {
        const float EPSILON = 1e-9f;
        const float BIG_EPSILON = 1e-11f;

        public static void GetConvexHull(RawList<float3> points, IList<int> newTriangleIndicies, IList<float3> surfacePoints)
        {
            var originalTriangleIndicies = Pool.BorrowRawList<int>();

            GetConvexHull(points, originalTriangleIndicies);

            var alreadyContainedIndices = Pool.BorrowDictionary<int, int>();

            for (int i = originalTriangleIndicies.Count - 1; i >= 0; i--)
            {
                int index = originalTriangleIndicies[i];

                if(!alreadyContainedIndices.TryGetValue(index, out int newIndex))
                {
                    newIndex = surfacePoints.Count;
                    surfacePoints.Add(points[index]);

                    alreadyContainedIndices.Add(index, newIndex);
                }

                newTriangleIndicies.Add(newIndex);
            }

            Pool.Return(ref originalTriangleIndicies);
            Pool.Return(ref alreadyContainedIndices);
        }

        /// <summary>
        /// Identifies the indices of points in a set which are on the outer convex hull of the set.
        /// </summary>
        /// <param name="points">List of points in the set.</param>
        /// <param name="outputTriangleIndices">List of indices into the input point set composing the triangulated surface of the convex hull.
        /// Each group of 3 indices represents a triangle on the surface of the hull.</param>
        public static bool GetConvexHull(RawList<float3> points, RawList<int> outputTriangleIndices)
        {
            if (points.Count == 0)
                return false;
                //throw new ArgumentException("Point set must have volume.");

            RawList<int> outsidePoints = Pool.BorrowRawList<int>();

            if (outsidePoints.Capacity < points.Count - 4)
                outsidePoints.Capacity = points.Count - 4;

            //Build the initial tetrahedron.
            //It will also give us the location of a point which is guaranteed to be within the
            //final convex hull.  We can use this point to calibrate the winding of triangles.
            //A set of outside point candidates (all points other than those composing the tetrahedron) will be returned in the outsidePoints list.
            //That list will then be further pruned by the RemoveInsidePoints call.
            float3 insidePoint;
            if (!ComputeInitialTetrahedron(points, outsidePoints, outputTriangleIndices, out insidePoint))
            {
                Pool.Return(ref outsidePoints);
                return false;
            }

            //Compute outside points.
            RemoveInsidePoints(points, outputTriangleIndices, outsidePoints);

            var edges = Pool.BorrowRawList<int>();
            var toRemove = Pool.BorrowRawList<int>();
            var newTriangles = Pool.BorrowRawList<int>();

            //We're now ready to begin the main loop.
            while (outsidePoints.Count > 0)
            {
                //While the convex hull is incomplete...
                for (int k = 0; k < outputTriangleIndices.Count; k += 3)
                {
                    //Find the normal of the triangle
                    float3 normal;
                    FindNormal(outputTriangleIndices, points, k, out normal);

                    //Get the furthest point in the direction of the normal.
                    int maxIndexInOutsideList = GetExtremePoint(ref normal, points, outsidePoints);
                    int maxIndex = outsidePoints.Elements[maxIndexInOutsideList];
                    float3 maximum = points.Elements[maxIndex];

                    //If the point is beyond the current triangle, continue.
                    float3 offset = maximum - points.Elements[outputTriangleIndices.Elements[k]];
                    float dot = MathX.Dot(normal, offset);
                    
                    if (dot > 0)
                    {
                        //It's been picked! Remove the maximum point from the outside.
                        outsidePoints.FastRemoveAt(maxIndexInOutsideList);
                        //Remove any triangles that can see the point, including itself!
                        edges.Clear();
                        toRemove.Clear();
                        for (int n = outputTriangleIndices.Count - 3; n >= 0; n -= 3)
                        {
                            //Go through each triangle, if it can be seen, delete it and use maintainEdge on its edges.
                            if (IsTriangleVisibleFromPoint(outputTriangleIndices, points, n, ref maximum))
                            {
                                //This triangle can see it!
                                //TODO: CONSIDER CONSISTENT WINDING HAPPYTIMES
                                MaintainEdge(outputTriangleIndices[n], outputTriangleIndices[n + 1], edges);
                                MaintainEdge(outputTriangleIndices[n], outputTriangleIndices[n + 2], edges);
                                MaintainEdge(outputTriangleIndices[n + 1], outputTriangleIndices[n + 2], edges);
                                //Because fast removals are being used, the order is very important.
                                //It's pulling indices in from the end of the list in order, and also ensuring
                                //that we never issue a removal order beyond the end of the list.
                                outputTriangleIndices.FastRemoveAt(n + 2);
                                outputTriangleIndices.FastRemoveAt(n + 1);
                                outputTriangleIndices.FastRemoveAt(n);

                            }
                        }
                        //Create new triangles.
                        for (int n = 0; n < edges.Count; n += 2)
                        {
                            //For each edge, create a triangle with the extreme point.
                            newTriangles.Add(edges[n]);
                            newTriangles.Add(edges[n + 1]);
                            newTriangles.Add(maxIndex);
                        }
                        //Only verify the windings of the new triangles.
                        VerifyWindings(newTriangles, points, ref insidePoint);
                        outputTriangleIndices.AddRange(newTriangles);
                        newTriangles.Clear();

                        //Remove all points from the outsidePoints if they are inside the polyhedron
                        RemoveInsidePoints(points, outputTriangleIndices, outsidePoints);

                        //The list has been significantly messed with, so restart the loop.
                        break;
                    }
                }
            }


            Pool.Return(ref outsidePoints);
            Pool.Return(ref edges);
            Pool.Return(ref toRemove);
            Pool.Return(ref newTriangles);

            return true;
        }

        private static void MaintainEdge(int a, int b, RawList<int> edges)
        {
            bool contained = false;
            int index = 0;
            for (int k = 0; k < edges.Count; k += 2)
            {
                if ((edges[k] == a && edges[k + 1] == b) || (edges[k] == b && edges[k + 1] == a))
                {
                    contained = true;
                    index = k;
                }
            }
            //If it isn't present, add it to the edge list.
            if (!contained)
            {
                edges.Add(a);
                edges.Add(b);
            }
            else
            {
                //If it is present, that means both edge-connected triangles were deleted now, so get rid of it.
                edges.FastRemoveAt(index + 1);
                edges.FastRemoveAt(index);
            }
        }

        private static int GetExtremePoint(ref float3 direction, RawList<float3> points, RawList<int> outsidePoints)
        {
            float maximumDot = -float.MaxValue;
            int extremeIndex = 0;
            for (int i = 0; i < outsidePoints.Count; ++i)
            {
                float dot = MathX.Dot(points.Elements[outsidePoints[i]], direction);
                
                if (dot > maximumDot)
                {
                    maximumDot = dot;
                    extremeIndex = i;
                }
            }
            return extremeIndex;
        }

        private static void GetExtremePoints(ref float3 direction, RawList<float3> points, out float maximumDot, out float minimumDot, out int maximumIndex, out int minimumIndex)
        {
            maximumIndex = 0;
            minimumIndex = 0;

            float dot = MathX.Dot(points.Elements[0], direction);
            minimumDot = dot;
            maximumDot = dot;
            for (int i = 1; i < points.Count; ++i)
            {
                dot = MathX.Dot(points.Elements[i], direction);

                if (dot > maximumDot)
                {
                    maximumDot = dot;
                    maximumIndex = i;
                }
                else if (dot < minimumDot)
                {
                    minimumDot = dot;
                    minimumIndex = i;
                }
            }
        }

        private static bool ComputeInitialTetrahedron(RawList<float3> points, RawList<int> outsidePointCandidates, RawList<int> triangleIndices, out float3 centroid)
        {
            centroid = default;

            //Find four points on the hull.
            //We'll start with using the x axis to identify two points on the hull.
            int a, b, c, d;
            float3 direction;
            //Find the extreme points along the x axis.
            float minimumX = float.MaxValue, maximumX = -float.MaxValue;
            int minimumXIndex = 0, maximumXIndex = 0;
            for (int i = 0; i < points.Count; ++i)
            {
                var v = points.Elements[i];
                if (v.x > maximumX)
                {
                    maximumX = v.x;
                    maximumXIndex = i;
                }
                else if (v.x < minimumX)
                {
                    minimumX = v.x;
                    minimumXIndex = i;
                }
            }
            a = minimumXIndex;
            b = maximumXIndex;
            //Check for redundancies..
            if (a == b)
                return false;

            //Now, use a second axis perpendicular to the two points we found.
            float3 ab = points.Elements[b] - points.Elements[a];
            direction = MathX.Cross(ab, float3.Up);

            if (direction.SqrMagnitude < EPSILON)
                direction = MathX.Cross(ab, float3.Right);

            float minimumDot, maximumDot;
            int minimumIndex, maximumIndex;
            GetExtremePoints(ref direction, points, out maximumDot, out minimumDot, out maximumIndex, out minimumIndex);
            //Compare the location of the extreme points to the location of the axis.
            float dot = MathX.Dot(direction, points.Elements[a]);
            
            //Use the point further from the axis.
            if (Math.Abs(dot - minimumDot) > Math.Abs(dot - maximumDot))
            {
                //In this case, we should use the minimum index.
                c = minimumIndex;
            }
            else
            {
                //In this case, we should use the maximum index.
                c = maximumIndex;
            }

            //Check for redundancies..
            if (a == c || b == c)
                return false;

            //Use a third axis perpendicular to the plane defined by the three unique points a, b, and c.
            float3 ac = points.Elements[c] - points.Elements[a];

            direction = MathX.Cross(ab, ac);

            GetExtremePoints(ref direction, points, out maximumDot, out minimumDot, out maximumIndex, out minimumIndex);
            //Compare the location of the extreme points to the location of the plane.
            dot = MathX.Dot(direction, points.Elements[a]);
            //Use the point further from the plane. 
            if (Math.Abs(dot - minimumDot) > Math.Abs(dot - maximumDot))
            {
                //In this case, we should use the minimum index.
                d = minimumIndex;
            }
            else
            {
                //In this case, we should use the maximum index.
                d = maximumIndex;
            }

            //Check for redundancies..
            if (a == d || b == d || c == d)
                return false;

            //Add the triangles.
            triangleIndices.Add(a);
            triangleIndices.Add(b);
            triangleIndices.Add(c);

            triangleIndices.Add(a);
            triangleIndices.Add(b);
            triangleIndices.Add(d);

            triangleIndices.Add(a);
            triangleIndices.Add(c);
            triangleIndices.Add(d);

            triangleIndices.Add(b);
            triangleIndices.Add(c);
            triangleIndices.Add(d);

            //The centroid is guaranteed to be within the convex hull.  It will be used to verify the windings of triangles throughout the hull process.
            centroid = points.Elements[a] + points.Elements[b] + points.Elements[c] + points.Elements[d];
            centroid *= 0.25f;

            for (int i = 0; i < triangleIndices.Count; i += 3)
            {
                var vA = points.Elements[triangleIndices.Elements[i]];
                var vB = points.Elements[triangleIndices.Elements[i + 1]];
                var vC = points.Elements[triangleIndices.Elements[i + 2]];

                //Check the signed volume of a parallelepiped with the edges of this triangle and the centroid.
                float3 cross;

                ab = vB - vA;
                ac = vC - vA;
                cross = MathX.Cross(ac, ab);

                float3 offset = vA - centroid;
                float volume = MathX.Dot(offset, cross);
                
                //This volume/cross product could also be used to check for degeneracy, but we already tested for that.
                if (Math.Abs(volume) < BIG_EPSILON)
                    return false;
                
                if (volume < 0)
                {
                    //If the signed volume is negative, that means the triangle's winding is opposite of what we want.
                    //Flip it around!
                    var temp = triangleIndices.Elements[i];
                    triangleIndices.Elements[i] = triangleIndices.Elements[i + 1];
                    triangleIndices.Elements[i + 1] = temp;
                }
            }

            //Points which belong to the tetrahedra are guaranteed to be 'in' the convex hull. Do not allow them to be considered.
            var tetrahedronIndices = Pool.BorrowRawList<int>();
            tetrahedronIndices.Add(a);
            tetrahedronIndices.Add(b);
            tetrahedronIndices.Add(c);
            tetrahedronIndices.Add(d);
            //Sort the indices to allow a linear time loop.
            Array.Sort(tetrahedronIndices.Elements, 0, 4);

            int tetrahedronIndex = 0;
            for (int i = 0; i < points.Count; ++i)
            {
                if (tetrahedronIndex < 4 && i == tetrahedronIndices[tetrahedronIndex])
                {
                    //Don't add a tetrahedron index. Now that we've found this index, though, move on to the next one.
                    ++tetrahedronIndex;
                }
                else
                {
                    outsidePointCandidates.Add(i);
                }
            }

            Pool.Return(ref tetrahedronIndices);

            return true;
        }

        private static void RemoveInsidePoints(RawList<float3> points, RawList<int> triangleIndices, RawList<int> outsidePoints)
        {
            var insidePoints = Pool.BorrowRawList<int>();
            //We're going to remove points from this list as we go to prune it down to the truly inner points.
            insidePoints.AddRange(outsidePoints);
            outsidePoints.Clear();

            for (int i = 0; i < triangleIndices.Count && insidePoints.Count > 0; i += 3)
            {
                //Compute the triangle's plane in point-normal representation to test other points against.
                float3 normal;
                FindNormal(triangleIndices, points, i, out normal);
                float3 p = points.Elements[triangleIndices.Elements[i]];

                for (int j = insidePoints.Count - 1; j >= 0; --j)
                {
                    //Offset from the triangle to the current point, tested against the normal, determines if the current point is visible
                    //from the triangle face.
                    float3 offset = points.Elements[insidePoints.Elements[j]] - p;

                    float dot = MathX.Dot(offset, normal);
                    
                    //If it's visible, then it's outside!
                    if (dot > 0)
                    {
                        //This point is known to be on the outside; put it on the outside!
                        outsidePoints.Add(insidePoints.Elements[j]);
                        insidePoints.FastRemoveAt(j);
                    }
                }
            }
            Pool.Return(ref insidePoints);
        }


        private static void FindNormal(RawList<int> indices, RawList<float3> points, int triangleIndex, out float3 normal)
        {
            var a = points.Elements[indices.Elements[triangleIndex]];
            float3 ab, ac;

            ab = points.Elements[indices.Elements[triangleIndex + 1]] - a;
            ac = points.Elements[indices.Elements[triangleIndex + 2]] - a;

            normal = MathX.Cross(ac, ab);
        }

        private static bool IsTriangleVisibleFromPoint(RawList<int> indices, RawList<float3> points, int triangleIndex, ref float3 point)
        {
            //Compute the normal of the triangle.
            var a = points.Elements[indices.Elements[triangleIndex]];
            float3 ab, ac;

            ab = points.Elements[indices.Elements[triangleIndex + 1]] - a;
            ac = points.Elements[indices.Elements[triangleIndex + 2]] - a;

            float3 normal = MathX.Cross(ac, ab);

            //Assume a consistent winding.  Check to see if the normal points at the point.
            float3 offset = point - a;
            float dot = MathX.Dot(offset, normal);
            
            return dot >= 0;
        }

        private static void VerifyWindings(RawList<int> newIndices, RawList<float3> points, ref float3 centroid)
        {
            //Go through every triangle.
            for (int k = 0; k < newIndices.Count; k += 3)
            {
                //Check if the triangle faces away or towards the centroid.

                if (IsTriangleVisibleFromPoint(newIndices, points, k, ref centroid))
                {
                    //If it's towards, flip the winding.
                    int temp = newIndices[k + 1];
                    newIndices[k + 1] = newIndices[k + 2];
                    newIndices[k + 2] = temp;
                }
            }
        }
    }
}
