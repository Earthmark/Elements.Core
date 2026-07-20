using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public readonly struct EnumerableWrapper<T> : IEnumerable<T>
    {
        public bool IsNull => _enumeratorGetter == null;

        readonly Func<IEnumerator<T>> _enumeratorGetter;

        public EnumerableWrapper(Func<IEnumerator<T>> enumeratorGetter)
        {
            _enumeratorGetter = enumeratorGetter;
        }

        public IEnumerator<T> GetEnumerator() => _enumeratorGetter();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public readonly struct EnumerableWrapper<T, E> : IEnumerable<T>
        where E : IEnumerator<T>
    {
        public bool IsNull => _enumeratorGetter == null;

        readonly Func<E> _enumeratorGetter;

        public EnumerableWrapper(Func<E> enumeratorGetter)
        {
            _enumeratorGetter = enumeratorGetter;
        }

        public E GetEnumerator() => _enumeratorGetter();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
