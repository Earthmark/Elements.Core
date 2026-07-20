using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Elements.Core
{
    public static class RandomExtensions
    {
        public static double GetElapsedMilliseconds(this Stopwatch watch)
        {
            return (watch.ElapsedTicks / (double)Stopwatch.Frequency) * 1000.0;
        }
    }
}
