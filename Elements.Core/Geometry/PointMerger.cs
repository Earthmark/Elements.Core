using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

// ported from BEPU

namespace Elements.Core
{
    public static class PointMerger
    {
        /// <summary>
        /// Removes redundant points.  Two points are redundant if they occupy the same hash grid cell.
        /// </summary>
        /// <param name="points">List of points to prune.</param>
        /// <param name="cellSize">Size of cells to determine redundancy.</param>
        public static void RemoveRedundantPoints(RawList<float3> points, double cellSize = 0.001)
        {
            var set = Pool.BorrowHashSet<int3>();

            for (int i = points.Count - 1; i >= 0; --i)
            {
                var point = points.Elements[i];
                var cell = MathX.FloorToInt(point / cellSize);

                if (!set.Add(cell))
                    points.FastRemoveAt(i);

                //TODO: Consider adding adjacent cells to guarantee that a point on the border between two cells will still detect the presence
                //of a point on the opposite side of that border.
            }

            Pool.Return(ref set);
        }

        public static void RemoveRedundantPoints(RawList<Vector3> points, double cellSize = 0.001)
        {
            var set = Pool.BorrowHashSet<int3>();

            for (int i = points.Count - 1; i >= 0; --i)
            {
                var point = points.Elements[i];
                var cell = MathX.FloorToInt((float3)point / cellSize);

                if (!set.Add(cell))
                    points.FastRemoveAt(i);

                //TODO: Consider adding adjacent cells to guarantee that a point on the border between two cells will still detect the presence
                //of a point on the opposite side of that border.
            }

            Pool.Return(ref set);
        }

        public static void GetMergedPoints(RawList<float3> points, IList<float3> mergedPoints, double cellSize = 0.001)
        {
            var set = Pool.BorrowHashSet<int3>();

            for(int i = 0; i < points.Count; i++)
            {
                var point = points.Elements[i];
                var cell = MathX.FloorToInt(point / cellSize);

                if (set.Add(cell))
                    mergedPoints.Add(point);
            }

            Pool.Return(ref set);
        }

        public static int GetMergedPoints(RawList<float3> points, Dictionary<int, int> remappedIndicies, double cellSize = 0.001)
        {
            var set = Pool.BorrowDictionary<int3, int>();

            for (int i = 0; i < points.Count; i++)
            {
                var point = points.Elements[i];
                var cell = MathX.FloorToInt(point / cellSize);

                if(set.TryGetValue(cell, out int firstIndex))
                    remappedIndicies.Add(i, firstIndex);
                else
                    set.Add(cell, i);
            }

            int uniquePoints = set.Count;

            Pool.Return(ref set);

            return uniquePoints;
        }
    }
}
