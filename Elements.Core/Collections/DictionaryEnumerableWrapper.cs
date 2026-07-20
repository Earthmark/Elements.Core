using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    // Wraps a dictionary without allocations to give direct access to the list's struct enumerator, to prevent allocations

    public readonly struct DictionaryEnumerableWrapper<K, T> : IEnumerable<KeyValuePair<K, T>>
    {
        static Dictionary<K, T> emptyDict;

        readonly Dictionary<K, T> dict;

        public DictionaryEnumerableWrapper(Dictionary<K, T> dict)
        {
            if (dict == null)
            {
                if (emptyDict == null)
                    emptyDict = new Dictionary<K, T>(0);

                this.dict = emptyDict;
            }
            else
                this.dict = dict;
        }

        public Dictionary<K, T>.Enumerator GetEnumerator() => dict.GetEnumerator();

        IEnumerator<KeyValuePair<K, T>> IEnumerable<KeyValuePair<K, T>>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator DictionaryEnumerableWrapper<K, T>(Dictionary<K, T> dict) => new DictionaryEnumerableWrapper<K, T>(dict);
    }
}