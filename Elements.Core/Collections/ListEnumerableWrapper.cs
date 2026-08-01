using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    // Wraps a list without allocations to give direct access to the list's struct enumerator, to prevent allocations

    [Obsolete("Likely no longer needed with current .net versions")]
    public readonly struct ListEnumerableWrapper<T> : IEnumerable<T>
    {
        static List<T> emptyList;

        readonly List<T> list;

        [Obsolete("Likely no longer needed with current .net versions")]
        public ListEnumerableWrapper(List<T> list)
        {
            if(list == null)
            {
                if (emptyList == null)
                    emptyList = new List<T>(0);

                this.list = emptyList;
            }
            else
                this.list = list;
        }

        public List<T>.Enumerator GetEnumerator() => list.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [Obsolete("Likely no longer needed with current .net versions")]
        public static implicit operator ListEnumerableWrapper<T>(List<T> list) => new ListEnumerableWrapper<T>(list);
    }

    [Obsolete("Likely no longer needed with current .net versions")]
    public readonly struct SlimListEnumerableWrapper<T> : IEnumerable<T>
    {
        readonly SlimList<T> list;

        [Obsolete("Likely no longer needed with current .net versions")]
        public SlimListEnumerableWrapper(SlimList<T> list)
        {
            this.list = list;
        }

        public SlimList<T>.Enumerator GetEnumerator() => list.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [Obsolete("Likely no longer needed with current .net versions")]
        public static implicit operator SlimListEnumerableWrapper<T>(SlimList<T> list) => new SlimListEnumerableWrapper<T>(list);
    }
}
