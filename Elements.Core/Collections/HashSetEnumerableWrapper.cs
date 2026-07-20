using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public struct HashSetEnumerableWrapper<T> : IEnumerable<T>
    {
        static HashSet<T> emptyHashSet;

        readonly HashSet<T> hashset;

        public HashSetEnumerableWrapper(HashSet<T> hashset)
        {
            if(hashset == null)
            {
                if (emptyHashSet == null)
                    emptyHashSet = new HashSet<T>();

                this.hashset = emptyHashSet;
            }
            else
                this.hashset = hashset;
        }

        public HashSet<T>.Enumerator GetEnumerator() => hashset.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static implicit operator HashSetEnumerableWrapper<T>(HashSet<T> hashset) => new HashSetEnumerableWrapper<T>(hashset);
    }
}
