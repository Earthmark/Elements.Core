using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public static class ParallelEx
    {
        public delegate void LoopBatchHandler(int startInclusive, int endExclusive);

        public static void BatchedFor(int fromInclusive, int toExclusive, int batchSize, LoopBatchHandler handler)
        {
            int start = fromInclusive;
            int end = toExclusive;
            bool reverse = false;

            if (start > end)
            {
                (start, end) = (end, start);
                reverse = true;
            }

            int totalCount = end - start;
            int batchCount = (totalCount + batchSize - 1) / batchSize;

            Parallel.For(0, batchCount, batchIndex =>
            {
                if (reverse)
                    batchIndex = batchCount - batchIndex - 1;

                int batchStart = batchIndex * batchSize;
                int batchEnd = MathX.Min(batchStart + batchSize, totalCount);

                // Offset by start
                batchStart += start;
                batchEnd += start;

                handler(batchStart, batchEnd);
            });
        }
    }
}
