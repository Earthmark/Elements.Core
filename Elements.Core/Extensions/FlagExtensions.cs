using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Elements.Core
{
    public static class FlagExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetFlag(this byte flags, int index) => ((flags >> index) & 1U) == 1U;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFlag(ref this byte flags, int index, bool value)
        {
            if (value)
                flags = (byte)(flags | (1U << index));
            else
                flags = (byte)(flags & ~(1U << index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetFlag(this ushort flags, int index) => ((flags >> index) & 1U) == 1U;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFlag(ref this ushort flags, int index, bool value)
        {
            if (value)
                flags = (ushort)(flags | (1U << index));
            else
                flags = (ushort)(flags & ~(1U << index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetFlag(this uint flags, int index) => ((flags >> index) & 1U) == 1U;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFlag(ref this uint flags, int index, bool value)
        {
            if (value)
                flags = (flags | (1U << index));
            else
                flags = (flags & ~(1U << index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetFlag(this ulong flags, int index) => ((flags >> index) & 1UL) == 1UL;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFlag(ref this ulong flags, int index, bool value)
        {
            if (value)
                flags = (flags | (1UL << index));
            else
                flags = (flags & ~(1UL << index));
        }
    }
}
