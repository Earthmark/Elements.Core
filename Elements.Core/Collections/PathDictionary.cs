using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public class PathDictionary<K, T> : IEnumerable<KeyValuePair<IReadOnlyList<K>, T>>
    {
        class Node
        {
            public T item;
            public bool itemOccupied;

            public PathDictionary<K, T> children;
        }

        Dictionary<K, Node> _items;

        public PathDictionary()
        {
            _items = new Dictionary<K, Node>();
        }

        public int KeyCount => _items.Count;

        public void Add(IReadOnlyList<K> path, T item)
        {
            if (!TryAdd(path, item))
                throw new InvalidOperationException("PathDictionary already contains item with given key");
        }

        public bool TryAdd(IReadOnlyList<K> path, T item)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Count == 0)
                throw new ArgumentException("Path cannot be empty");

            return SetRecursive(path, 0, item, false);
        }

        public bool TryGetValue(IReadOnlyList<K> path, out T item)
        {
            var result = GetRecursive(path, 0);

            if(result.occupied)
            {
                item = result.item;
                return true;
            }
            else
            {
                item = default;
                return false;
            }
        }

        public bool Remove(IReadOnlyList<K> path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (path.Count == 0)
                throw new ArgumentException("Path cannot be empty");

            return RemoveRecursive(path, 0);
        }

        public void Clear()
        {
            // TODO!!! Is this actually necessary to do recursively? We'll need to do this if we implement
            // pooling, so they can be recycled, but otherwise we can just clear the top level list
            foreach (var item in _items)
                item.Value.children?.Clear();

            _items.Clear();
        }

        public bool ContainsPath(IReadOnlyList<K> path) => GetRecursive(path, 0).occupied;

        public T this[IReadOnlyList<K> path]
        {
            get
            {
                var result = GetRecursive(path, 0);

                if (result.occupied)
                    return result.item;
                else
                    throw new KeyNotFoundException();
            }

            set => SetRecursive(path, 0, value, true);
        }

        bool SetRecursive(IReadOnlyList<K> path, int pathIndex, T item, bool allowOverwrite)
        {
            var key = path[pathIndex];

            if(!_items.TryGetValue(key, out var node))
            {
                node = new Node();
                _items.Add(key, node);
            }

            if(pathIndex == path.Count - 1)
            {
                // We've arrived at the node where we want to add the item, add it here

                if (node.itemOccupied && !allowOverwrite)
                    return false;                

                node.item = item;
                node.itemOccupied = true;

                return true;
            }
            else
            {
                if (node.children == null)
                    node.children = new PathDictionary<K, T>();

                return node.children.SetRecursive(path, pathIndex + 1, item, allowOverwrite);
            }
        }

        (T item, bool occupied) GetRecursive(IReadOnlyList<K> path, int pathIndex)
        {
            var key = path[pathIndex];

            if (!_items.TryGetValue(key, out var node))
                return (default, false);

            if(pathIndex == path.Count - 1)
            {
                // We've arrived at the node we want to check
                if (!node.itemOccupied)
                    return (default, false);
                else
                    return (node.item, true);
            }
            else
            {
                if (node.children == null)
                    return (default, false);

                return node.children.GetRecursive(path, pathIndex + 1);
            }
        }

        bool RemoveRecursive(IReadOnlyList<K> path, int pathIndex)
        {
            var key = path[pathIndex];

            if (!_items.TryGetValue(key, out var node))
                return false;

            bool removed;

            if (pathIndex == path.Count - 1)
            {
                if (!node.itemOccupied)
                    return false;

                node.item = default;
                node.itemOccupied = false;

                removed = true;
            }
            else if (node.children == null)
                return false;
            else
                removed = node.children.RemoveRecursive(path, pathIndex + 1);

            if (removed)
            {
                // Check if the node is empty, in which case we can remove it completely
                if (node.children == null || node.children.KeyCount == 0)
                    _items.Remove(key);
            }

            return removed;
        }

        #region ENUMERABLE

        public IEnumerator<KeyValuePair<IReadOnlyList<K>, T>> GetEnumerator()
        {
            var list = Pool.BorrowList<K>();

            try
            {
                foreach (var item in EnumerateRecursive(list))
                    yield return item;
            }
            finally
            {
                Pool.Return(ref list);
            }
        }

        IEnumerable<KeyValuePair<IReadOnlyList<K>, T>> EnumerateRecursive(List<K> path)
        {
            int index = path.Count;
            path.Add(default);

            foreach(var group in _items)
            {
                path[index] = group.Key;

                if (group.Value.itemOccupied)
                    yield return new KeyValuePair<IReadOnlyList<K>, T>(path, group.Value.item);

                if (group.Value.children != null)
                    foreach (var subitem in group.Value.children.EnumerateRecursive(path))
                        yield return subitem;
            }

            path.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
