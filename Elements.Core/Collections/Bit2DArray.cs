using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public class Bit2DArray
    {
        public int2 Size { get; private set; }

        BitArray _baseBitArray;

        public Bit2DArray(int2 size, bool defaultValue = false)
        {
            Size = size;

            _baseBitArray = new BitArray(size.x * size.y, defaultValue);
        }

        public bool this[int x, int y]
        {
            get
            {
                CheckCoordinate(x, y);

                return _baseBitArray.Get(x + y * Size.x);
            }

            set
            {
                CheckCoordinate(x, y);

                _baseBitArray.Set(x + y * Size.x, value);
            }
        }

        void CheckCoordinate(int x, int y)
        {
            if (x < 0 || x >= Size.x)
                throw new ArgumentOutOfRangeException("x");

            if (y < 0 || y >= Size.y)
                throw new ArgumentOutOfRangeException("y");
        }
    }
}
