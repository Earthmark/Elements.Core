using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public class LockCounter
    {
        object _lock = new object();

        public int Count { get; private set; }

        public void Increment()
        {
            lock (_lock)
                Count++;
        }

        public void Decrement()
        {
            lock (_lock)
                Count--;
        }
    }
}
