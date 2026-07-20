using Elements.Quantity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elements.Core
{
    // Adapted from: https://github.com/aras-p/UnityGaussianSplatting/blob/main/package/Editor/Utils/KMeansClustering.cs
    public static class KMeansClustering
    {
        const int BATCH_SIZE = 1024;
        const string LOCALE_HEADER = "K-Means Clustering";

        public static bool Calculate(int dim, Memory<float> inputData, int batchSize, float passesOverData,
            Memory<float> outClusterMeans, Memory<int> outDataLabels,
            IProgressIndicator progress = null, CancellationToken cancel = default)
        {
            if(dim < 1)
                throw new ArgumentOutOfRangeException(nameof(dim), "Dimensions must be >= 1");
            if (batchSize < 1)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be >= 1");
            if (passesOverData < 0.0001f)
                throw new ArgumentOutOfRangeException(nameof(passesOverData), "Passes over data must be larger positive number");

            if(inputData.Length % dim != 0)
                throw new ArgumentException("Input data length must be multiple of dimensions", nameof(inputData));
            if(outClusterMeans.Length % dim != 0)
                throw new ArgumentException("Cluster means length must be multiple of dimensions", nameof(outClusterMeans));

            var dataSize = inputData.Length / dim;
            var k = outClusterMeans.Length / dim;

            if (k < 1)
                throw new ArgumentOutOfRangeException("cluster count length must be at least 1");

            if(dataSize < k)
                throw new ArgumentOutOfRangeException("Input data length must be at least equal to cluster count");

            if(dataSize != outDataLabels.Length)
                throw new ArgumentException("Data labels length must be equal to input data length", nameof(outDataLabels));

            batchSize = MathX.Min(dataSize, batchSize);

            uint rngState = 1;

            // Do initial cluster placement
            int initBatchSize = 10 * k;

            const int kInitAttempts = 3;

            if (!InitializeCentroids(dim, inputData.Span, initBatchSize, ref rngState, kInitAttempts, outClusterMeans.Span,
                progress, cancel))
                return false;

            var counts = new float[k];
            var batchPoints = new float[batchSize * dim];
            var batchClusters = new int[batchSize];

            bool cancelled = false;

            for(float calcDone = 0f, calcLimit = dataSize * passesOverData; calcDone < calcLimit; calcDone += batchSize)
            {
                if (cancel.IsCancellationRequested)
                {
                    cancelled = true;
                    break;
                }

                progress?.UpdateProgress(0.3f + calcDone / calcLimit * 0.4f, LOCALE_HEADER, "Computing batches");

                // generate a batch of random input points
                MakeRandomBatch(dim, inputData.Span, ref rngState, batchPoints);

                AssignClusters(batchSize, dim, 0, batchPoints, outClusterMeans, batchClusters, default);

                UpdateCentroids(batchSize, dim, batchClusters, batchPoints, counts, outClusterMeans.Span);
            }

            // finally find out closest clusters for all input points
            if(!cancelled)
            {
                const int kAssignBatchCount = 256 * 1024;

                for (int i = 0; i < dataSize; i += kAssignBatchCount)
                {
                    if (cancel.IsCancellationRequested)
                    {
                        cancelled = true;
                        break;
                    }

                    progress?.UpdateProgress(0.7f + (float)i / dataSize * 0.3f, LOCALE_HEADER, "Finding closest clusters");

                    AssignClusters(MathX.Min(kAssignBatchCount, dataSize - i), dim, i, inputData, outClusterMeans, outDataLabels, null);
                }
            }

            return !cancelled;
        }

        static void UpdateCentroids(int batchSize, int dim, ReadOnlySpan<int> batchClusters, ReadOnlySpan<float> batchPoints,
            Span<float> counts, Span<float> clusters)
        {
            for (int i = 0; i < batchSize; ++i)
            {
                int clusterIndex = batchClusters[i];
                counts[clusterIndex]++;

                float alpha = 1.0f / counts[clusterIndex];

                for (int j = 0; j < dim; ++j)
                    clusters[clusterIndex * dim + j] = MathX.LerpUnclamped(clusters[clusterIndex * dim + j], batchPoints[i * dim + j], alpha);
            }
        }

        static bool InitializeCentroids(int dim, Span<float> inputData, int initBatchSize,
            ref uint rngState, int initAttempts, Span<float> outClusters,
            IProgressIndicator progress = null, CancellationToken cancel = default)
        {
            int k = outClusters.Length / dim;
            int dataSize = inputData.Length / dim;
            initBatchSize = MathX.Min(initBatchSize, dataSize);

            var centroidBatch = new float[initBatchSize * dim];
            var validationBatch = new float[initBatchSize * dim];

            MakeRandomBatch(dim, inputData, ref rngState, centroidBatch);
            MakeRandomBatch(dim, inputData, ref rngState, validationBatch);

            var tmpIndices = new int[initBatchSize];
            var tmpDistances = new float[initBatchSize];
            var curCentroids = new float[k * dim];

            float minDistSum = float.MaxValue;

            bool cancelled = false;

            for (int ia = 0; ia < initAttempts; ++ia)
            {
                if(cancel.IsCancellationRequested)
                {
                    cancelled = true;
                    break;
                }

                progress?.UpdateProgress((float)ia / initAttempts * 0.3f, LOCALE_HEADER, "Initializing centroids");

                KMeansPlusPlus(dim, k, centroidBatch, curCentroids, tmpDistances, ref rngState);

                AssignClusters(initBatchSize, dim, 0, validationBatch, curCentroids, tmpIndices, tmpDistances);

                float distSum = 0;

                foreach (var d in tmpDistances)
                    distSum += d;

                // is this centroid better?
                if (distSum < minDistSum)
                {
                    minDistSum = distSum;
                    curCentroids.CopyTo(outClusters);
                }
            }

            return !cancelled;
        }

        static void MakeRandomBatch(int dim, Span<float> inputData, ref uint rngState, Span<float> outBatch)
        {
            uint dataSize = (uint)(inputData.Length / dim);
            int batchSize = outBatch.Length / dim;

            uint seed = pcg_random(ref rngState);

            HashSet<int> picked = Pool.BorrowHashSet<int>();

            while (picked.Count < batchSize)
            {
                int index = (int)(pcg_hash(seed++) % dataSize);

                if (!picked.Contains(index))
                {
                    CopyElement(dim, inputData, index, outBatch, picked.Count);
                    picked.Add(index);
                }
            }

            Pool.Return(ref picked);
        }

        static void KMeansPlusPlus(int dim, int k, Memory<float> data, Memory<float> means, Memory<float> minDistSq, ref uint rngState)
        {
            int dataSize = data.Length / dim;

            var taken = new BitArray(dataSize);

            // Select first mean randomly
            int pointIndex = (int)(pcg_random(ref rngState) % dataSize);
            taken.Set(pointIndex, true);
            CopyElement(dim, data.Span, pointIndex, means.Span, 0);

            // For each point: closest squared distance to the picked point
            {
                ParallelEx.BatchedFor(0, dataSize, 1024, (from, to) =>
                {
                    var _minDistSq = minDistSq.Span;
                    var _data = data.Span;
                    var _means = means.Span;

                    for (int index = from; index < to; index++)
                    {
                        if (index == pointIndex)
                            continue;

                        _minDistSq[index] = DistanceSquared(dim, _data, index, _means, 0);
                    }
                });
            }

            int sumBatches = (dataSize + BATCH_SIZE - 1) / BATCH_SIZE;

            var partialSums = new float[sumBatches];
            int resultCount = 1;

            while (resultCount < k)
            {
                // Find total sum of distances of not yet taken points
                float distSqTotal = 0;

                {
                    Parallel.For(0, sumBatches, batchIndex =>
                    {
                        int iStart = MathX.Min(batchIndex * BATCH_SIZE, dataSize);
                        int iEnd = MathX.Min((batchIndex + 1) * BATCH_SIZE, dataSize);
                        float sum = 0;

                        var _minDistSq = minDistSq.Span;

                        for (int i = iStart; i < iEnd; ++i)
                        {
                            if (taken[i])
                                continue;

                            sum += _minDistSq[i];
                        }

                        partialSums[batchIndex] = sum;
                    });

                    for (int i = 0; i < sumBatches; ++i)
                    {
                        distSqTotal += partialSums[i];
                        partialSums[i] = distSqTotal;
                    }
                }

                // Pick a non-taken point, with a probability proportional
                // to distance: points furthest from any cluster are picked more.
                {
                    float rval = pcg_hash_float(rngState + (uint)resultCount, distSqTotal);
                    pointIndex = PickPointIndex(dataSize, partialSums, taken, minDistSq.Span, rval);
                }

                // Take this point as a new cluster mean
                taken.Set(pointIndex, true);
                CopyElement(dim, data.Span, pointIndex, means.Span, resultCount);
                ++resultCount;

                if (resultCount < k)
                {
                    // Update distances of the points: since it tracks closest one,
                    // calculate distance to the new cluster and update if smaller.
                    var meanIndex = resultCount - 1;

                    ParallelEx.BatchedFor(0, dataSize, 256, (start, end) =>
                    {
                        var _data = data.Span;
                        var _means = means.Span;
                        var _minDistSq = minDistSq.Span;

                        for (int index = start; index < end; index++)
                        {
                            if (taken[index])
                                continue;

                            float distSq = DistanceSquared(dim, _data, index, _means, meanIndex);
                            _minDistSq[index] = MathX.Min(_minDistSq[index], distSq);
                        }
                    });
                }
            }
        }

        static int PickPointIndex(int dataSize, Span<float> partialSums, BitArray taken, Span<float> minDistSq, float rval)
        {
            // Skip batches until we hit the ones that might have value to pick from: binary search for the batch
            int indexL = 0;
            int indexR = partialSums.Length;
            while (indexL < indexR)
            {
                int indexM = (indexL + indexR) / 2;
                if (partialSums[indexM] < rval)
                    indexL = indexM + 1;
                else
                    indexR = indexM;
            }
            float acc = 0.0f;
            if (indexL > 0)
            {
                acc = partialSums[indexL - 1];
            }

            // Now search for the needed point
            int pointIndex = -1;
            for (int i = indexL * BATCH_SIZE; i < dataSize; ++i)
            {
                if (taken[i])
                    continue;

                acc += minDistSq[i];
                if (acc >= rval)
                {
                    pointIndex = i;
                    break;
                }
            }

            // If we have not found a point, pick the last available one
            if (pointIndex < 0)
            {
                for (int i = dataSize - 1; i >= 0; --i)
                {
                    if (taken[i])
                        continue;

                    pointIndex = i;
                    break;
                }
            }

            if (pointIndex < 0)
                pointIndex = 0;

            return pointIndex;
        }

        static unsafe float DistanceSquared(int dim, ReadOnlySpan<float> a, int aIndex, ReadOnlySpan<float> b, int bIndex)
        {
            aIndex *= dim;
            bIndex *= dim;

            float d = 0;
            
            for (var i = 0; i < dim; ++i)
            {
                float delta = a[aIndex + i] - b[bIndex + i];
                d += delta * delta;
            }

            return d;
        }

        static void AssignClusters(int count, int dim, int indexOffset, ReadOnlyMemory<float> data, ReadOnlyMemory<float> means,
            Memory<int> clusters, Memory<float> distances)
        {
            int meansCount = means.Length / dim;

            Parallel.For(0, count, index =>
            {
                var _data = data.Span;
                var _means = means.Span;
                var _clusters = clusters.Span;
                var _distances = distances.Span;

                index += indexOffset;

                float minDist = float.MaxValue;
                int minIndex = 0;

                for (int i = 0; i < meansCount; ++i)
                {
                    float dist = DistanceSquared(dim, _data, index, _means, i);

                    if (dist < minDist)
                    {
                        minIndex = i;
                        minDist = dist;
                    }
                }

                _clusters[index] = minIndex;

                if (!distances.IsEmpty)
                    _distances[index] = minDist;
            });
        }

        static void CopyElement(int dimensions, Span<float> src, int srcIndex, Span<float> dst, int dstIndex)
        {
            dst = dst.Slice(dstIndex * dimensions, dimensions);
            src = src.Slice(srcIndex * dimensions, dimensions);

            src.CopyTo(dst);
        }

        // https://www.reedbeta.com/blog/hash-functions-for-gpu-rendering/
        static uint pcg_hash(uint input)
        {
            uint state = input * 747796405u + 2891336453u;
            uint word = ((state >> (int)((state >> 28) + 4u)) ^ state) * 277803737u;
            return (word >> 22) ^ word;
        }

        static float pcg_hash_float(uint input, float upTo)
        {
            uint val = pcg_hash(input);
            float f = MathX.ReinterpretAsFloat(0x3f800000 | (val >> 9)) - 1.0f;
            return f * upTo;
        }

        static uint pcg_random(ref uint rng_state)
        {
            uint state = rng_state;
            rng_state = rng_state * 747796405u + 2891336453u;
            uint word = ((state >> (int)((state >> 28) + 4u)) ^ state) * 277803737u;
            return (word >> 22) ^ word;
        }
    }
}
