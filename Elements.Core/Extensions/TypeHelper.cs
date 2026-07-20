using Elements.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public static class TypeHelper
    {
        static BiDictionary<string, Type> aliasTable = new BiDictionary<string, Type>();
        static Dictionary<string, Type> extraAliases = new Dictionary<string, Type>();

        static TypeHelper()
        {
            aliasTable.Add("void", typeof(void));

            aliasTable.Add("bool", typeof(bool));
            aliasTable.Add("string", typeof(string));
            aliasTable.Add("char", typeof(char));
            aliasTable.Add("object", typeof(object));
            aliasTable.Add("Type", typeof(Type));

            aliasTable.Add("DateTime", typeof(DateTime));
            aliasTable.Add("TimeSpan", typeof(TimeSpan));

            aliasTable.Add("Uri", typeof(Uri));

            extraAliases.Add("uri", typeof(Uri));
            extraAliases.Add("URL", typeof(Uri));
            extraAliases.Add("Url", typeof(Uri));
            extraAliases.Add("url", typeof(Uri));

            aliasTable.Add("byte", typeof(byte));
            aliasTable.Add("ushort", typeof(ushort));
            aliasTable.Add("uint", typeof(uint));
            aliasTable.Add("ulong", typeof(ulong));

            aliasTable.Add("sbyte", typeof(sbyte));
            aliasTable.Add("short", typeof(short));
            aliasTable.Add("int", typeof(int));
            aliasTable.Add("long", typeof(long));

            aliasTable.Add("half", typeof(half));
            aliasTable.Add("float", typeof(float));
            aliasTable.Add("double", typeof(double));
            aliasTable.Add("decimal", typeof(decimal));

            // vectors
            aliasTable.Add("half2", typeof(half2));
            aliasTable.Add("half3", typeof(half3));
            aliasTable.Add("half4", typeof(half4));

            aliasTable.Add("float2", typeof(float2));
            aliasTable.Add("float3", typeof(float3));
            aliasTable.Add("float4", typeof(float4));

            aliasTable.Add("double2", typeof(double2));
            aliasTable.Add("double3", typeof(double3));
            aliasTable.Add("double4", typeof(double4));

            aliasTable.Add("int2", typeof(int2));
            aliasTable.Add("int3", typeof(int3));
            aliasTable.Add("int4", typeof(int4));

            aliasTable.Add("uint2", typeof(uint2));
            aliasTable.Add("uint3", typeof(uint3));
            aliasTable.Add("uint4", typeof(uint4));

            aliasTable.Add("long2", typeof(long2));
            aliasTable.Add("long3", typeof(long3));
            aliasTable.Add("long4", typeof(long4));

            aliasTable.Add("ulong2", typeof(ulong2));
            aliasTable.Add("ulong3", typeof(ulong3));
            aliasTable.Add("ulong4", typeof(ulong4));

            aliasTable.Add("bool2", typeof(bool2));
            aliasTable.Add("bool3", typeof(bool3));
            aliasTable.Add("bool4", typeof(bool4));

            // quaternions
            aliasTable.Add("floatQ", typeof(floatQ));
            aliasTable.Add("doubleQ", typeof(doubleQ));

            // color
            aliasTable.Add("color", typeof(color));
            aliasTable.Add("color32", typeof(color32));

            aliasTable.Add("colorX", typeof(colorX));
            extraAliases.Add("colorx", typeof(colorX));

            // matrices
            aliasTable.Add("float2x2", typeof(float2x2));
            aliasTable.Add("float3x3", typeof(float3x3));
            aliasTable.Add("float4x4", typeof(float4x4));

            aliasTable.Add("double2x2", typeof(double2x2));
            aliasTable.Add("double3x3", typeof(double3x3));
            aliasTable.Add("double4x4", typeof(double4x4));

            aliasTable.Add("RefID", typeof(RefID));

            extraAliases.Add("refid", typeof(RefID));
            extraAliases.Add("RefId", typeof(RefID));

            aliasTable.Add("Rect", typeof(Rect));
            extraAliases.Add("rect", typeof(Rect));
        }


        public static Type FindType(string typename)
        {
            var type = Type.GetType(typename, false, true);

            if (type != null)
                return type;

            // search all assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typename, false, true);

                if (type != null)
                    return type;
            }

            return null;
        }

        public static Type TryResolveAlias(string alias)
        {
            if(alias.EndsWith("?"))
            {
                var nestedType = TryResolveAlias(alias.Substring(0, alias.Length - 1));

                if (nestedType == null)
                    return null;

                if (nestedType.IsClass)
                    return null;

                return typeof(Nullable<>).MakeGenericType(nestedType);
            }

            if (aliasTable.TryGetSecond(alias, out Type type))
                return type;

            if (extraAliases.TryGetValue(alias, out type))
                return type;

            return null;
        }

        public static string TryGetAlias(Type type)
        {
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var nestedAlias = TryGetAlias(type.GetGenericArguments()[0]);

                if (nestedAlias == null)
                    return null;

                return nestedAlias + "?";
            }

            if (aliasTable.TryGetFirst(type, out string alias))
                return alias;

            return null;
        }

        public static Type GetNewType(this TypeReplacement typeReplacement, Type inputType)
        {
            if (typeReplacement.NewType != null)
            {
                var replacement = typeReplacement.NewType;

                // If the replacement is generic type definition, but input isn't, copy the generic arguments
                if(inputType.IsGenericType && replacement.IsGenericType &&
                    !inputType.IsGenericTypeDefinition && replacement.IsGenericTypeDefinition)
                {
                    replacement = replacement.MakeGenericType(inputType.GetGenericArguments());
                }

                return replacement;
            }

            var sourceTypename = inputType.Name;
            var replaceTypename = sourceTypename.Replace(typeReplacement.ReplaceSource, typeReplacement.ReplaceTarget);

            // make sure to replace the name of the type itself and not any other parts (like the namespace)
            var newTypename = inputType.FullName.Replace(sourceTypename, replaceTypename);
            var newType = TypeHelper.FindType(newTypename);

            if (newType == null)
                throw new Exception($"Replacement type doesn't exist.\nInputType: {inputType}\nReplacementType: {newTypename}");

            return newType;
        }
    }
}
