using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Elements.Core
{
    public static class SizeOfHelper
    {
        static ConcurrentDictionary<Type, int> _sizeCache = new ConcurrentDictionary<Type, int>();

        public static int UnmanagedSizeOf(this Type type)
        {
            if (_sizeCache.TryGetValue(type, out var size))
                return size;

            try
            {
                if (type == typeof(sbyte) || type == typeof(byte))
                    return 1;

                if (type == typeof(ushort) || type == typeof(short))
                    return 2;

                if (type == typeof(int) || type == typeof(uint))
                    return 4;

                if (type == typeof(long) || type == typeof(ulong))
                    return 8;

                if (type == typeof(float))
                    return sizeof(float);

                if (type == typeof(double))
                    return sizeof(double);

                if (type == typeof(decimal))
                    return sizeof(decimal);

                if (type == typeof(bool))
                    return sizeof(bool);

                if (type == typeof(char))
                    return sizeof(char);

                unsafe
                {
                    if (type == typeof(IntPtr) || type == typeof(UIntPtr))
                        return sizeof(IntPtr);

                    if (type == typeof(string))
                        return 8;
                }

                if (type.IsPrimitive)
                    throw new NotImplementedException("Unsupported primitive: " + type);

                size = 0;

                foreach(var f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    size += f.FieldType.UnmanagedSizeOf();

                _sizeCache.TryAdd(type, size);

                return size;
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception when determining size of type {type}", ex);
            }
        }
    }
}
