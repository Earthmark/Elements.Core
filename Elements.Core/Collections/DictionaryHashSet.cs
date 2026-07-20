using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public class DictionaryHashSet<K,T> : IEnumerable<KeyValuePair<K, HashSet<T>>>
    {
        Dictionary<K, HashSet<T>> dictionary = new Dictionary<K, HashSet<T>>();

        public int Count => dictionary.Count;

        public bool Add(K key, T element)
        {
            if(!dictionary.TryGetValue(key, out var set))
            {
                set = Pool.BorrowHashSet<T>();
                dictionary.Add(key, set);
            }

            return set.Add(element);
        }

        public bool Remove(K key, T element)
        {
            if (!dictionary.TryGetValue(key, out var set))
                return false;

            var removed = set.Remove(element);

            if (removed)
                CheckRemoveSet(key, set);

            return removed;
        }

        public bool RemoveSet(K key)
        {
            if (!dictionary.TryGetValue(key, out var set))
                return false;

            Pool.Return(ref set);
            dictionary.Remove(key);

            return true;
        }
        public void Clear()
        {
            foreach(var e in dictionary)
            {
                var set = e.Value;
                Pool.Return(ref set);
            }

            dictionary.Clear();
        }

        public bool ContainsKey(K key) => dictionary.ContainsKey(key);

        public HashSet<T> TryGetSet(K key)
        {
            dictionary.TryGetValue(key, out var set);
            return set;
        }

        void CheckRemoveSet(K key, HashSet<T> set)
        {
            if (set.Count > 0)
                return;

            dictionary.Remove(key);
            Pool.Return(ref set);
        }

        public IEnumerator<KeyValuePair<K, HashSet<T>>> GetEnumerator() => ((IEnumerable<KeyValuePair<K, HashSet<T>>>)dictionary).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
