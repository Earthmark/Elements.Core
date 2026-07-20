using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public delegate Type LegacyTypeSeacher(string typeName, string assemblyName);

    public static class LegacyTypeParser
    {
        internal static string LegacyMatch = Encoding.UTF8.GetString(new byte[] { 0x4E, 0x65, 0x6F, 0x73 });
        internal static string LegacyMatchCloud = Encoding.UTF8.GetString(new byte[] { 0x43, 0x6C, 0x6F, 0x75, 0x64, 0x58, 0x2E, 0x53, 0x68, 0x61, 0x72, 0x65, 0x64 });
        internal static string LegacyMatchCore = Encoding.UTF8.GetString(new byte[] { 0x42, 0x61, 0x73, 0x65, 0x58 });
        internal static string LegacyMatchAssets = Encoding.UTF8.GetString(new byte[] { 0x43, 0x6F, 0x64, 0x65, 0x58 });
        internal static string LegacyMatchColor = LegacyMatchCore + ".color";

        public static Type TryParse(string str, LegacyTypeSeacher typeSearcher) => TryParse(new StringSegment(str), typeSearcher);

        public static Type TryParse(StringSegment typename, LegacyTypeSeacher typeSearcher)
        {
            var genericArguments = new SlimList<Type>();

            var bracketStart = typename.IndexOf("[");

            StringSegment name;
            StringSegment remainder;

            if(bracketStart >= 0)
            {
                var bracketEnd = typename.LastIndexOf("]");

                // Invalid type, bail out
                if (bracketEnd < 0)
                    return null;

                var argumentsStr = typename.Slice(bracketStart + 1, (bracketEnd - bracketStart - 1));

                // Split and parse the inner types
                int nestLevel = 0;
                int typeStart = -1;

                for(int i = 0; i < argumentsStr.length; i++)
                {
                    var ch = argumentsStr[i];

                    if (ch == '[')
                    {
                        if (nestLevel == 0)
                            typeStart = i + 1;

                        nestLevel++;
                    }
                    else if(ch == ']')
                    {
                        nestLevel--;

                        if (nestLevel == 0)
                        {
                            // We haven't found a start bracket, something is wrong
                            if (typeStart < 0)
                                return null;

                            // We found the end of this type segment. Slice it out and parse
                            var typeArgStr = argumentsStr.Slice(typeStart, i - typeStart);
                            var typeArg = TryParse(typeArgStr, typeSearcher);

                            // Type arguments MUST be parsed here! If this fails, we don't support the whole type
                            if (typeArg == null)
                                return null;

                            genericArguments.Add(typeArg);
                        }
                        else if (nestLevel < 0)
                            return null; // something else is wrong
                    }
                    else if(nestLevel == 0)
                    {
                        if (char.IsWhiteSpace(ch))
                            continue;

                        if (ch == ',')
                            continue;

                        // We only expect comma's or whitespaces, but nothing else in between
                        return null;
                    }
                }

                // Get the raw typename for the root type
                name = typename.Slice(0, bracketStart); // The name will be before the first bracket starts
                remainder = typename.Slice(bracketEnd + 1); // Get the remainder, which might have the rest of the stuff

                // If there's remainder, it'll start with a comma, since we technically parsed the first bit
                var commaIndex = remainder.IndexOf(",");

                if (commaIndex >= 0)
                    remainder = remainder.Slice(commaIndex + 1);
            }
            else
                typename.SplitAround(",", out name, out remainder); // There's no brackets, just split around the next comma

            // Process the raw typename, in case it specifies assembly and such
            remainder.SplitAround(",", out var assembly, out remainder);
            remainder.SplitAround(",", out var version, out remainder);
            remainder.SplitAround(",", out var culture, out remainder);
            remainder.SplitAround(",", out var token, out remainder);

            var nameStr = name.Trim().ToString();
            var assemblyStr = assembly.Trim().ToString();

            nameStr = nameStr.Replace(LegacyMatchCore, "Elements.Core");
            nameStr = nameStr.Replace(LegacyMatchAssets, "Elements.Assets");

            var containerType = TryResolveLegacyAlias(nameStr) ?? typeSearcher(nameStr, assemblyStr);

            if (containerType == null)
                return null;

            if (genericArguments.Count == 0)
                return containerType;

            try
            {
                return containerType.MakeGenericType(genericArguments.ToArray());
            }
            catch
            {
                return null;
            }
        }

        public static Type TryResolveLegacyAlias(string name)
        {
            switch (name)
            {
                case "System.Boolean": return typeof(bool);

                case "System.Byte": return typeof(byte);
                case "System.UInt16": return typeof(ushort);
                case "System.UInt32": return typeof(uint);
                case "System.UInt64": return typeof(ulong);

                case "System.SByte": return typeof(sbyte);
                case "System.Int16": return typeof(short);
                case "System.Int32": return typeof(int);
                case "System.Int64": return typeof(long);

                case "System.Single": return typeof(float);
                case "System.Double": return typeof(double);

                case "System.Decimal": return typeof(decimal);

                case "System.Char": return typeof(char);
                case "System.String": return typeof(string);

                case "System.Uri": return typeof(Uri);

                case "Elements.Core.bool2": return typeof(bool2);
                case "Elements.Core.bool3": return typeof(bool3);
                case "Elements.Core.bool4": return typeof(bool4);

                case "Elements.Core.int2": return typeof(int2);
                case "Elements.Core.int3": return typeof(int3);
                case "Elements.Core.int4": return typeof(int4);

                case "Elements.Core.uint2": return typeof(uint2);
                case "Elements.Core.uint3": return typeof(uint3);
                case "Elements.Core.uint4": return typeof(uint4);

                case "Elements.Core.long2": return typeof(long2);
                case "Elements.Core.long3": return typeof(long3);
                case "Elements.Core.long4": return typeof(long4);

                case "Elements.Core.ulong2": return typeof(ulong2);
                case "Elements.Core.ulong3": return typeof(ulong3);
                case "Elements.Core.ulong4": return typeof(ulong4);

                case "Elements.Core.float2": return typeof(float2);
                case "Elements.Core.float3": return typeof(float3);
                case "Elements.Core.float4": return typeof(float4);

                case "Elements.Core.double2": return typeof(double2);
                case "Elements.Core.double3": return typeof(double3);
                case "Elements.Core.double4": return typeof(double4);

                case "Elements.Core.floatQ": return typeof(floatQ);
                case "Elements.Core.doubleQ": return typeof(doubleQ);

                case "Elements.Core.float2x2": return typeof(float2x2);
                case "Elements.Core.float3x3": return typeof(float3x3);
                case "Elements.Core.float4x4": return typeof(float4x4);

                case "Elements.Core.double2x2": return typeof(double2x2);
                case "Elements.Core.double3x3": return typeof(double3x3);
                case "Elements.Core.double4x4": return typeof(double4x4);

                case "System.DateTime": return typeof(DateTime);
                case "System.TimeSpan": return typeof(TimeSpan);

                case "Elements.Core.color": return typeof(color);
                case "Elements.Core.colorX": return typeof(colorX);

                case "Elements.Core.RefID":
                case "FrooxEngine.RefID":
                    return typeof(RefID);

                case "System.half": return typeof(half);

                case "Elements.Core.Rect": return typeof(Rect);
                case "Elements.Core.BoundingBox": return typeof(BoundingBox);
                case "Elements.Core.dummy": return typeof(dummy);
                case "Elements.Core.dummy`1": return typeof(dummy<>);

                case "System.Object": return typeof(object);
                case "System.Nullable`1": return typeof(Nullable<>);
                case "System.Type": return typeof(Type);
                case "System.Guid": return typeof(Guid);
                case "System.DateTimeKind": return typeof(DateTimeKind);
                case "System.DayOfWeek": return typeof(DayOfWeek);
                case "System.StringComparison": return typeof(StringComparison);
                case "System.IFormatProvider": return typeof(IFormatProvider);
                case "System.Globalization.NumberStyles": return typeof(NumberStyles);
                case "System.Globalization.CultureInfo": return typeof(CultureInfo);
                case "System.Net.HttpStatusCode": return typeof(HttpStatusCode);
                case "System.Threading.Tasks.Task": return typeof(Task);
                case "System.Threading.Tasks.Task`1": return typeof(Task<>);

                case "System.Action":    return typeof(Action);
                case "System.Action`1":  return typeof(Action<>);
                case "System.Action`2":  return typeof(Action<,>);
                case "System.Action`3":  return typeof(Action<,,>);
                case "System.Action`4":  return typeof(Action<,,,>);
                case "System.Action`5":  return typeof(Action<,,,,>);
                case "System.Action`6":  return typeof(Action<,,,,,>);
                case "System.Action`7":  return typeof(Action<,,,,,,>);
                case "System.Action`8":  return typeof(Action<,,,,,,,>);
                case "System.Action`9":  return typeof(Action<,,,,,,,,>);
                case "System.Action`10": return typeof(Action<,,,,,,,,,>);
                case "System.Action`11": return typeof(Action<,,,,,,,,,,>);
                case "System.Action`12": return typeof(Action<,,,,,,,,,,,>);
                case "System.Action`13": return typeof(Action<,,,,,,,,,,,,>);
                case "System.Action`14": return typeof(Action<,,,,,,,,,,,,,>);
                case "System.Action`15": return typeof(Action<,,,,,,,,,,,,,,>);
                case "System.Action`16": return typeof(Action<,,,,,,,,,,,,,,,>);

                case "System.Func`1": return  typeof(Func<>);
                case "System.Func`2": return  typeof(Func<,>);
                case "System.Func`3": return  typeof(Func<,,>);
                case "System.Func`4": return  typeof(Func<,,,>);
                case "System.Func`5": return  typeof(Func<,,,,>);
                case "System.Func`6": return  typeof(Func<,,,,,>);
                case "System.Func`7": return  typeof(Func<,,,,,,>);
                case "System.Func`8": return  typeof(Func<,,,,,,,>);
                case "System.Func`9": return  typeof(Func<,,,,,,,,>);
                case "System.Func`10": return typeof(Func<,,,,,,,,,>);
                case "System.Func`11": return typeof(Func<,,,,,,,,,,>);
                case "System.Func`12": return typeof(Func<,,,,,,,,,,,>);
                case "System.Func`13": return typeof(Func<,,,,,,,,,,,,>);
                case "System.Func`14": return typeof(Func<,,,,,,,,,,,,,>);
                case "System.Func`15": return typeof(Func<,,,,,,,,,,,,,,>);
                case "System.Func`16": return typeof(Func<,,,,,,,,,,,,,,,>);
                case "System.Func`17": return typeof(Func<,,,,,,,,,,,,,,,,>);

                case "System.Predicate`1": return typeof(Predicate<>);

                default:
                    return null;
            }
        }
    }
}
