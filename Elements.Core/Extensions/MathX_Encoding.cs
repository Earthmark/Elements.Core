using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Core
{
    public static partial class MathX
    {
        // Following are from the Gaussian Splat Unity project

        // Based on https://fgiesen.wordpress.com/2009/12/13/decoding-morton-codes/
        // Insert two 0 bits after each of the 21 low bits of x
        static ulong MortonPart1By2(ulong x)
        {
            x &= 0x1fffff;
            x = (x ^ (x << 32)) & 0x1f00000000ffffUL;
            x = (x ^ (x << 16)) & 0x1f0000ff0000ffUL;
            x = (x ^ (x << 8)) & 0x100f00f00f00f00fUL;
            x = (x ^ (x << 4)) & 0x10c30c30c30c30c3UL;
            x = (x ^ (x << 2)) & 0x1249249249249249UL;
            return x;
        }

        // Encode three 21-bit integers into 3D Morton order
        public static ulong MortonEncode21bit3(in uint3 v)
        {
            return (MortonPart1By2(v.z) << 2) | (MortonPart1By2(v.y) << 1) | MortonPart1By2(v.x);
        }

        public static uint2 DecodeMorton2D_16x16(uint t)
        {
            t = (t & 0xFF) | ((t & 0xFE) << 7); // -EAFBGCHEAFBGCHD
            t &= 0x5555;                        // -E-F-G-H-A-B-C-D
            t = (t ^ (t >> 1)) & 0x3333;        // --EF--GH--AB--CD
            t = (t ^ (t >> 2)) & 0x0f0f;        // ----EFGH----ABCD
            return new uint2(t & 0xF, t >> 8);  // --------EFGHABCD
        }
    }
}
