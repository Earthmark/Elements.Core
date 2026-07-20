using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public class EmptyEnumerator<T> : IEnumerable<T>
    {
        public static readonly EmptyEnumerator<T> Instance = new EmptyEnumerator<T>();

        static Enumerator Singleton = new Enumerator();

        class Enumerator : IEnumerator<T>
        {
            public T Current => default;
            object IEnumerator.Current => null;

            public void Dispose() { }

            public bool MoveNext() => false;

            public void Reset() { }
        }

        public IEnumerator<T> GetEnumerator() => Singleton;

        IEnumerator IEnumerable.GetEnumerator() => Singleton;
    }
}
