using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public readonly struct SingleItemEnumerable<T> : IEnumerable<T>
    {
        public readonly T item;

        public struct Enumerator : IEnumerator<T>
        {
            public T Current => (_isOnItem == true) ? _item : default;
            object IEnumerator.Current => Current;

            public Enumerator(T item)
            {
                _item = item;
                _isOnItem = null;
            }

            T _item;
            bool? _isOnItem;

            public void Dispose()
            {
                _item = default;
            }

            public bool MoveNext()
            {
                if(_isOnItem == null)
                {
                    _isOnItem = true;
                    return true;
                }

                _isOnItem = false;
                return false;
            }

            public void Reset()
            {
                _isOnItem = null;
            }
        }

        public SingleItemEnumerable(T item)
        {
            this.item = item;
        }

        public Enumerator GetEnumerator() => new Enumerator(item);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static class SingleItemEnumerableHelper
    {
        public static SingleItemEnumerable<T> AsSingleItemEnumerable<T>(this T item) => new SingleItemEnumerable<T>(item);
    }
}
