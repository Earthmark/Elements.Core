using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public class DictionaryList<K,T> : IEnumerable<KeyValuePair<K, List<T>>>
    {
        Dictionary<K, List<T>> dictionary = new Dictionary<K, List<T>>();

        public int Count => dictionary.Count;

        public void Add(K key, T element)
        {
            if(!dictionary.TryGetValue(key, out List<T> list))
            {
                list = Pool.BorrowList<T>();
                dictionary.Add(key, list);
            }
            
            list.Add(element);
        }

        public bool Remove(K key, T element)
        {
            if(dictionary.TryGetValue(key, out List<T> list))
            {
                var removed = list.Remove(element);

                CheckRemoveList(key, list);

                return removed;
            }

            return false;
        }

        void CheckRemoveList(K key, List<T> list)
        {
            if (list.Count == 0)
            {
                dictionary.Remove(key);
                Pool.Return(ref list);
            }
        }

        public bool RemoveList(K key)
        {
            if (dictionary.TryGetValue(key, out List<T> list))
            {
                dictionary.Remove(key);
                Pool.Return(ref list);
                return true;
            }
            else
                return false;
        }

        public void Clear()
        {
            foreach (var e in dictionary)
            {
                var list = e.Value;
                Pool.Return(ref list);
            }

            dictionary.Clear();
        }

        public bool ContainsKey(K key) => dictionary.ContainsKey(key);

        public List<T> TryGetList(K key)
        {
            dictionary.TryGetValue(key, out List<T> list);
            return list;
        }

        public List<T> GetListOrCreate(K key)
        {
            if(!dictionary.TryGetValue(key, out List<T> list))
            {
                list = new List<T>();
                dictionary.Add(key, list);
            }

            return list;
        }

        public bool TryTakeOne(K key, out T element)
        {
            var list = TryGetList(key);

            if(list != null && list.Count > 0)
            {
                element = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);

                CheckRemoveList(key, list);

                return true;
            }
            else
            {
                element = default;
                return false;
            }
        }

        public IEnumerator<KeyValuePair<K, List<T>>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<K, List<T>>>)dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<K, List<T>>>)dictionary).GetEnumerator();
        }
    }
}
