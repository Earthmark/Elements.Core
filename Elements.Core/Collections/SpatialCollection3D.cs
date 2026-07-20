using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Core
{
    public class SpatialCollection3D<T> : IDisposable
    {
        public int Count => tree.Count;

        T[] items;
        Dictionary<T, int> itemToIndex = new Dictionary<T, int>();
        BoundingBoxTree tree = new BoundingBoxTree();

        #region MANAGING ITEMS

        public void Add(T item, BoundingBox bounds)
        {
            var index = tree.CreateNode(bounds);

            try
            {
                EnsureCapacity(index);

                items[index] = item;
                itemToIndex.Add(item, index);
            }
            catch
            {
                // Avoid lingering items when this fails (e.g. when item already exists)
                HandleItemRemoval(index, tree.RemoveNode(index));

                throw;
            }
        }

        public void UpdateBounds(T item, BoundingBox bounds)
        {
            if (!UpdateBoundsIfExists(item, bounds))
                throw new ArgumentException("Given item is not part of this collection");
        }

        public bool UpdateBoundsIfExists(T item, BoundingBox bounds)
        {
            if (!itemToIndex.TryGetValue(item, out var index))
                return false;

            tree.UpdateNode(index, bounds);

            return true;
        }

        public void UpdateBoundsAndRefit(T item, BoundingBox bounds)
        {
            if (!itemToIndex.TryGetValue(item, out var index))
                throw new ArgumentException("Given item is not part of this collection");

            tree.UpdateNodeAndRefit(index, bounds);
        }

        public bool Remove(T item)
        {
            if (!itemToIndex.TryGetValue(item, out var index))
                return false;

            HandleItemRemoval(index, tree.RemoveNode(index));

            return true;
        }

        public void Refit() => tree.Refit();
        public void RefitAndRefine() => tree.RefitAndRefine();

        #endregion

        #region QUERYING

        public int GetOverlaps(BoundingBox bounds, ICollection<T> results, Predicate<T> filter)
        {
            int count = 0;
            tree.GetOverlaps(bounds, index =>
            {
                var item = items[index];

                if (!filter(item))
                    return;

                count++;
                results.Add(item);
            });

            return count;
        }

        public int GetOverlaps(BoundingBox bounds, ICollection<T> results)
        {
            int count = 0;

            tree.GetOverlaps(bounds, index =>
            {
                count++;
                results.Add(items[index]);
            });

            return count;
        }

        public List<T> GetOverlaps(BoundingBox bounds)
        {
            var list = new List<T>();
            GetOverlaps(bounds, list);
            return list;
        }

        #endregion

        void HandleItemRemoval(int removedIndex, int swapIndex)
        {
            // We always remove the mapping of the removed item
            itemToIndex.Remove(items[removedIndex]);

            if (swapIndex < 0)
            {
                items[removedIndex] = default;
                return;
            }

            // Remove the mapping for the swapped item and place it under the new one
            var swappedItem = items[swapIndex];

            // Update the index of the swapped item to the new one
            itemToIndex[swappedItem] = removedIndex;

            // Swap the actual item
            items[removedIndex] = items[swapIndex];
            items[swapIndex] = default;
        }

        void EnsureCapacity(int index)
        {
            var currentLength = items?.Length ?? 0;

            if (currentLength > index)
                return;

            var newItems = new T[MathX.Max(index + 1, currentLength * 2)];

            if (currentLength > 0)
                Array.Copy(items, newItems, currentLength);

            items = newItems;
        }

        public void Dispose()
        {
            tree.Dispose();
            tree = null;

            items = null;
            itemToIndex = null;

            GC.SuppressFinalize(this);
        }

        ~SpatialCollection3D()
        {
            if (tree == null)
                return;

            Dispose();
        }
    }
}
