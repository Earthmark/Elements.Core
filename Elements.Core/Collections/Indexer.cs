using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public class Indexer<T>
    {
        readonly Func<int, T> getter;
        readonly Action<int, T> setter;

        public Indexer(Func<int,T> getter, Action<int,T> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public T this[int index]
        {
            get { return getter(index); }
            set { setter(index, value); }
        }
    }
}
