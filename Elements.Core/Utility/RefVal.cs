using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public class RefVal<T> where T : struct
    {
        public T Value { get; set; }

        public RefVal() : this(default(T))
        {

        }

        public RefVal(T init)
        {
            Value = init;
        }

        public static implicit operator T(RefVal<T> refVal) { return refVal.Value; }

        public override string ToString() => Value.ToString();
    }
}
