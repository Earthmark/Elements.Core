using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Elements.Core; using Elements.Data;
using BepuPhysics;
using BepuPhysics.Trees;
using BepuUtilities;
using BepuUtilities.Memory;
using System.Numerics;

namespace Elements.Core
{
    public class BoundingBoxTree : IDisposable
    {
        struct EnumeratorCallbacks : IBreakableForEach<int>
        {
            public Action<int> handleOverlap;

            public bool LoopBody(int i)
            {
                handleOverlap(i);
                return true;
            }
        }

        struct EnumeratorBreakableCallbacks : IBreakableForEach<int>
        {
            public Func<int, bool> handleOverlap;

            public bool LoopBody(int i) => handleOverlap(i);
        }

        struct OverlapCallbacks : IOverlapHandler
        {
            public Action<int, int> handleOverlap;

            public void Handle(int indexA, int indexB) => handleOverlap(indexA, indexB);
        }

        public int Count => _tree.LeafCount;

        BufferPool _pool;
        Tree _tree;
        int _index;

        public BoundingBoxTree()
        {
            _pool = new BufferPool();
            _tree = new Tree(_pool);
        }

        public int CreateNode(Elements.Core.BoundingBox bounds)
        {
            var bepuBounds = bounds.ToBepu();
            return _tree.Add(ref bepuBounds, _pool);
        }

        public int RemoveNode(int index) => _tree.RemoveAt(index);

        public unsafe void UpdateNode(int index, Elements.Core.BoundingBox bounds)
        {
            GetBoundsPointers(index, ref _tree, out _, out var min, out var max);

            *min = bounds.min;
            *max = bounds.max;
        }

        public unsafe void UpdateNodeAndRefit(int index, Elements.Core.BoundingBox bounds)
        {
            GetBoundsPointers(index, ref _tree, out var nodeIndex, out var min, out var max);

            *min = bounds.min;
            *max = bounds.max;

            _tree.RefitForNodeBoundsChange(nodeIndex);
        }

        public void Refit() => _tree.Refit();

        public void RefitAndRefine() => _tree.RefitAndRefine(_pool, _index++);

        public void GetSelfOverlaps(Action<int, int> callback)
        {
            var handler = new OverlapCallbacks() { handleOverlap = callback };
            _tree.GetSelfOverlaps(ref handler);
        }

        public void GetOverlaps(Elements.Core.BoundingBox bounds, Action<int> callback)
        {
            var handler = new EnumeratorCallbacks() { handleOverlap = callback };
            var bepuBounds = bounds.ToBepu();
            _tree.GetOverlaps(bepuBounds, ref handler);
        }

        public void GetOverlaps(Elements.Core.BoundingBox bounds, Func<int, bool> callback)
        {
            var handler = new EnumeratorBreakableCallbacks() { handleOverlap = callback };
            var bepuBounds = bounds.ToBepu();
            _tree.GetOverlaps(bepuBounds, ref handler);
        }

        public void GetOverlaps(BoundingBoxTree other, Action<int, int> callback)
        {
            var handler = new OverlapCallbacks() { handleOverlap = callback };
            _tree.GetOverlaps(ref other._tree, ref handler);
        }

        public void GetSelfOverlaps<T>(ref T handler) where T : struct, IOverlapHandler => _tree.GetSelfOverlaps(ref handler);
        public void GetOverlaps<T>(BoundingBoxTree other, ref T handler) where T : struct, IOverlapHandler => _tree.GetOverlaps(ref other._tree, ref handler);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe void GetBoundsPointers(int index, ref Tree tree, out int nodeIndex, out Vector3* minPointer, out Vector3* maxPointer)
        {
            ref var leaf = ref tree.Leaves[index];
            nodeIndex = leaf.NodeIndex;
            var nodeChild = (&tree.Nodes.Memory[nodeIndex].A) + leaf.ChildIndex;
            minPointer = &nodeChild->Min;
            maxPointer = &nodeChild->Max;
        }

        public void Dispose()
        {
            _tree.Dispose(_pool);
            _pool.Clear();
            _pool = default;
        }
    }
}
