using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public struct ValuePair<A,B>
    {
        public ValuePair(A first, B second)
        {
            this.First = first;
            this.Second = second;
        }

        public A First { get; private set; }
        public B Second { get; private set; }
    }

    public class BiDictionary<A,B> : IEnumerable<ValuePair<A,B>>
    {
        public struct Enumerator : IEnumerator<ValuePair<A, B>>
        {
            Dictionary<A, B>.Enumerator dictEnumerator;

            public Enumerator(Dictionary<A, B>.Enumerator dictEnumerator)
            {
                this.dictEnumerator = dictEnumerator;
            }

            public ValuePair<A, B> Current
            {
                get
                {
                    var pair = dictEnumerator.Current;
                    return new ValuePair<A, B>(pair.Key, pair.Value);
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose() => dictEnumerator.Dispose();
            public bool MoveNext() => dictEnumerator.MoveNext();
            public void Reset()
            {
                var temp = (IEnumerator)dictEnumerator;
                temp.Reset();
                dictEnumerator = (Dictionary<A, B>.Enumerator)temp;
            }
        }

        Dictionary<A, B> a2b = new Dictionary<A, B>();
        Dictionary<B, A> b2a = new Dictionary<B, A>();

        public void Add(A first, B second)
        {
            if (a2b.ContainsKey(first) || b2a.ContainsKey(second))
                throw new ArgumentException("One of the dictionaries already contains one of the elements");

            a2b.Add(first, second);
            b2a.Add(second, first);
        }

        public int Count { get { return a2b.Count; } }

        public A this[B second]
        {
            get { return b2a[second]; }
        }

        public B this[A first]
        {
            get { return a2b[first]; }
        }

        public B GetSecond(A first)
        {
            return a2b[first];
        }

        public A GetFirst(B second)
        {
            return b2a[second];
        }

        public bool TryGetFirst(B second, out A first)
        {
            return b2a.TryGetValue(second, out first);
        }

        public bool TryGetSecond(A first, out B second)
        {
            return a2b.TryGetValue(first, out second);
        }

        public bool ContainsFirst(A first)
        {
            return a2b.ContainsKey(first);
        }

        public bool ContainsSecond(B second)
        {
            return b2a.ContainsKey(second);
        }

        public bool RemoveByFirst(A first)
        {
            // get second
            if (TryGetSecond(first, out B second))
            {
                // remove them both
                a2b.Remove(first);
                b2a.Remove(second);

                return true;
            }

            return false;
        }

        public bool RemoveBySecond(B second)
        {
            // get second
            if (TryGetFirst(second, out A first))
            {
                // remove them both
                a2b.Remove(first);
                b2a.Remove(second);

                return true;
            }

            return false;
        }

        public void Clear()
        {
            a2b.Clear();
            b2a.Clear();
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(a2b.GetEnumerator());
        }

        IEnumerator<ValuePair<A, B>> IEnumerable<ValuePair<A, B>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
