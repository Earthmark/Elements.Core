using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public static class HalfHelper
    {
        public static DataTreeNode Save(this half v) => ((float)v).Save();
        public static half LoadHalf(this DataTreeNode node) => (half)node.LoadFloat();

        public static void Write(this BinaryWriter wr, half v) => wr.Write(Unsafe.As<half, ushort>(ref v));
        public static half ReadHalf(this BinaryReader rd)
        {
            var v = rd.ReadUInt16();
            return Unsafe.As<ushort, half>(ref v);
        }
    }
}
