using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Elements.Core;

public static class ReflectionExtensions
{
    public const int MAX_VALUE_SIZE = 4096;

    public static bool IsValidGenericType(this Type type, bool validForInstantiation)
    {
        if (type == null)
            return false;

        // TODO!!! Find way to generalize this? There is an issue with Mono runtime, where if it's too big,
        // making a generic instance will just explode things and crash the runtime
        if (type.IsSphericalHarmonicsType())
        {
            if (type.SphericalHarmonicSize() > MAX_VALUE_SIZE)
                return false;
        }

        if (type.ContainsGenericParameters)
            return !validForInstantiation;

        var field = type.GetProperty("IsValidGenericType", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

        // doesn't provide custom validation, should be ok
        if (field == null)
            return true;

        var result = field.GetValue(null);

        if (result is bool)
            return (bool)result;

        // it's a wrong type?
        return false;
    }

    public static IEnumerable<FieldInfo> EnumerateAllInstanceFields(this Type type)
    {
        if (type == typeof(object))
            yield break;

        foreach (var f in type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
            | BindingFlags.DeclaredOnly))
            yield return f;

        // repeat for parent
        foreach (var f in type.BaseType.EnumerateAllInstanceFields())
            yield return f;
    }

    public static IEnumerable<MethodInfo> EnumerateAllInstanceMethods(this Type type)
    {
        if (type == typeof(object))
            yield break;

        foreach (var m in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
            | BindingFlags.DeclaredOnly))
            yield return m;

        // repeat for parent
        foreach (var m in type.BaseType.EnumerateAllInstanceMethods())
            yield return m;
    }

    public static IEnumerable<Type> EnumerateDirectInterfaces(this Type type)
    {
        var selfInterfaces = type.GetInterfaces();

        if (selfInterfaces == null || selfInterfaces.Length == 0)
            yield break;

        var baseSelfInterfaces = type.BaseType?.GetInterfaces().ToHashSet();

        foreach (var @interface in selfInterfaces)
            if (!(baseSelfInterfaces?.Contains(@interface) ?? false))
                yield return @interface;
    }

    public static IEnumerable<Type> EnumerateInterfacesRecursively(this Type type)
    {
        foreach (var t in type.GetInterfaces())
        {
            yield return t;

            // enumerate the interfaces of the interface itself
            foreach (var i in t.EnumerateInterfacesRecursively())
                yield return i;
        }
    }

    public static bool InheritsFrom(this Type type, Type baseType)
    {
        if (type.BaseType == baseType)
            return true; // inherits from this exact type
        if (type.BaseType == typeof(Object))
            return false; // reached the top, return false
                          // not at the top yet, but didn't find the base type yet, search higher
        return InheritsFrom(type.BaseType, baseType);
    }

    public static bool InheritsFromGeneric(this Type type, Type genericBaseType)
    {
        // reached the top, simply return false
        if (type.BaseType == typeof(Object))
            return false;

        // check if the base type is a generic one and then compare against its generic type definition
        var baseType = type.BaseType;
        Type gBaseType = null;
        if (baseType.IsGenericType)
            gBaseType = baseType.GetGenericTypeDefinition();

        if (gBaseType == genericBaseType)
            return true;

        // not at the top yet, but didn't find the generic base type yet, try higher
        return InheritsFromGeneric(baseType, genericBaseType);
    }

    public static bool OverridesMethod(this Type type, string methodName, Type methodOrigin)
    {
        var info = type.GetMethod(methodName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic
            | BindingFlags.Instance | BindingFlags.Public);

        return info.DeclaringType != methodOrigin;
    }

    public static Type FindGenericBaseClass(this Type type, Type genericBase)
    {
        if (type == null || type == typeof(object))
            return null;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == genericBase)
            return type;

        return FindGenericBaseClass(type.BaseType, genericBase);
    }

    public static void SetStaticField(this Type type, string fieldName, object value)
    {
        var info = type.GetField(fieldName, BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic);

        info.SetValue(null, value);
    }

    public static Type[] GetDirectGenericArguments(this Type type)
    {
        if (!type.IsGenericType)
            throw new ArgumentException($"Type {type} isn't a generic type!");

        var allArguments = type.GetGenericArguments();

        if (type.DeclaringType != null)
        {
            var parentCount = type.DeclaringType.GetGenericArguments().Length;

            // All of them are declared in the parent
            if (allArguments.Length == parentCount)
                return null;

            // We must slice the array
            var slice = new Type[allArguments.Length - parentCount];

            for (int i = 0; i < slice.Length; i++)
                slice[i] = allArguments[i + parentCount];
        }

        // They're all owned by this
        return allArguments;
    }

    public readonly struct NestedTypeEntry
    {
        public readonly Type type;
        public readonly Type[] directGenericArguments;

        public NestedTypeEntry(Type type, Type[] directGenericArguments)
        {
            this.type = type;
            this.directGenericArguments = directGenericArguments;
        }

        public override string ToString()
        {
            if (directGenericArguments == null)
                return $"Type: {type.Name}";
            else
                return $"Type: {type.Name}. Direct Arguments: {string.Join(", ", directGenericArguments.Select(t => t.Name))}";
        }
    }

    public static List<NestedTypeEntry> DecomposeGenericNestedType(this Type type)
    {
        if (!type.IsGenericType)
            throw new ArgumentException($"Type {type} isn't a generic type!");

        var entries = new List<NestedTypeEntry>();

        var args = type.GetGenericArguments();

        void Process(Type t, ref int argCount)
        {
            // Process all parents first
            if (t.DeclaringType != null)
                Process(t.DeclaringType, ref argCount);

            // We consumed all of the generic arguments
            var ownArgs = t.GetGenericArguments();

            var newArgCount = ownArgs.Length - argCount;

            if (newArgCount == 0)
                entries.Add(new NestedTypeEntry(t, null));
            else if (newArgCount == args.Length)
            {
                entries.Add(new NestedTypeEntry(t, args));
                argCount += ownArgs.Length;
            }
            else
            {
                var subArgs = new Type[newArgCount];

                for (int i = 0; i < subArgs.Length; i++)
                    subArgs[i] = args[argCount + i];

                argCount += subArgs.Length;

                entries.Add(new NestedTypeEntry(t, subArgs));
            }
        }

        // We need all entries to be generic type definitions for the decomposition
        if (!type.IsGenericTypeDefinition)
            type = type.GetGenericTypeDefinition();

        int argCount = 0;

        Process(type, ref argCount);

        return entries;
    }

    // This finds the start of array declaration for the major type, ignoring any array declarations that might be part
    // of the inner types (e.g. generic arguments). For this we search from the end
    static int FindArrayDeclarationStart(string typename)
    {
        int earliestFound = -1;

        for (int i = typename.Length - 1; i > 0; i--)
        {
            var ch = typename[i];

            if (char.IsWhiteSpace(ch) || ch == ']' || ch == ',')
                continue;

            if (ch == '[')
                earliestFound = i;
            else
                break; // if we found anything else - e.g. letter and other symbol, we stop
        }

        return earliestFound;
    }

    public static string FormatType(this Type type, Func<Type, string> typeFormatter, string open = "<", string close = ">", string nested = "+")
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType();
            var rank = type.GetArrayRank();

            var typeStr = elementType.FormatType(typeFormatter, open, close, nested);
            var arrayStr = $"[{new string(',', rank - 1)}]";

            // We must insert the array before all others, because of how array types are deconstructed - which actually goes in reverse
            var index = FindArrayDeclarationStart(typeStr);

            if (index < 0)
                return typeStr + arrayStr; // there's none, so we can just tack it at the end of it

            // Insert it before all the other array declarations
            return typeStr.Insert(index, arrayStr);
        }

        if (type.IsGenericParameter)
            return typeFormatter(type);

        // If it's not generic, things are pretty simple, we just run this recursively
        if (!type.IsGenericType)
        {
            var name = typeFormatter(type);

            if (type.DeclaringType != null)
                name = type.DeclaringType.FormatType(typeFormatter, open, close, nested) + nested + name;

            return name;
        }

        if (type.IsNullable())
        {
            // We can just return directly. Nullable type itself isn't part of any nested class, which means we
            // don't have to worry about that and check it
            var baseType = Nullable.GetUnderlyingType(type);

            if (baseType == null || baseType.IsGenericParameter)
                return $"Nullable{open}{close}";

            return baseType.FormatType(typeFormatter, open, close, nested) + "?";
        }

        // We must decompose the type, in case it's a nested type because of how generic arguments are
        // handled with those. This will get us direct generic arguments at each level
        var decomposed = type.DecomposeGenericNestedType();

        var builder = new StringBuilder();

        for (int i = 0; i < decomposed.Count; i++)
        {
            var entry = decomposed[i];

            var name = typeFormatter(entry.type);

            var tickIndex = name.IndexOf("`");

            if (tickIndex >= 0)
                name = name.Substring(0, tickIndex);

            builder.Append(name);

            if (entry.directGenericArguments != null)
            {
                builder.Append(open);

                for (int n = 0; n < entry.directGenericArguments.Length; n++)
                {
                    builder.Append(entry.directGenericArguments[n].FormatType(typeFormatter, open, close, nested));

                    if (n < entry.directGenericArguments.Length - 1)
                        builder.Append(",");
                }

                builder.Append(close);
            }

            if (i < decomposed.Count - 1)
                builder.Append(nested);
        }

        return builder.ToString();
    }

    public static string GetNiceFullName(this Type type, string open = "<", string close = ">", string nested = "+",
        bool includeGenericParameters = true)
    {
        return type.FormatType(t =>
        {
            if (!includeGenericParameters && t.IsGenericParameter)
                return "";

            var alias = TypeHelper.TryGetAlias(t);

            // We do not include namespaces for aliases - those are plain aliases!
            if (alias != null)
                return alias;

            // We only include namespace if we are not in the nested type
            if (t.DeclaringType == null && t.Namespace != null)
                return t.Namespace + "." + t.Name;
            else
                return t.Name;
        });
    }

    public static string GetNiceName(this Type type, string open = "<", string close = ">", string nested = "+")
    {
        return type.FormatType(t => TypeHelper.TryGetAlias(t) ?? t.Name, open, close, nested);
    }

    /// <summary>
    /// Gets the bare name of a type. In most cases, this is no different from <see cref="MemberInfo.Name"/>.
    /// If the type's name contains a backtick (<c>`</c>), such as for generic types, that character and everything
    /// after is disregarded.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetBareName(this Type type)
    {
        var name = type.Name;

        var tickIdx = name.IndexOf('`');
        if (tickIdx >= 0)
            return name[..tickIdx];

        return name;
    }

    // based on: http://stackoverflow.com/questions/2119441/check-if-types-are-castable-subclasses
    public static bool IsCastableTo(this Type from, Type to)
    {
        if (to.IsAssignableFrom(from))
            return true;

        // Check special cases (primitives)
        if (from.IsPrimitive && to.IsPrimitive &&
            from != typeof(bool) && to != typeof(bool))
        {
            return true;
        }

        var methods = from.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(
            m => m.ReturnType == to && (m.Name == "op_Implicit" || m.Name == "op_Explicit"));

        if (methods.Any())
            return true;

        var byRefFrom = from.MakeByRefType();

        methods = to.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(
            m => m.ReturnType == to && (m.Name == "op_Implicit" || m.Name == "op_Explicit") &&
            (m.GetParameters()?.Any(p => p.ParameterType == from || p.ParameterType == byRefFrom) ?? false));

        if (methods.Any())
            return true;

        return false;
    }

    public static List<Type> GetAllNestedTypes(this Type type)
    {
        var list = new List<Type>();
        GetAllNestedTypes(type, list);
        return list;
    }

    public static void GetAllNestedTypes(this Type type, List<Type> list)
    {
        list.Add(type);

        foreach (var nested in type.GetNestedTypes())
            GetAllNestedTypes(nested, list);
    }

    public struct AttributeMethod<A, D>
        where A : Attribute
    {
        public readonly A Attribute;
        public readonly D Method;

        public AttributeMethod(A attribute, D method)
        {
            this.Attribute = attribute;
            this.Method = method;
        }
    }

    public static List<AttributeMethod<A, D>> FindAllStaticMethodsWithAttribute<A, D>(Predicate<Assembly> assemblyFilter = null)
        where A : Attribute
        where D : class
    {
        var list = new List<AttributeMethod<A, D>>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                if (assemblyFilter != null && !assemblyFilter(assembly))
                    continue;

                foreach (var type in assembly.GetTypes())
                    foreach (var methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
                    {
                        var attribute = methodInfo.GetCustomAttribute<A>();
                        if (attribute != null)
                        {
                            var method = Delegate.CreateDelegate(typeof(D), methodInfo) as D;
                            if (method != null)
                                list.Add(new AttributeMethod<A, D>(attribute, method));
                        }
                    }
            }
            catch (Exception ex)
            {
                UniLog.Error("Exception loading types from assembly: " + assembly.FullName);
            }
        }

        return list;
    }

    public static bool IsNullable(this Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            return true;

        return false;
    }

    public static T GetCustomAttribute<T>(this Type type, bool inherit, bool fromInterfaces)
        where T : Attribute
    {
        return type.GetCustomAttributes<T>(inherit, fromInterfaces).FirstOrDefault();
    }

    public static IEnumerable<T> GetCustomAttributes<T>(this Type type, bool inherit, bool fromInterfaces)
        where T : Attribute
    {
        foreach (var att in type.GetCustomAttributes<T>(inherit))
            yield return att;

        if (fromInterfaces)
        {
            foreach (var @interface in type.EnumerateInterfacesRecursively())
                foreach (var att in @interface.GetCustomAttributes<T>(inherit))
                    yield return att;
        }
    }

    public static List<string> ExtractFullTypenames(string fullTypename)
    {
        var typenames = new List<string>();
        var str = new StringBuilder();
        bool collectName = true;

        for (int i = 0; i <= fullTypename.Length; i++)
        {
            if (i == fullTypename.Length || fullTypename[i] == '`' || fullTypename[i] == ',' || fullTypename[i] == '['
                || fullTypename[i] == ']')
            {
                if (str.Length > 0)
                    typenames.Add(str.ToString());
                str.Clear();

                collectName = i < fullTypename.Length && fullTypename[i] == '[';

                continue;
            }

            if (collectName)
                str.Append(fullTypename[i]);
        }

        return typenames;
    }

    public static Type[] GetGenericArgumentsFromClass(this Type type, Type classType)
    {
        if (type == typeof(object))
            return null;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == classType)
            return type.GetGenericArguments();

        return type.BaseType.GetGenericArgumentsFromClass(classType);
    }

    public static Type[] GetGenericArgumentsFromInterface(this Type type, Type genericInterfaceType)
    {
        foreach (var @interface in type.EnumerateInterfacesRecursively())
        {
            if (@interface.IsGenericType)
            {
                var genericDefinition = @interface.GetGenericTypeDefinition();

                if (genericDefinition == genericInterfaceType)
                    return @interface.GetGenericArguments();
            }
        }

        return null;
    }

    public static MethodInfo GetGenericMethod(this Type type, string name, BindingFlags bindingFlags, params Type[] typeArguments)
    {
        var methods = type.GetMethods(bindingFlags);

        foreach (var method in methods)
            if (method.IsGenericMethod && method.Name == name)
                return method.MakeGenericMethod(typeArguments);

        return null;
    }

    public static string TypeHierarchyToString(this Type type)
    {
        var str = new StringBuilder();

        while (type != typeof(object))
        {
            str.AppendLine(type.FullName);
            type = type?.DeclaringType ?? type.BaseType;
        }

        return str.ToString();
    }

    public static bool IsIntegerType(this Type type)
    {
        if (type == typeof(byte) ||
            type == typeof(ushort) ||
            type == typeof(uint) ||
            type == typeof(ulong) ||
            type == typeof(sbyte) ||
            type == typeof(short) ||
            type == typeof(int) ||
            type == typeof(long))
            return true;

        // Check for vector types
        if (type.IsVectorType())
            return type.GetVectorBaseType().IsIntegerType();

        return false;
    }

    public static bool IsVectorType(this Type type) => typeof(IVector).IsAssignableFrom(type);
    public static bool IsNumericVectorType(this Type type) => typeof(INumericVector).IsAssignableFrom(type);
    public static bool IsQuaternionType(this Type type) => typeof(IQuaternion).IsAssignableFrom(type);
    public static bool IsMatrixType(this Type type) => typeof(IMatrix).IsAssignableFrom(type);
    public static bool IsSphericalHarmonicsType(this Type type) => typeof(ISphericalHarmonics).IsAssignableFrom(type);

    public static int GetSphericalHarmonicsOrder(this Type type)
    {
        if (!type.IsGenericType)
            throw new ArgumentException("Type is not a spherical harmonic type");

        if (!type.IsGenericTypeDefinition)
            type = type.GetGenericTypeDefinition();

        if (type == typeof(SphericalHarmonicsL1<>))
            return 1;
        if (type == typeof(SphericalHarmonicsL2<>))
            return 2;
        if (type == typeof(SphericalHarmonicsL3<>))
            return 3;
        if (type == typeof(SphericalHarmonicsL4<>))
            return 4;

        throw new ArgumentException("Type is not a spherical harmonic type");
    }

    public static int SphericalHarmonicSize(this Type type)
    {
        var coeffCount = type.GetSphericalHarmonicsCoefficientCount();
        var arg = type.GetGenericArguments()[0];
        var size = arg.UnmanagedSizeOf();

        return size * coeffCount;
    }

    public static int GetSphericalHarmonicsCoefficientCount(this Type type) => SphericalHarmonicsHelper.CoefficientCount(type.GetSphericalHarmonicsOrder());

    public static int GetVectorDimensions(this Type type) => (int)type.GetField("DIMENSIONS").GetValue(null);
    public static Type GetVectorBaseType(this Type type) => (Type)type.GetField("BASE_TYPE").GetValue(null);

    public static int2 GetMatrixDimensions(this Type type)
    {
        var rows = type.GetField("ROWS", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var cols = type.GetField("COLUMNS", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        return new int2((int)cols.GetValue(null), (int)rows.GetValue(null));
    }

    public static int GetElementCount(this Type type)
    {
        if (type.IsVectorType())
            return type.GetVectorDimensions();
        else if (type == typeof(color) || type == typeof(colorX) || type.IsQuaternionType())
            return 4;
        else if (type.IsMatrixType())
        {
            var dim = type.GetMatrixDimensions();
            return dim.x * dim.y;
        }
        else if (type.IsSphericalHarmonicsType())
            return type.GetSphericalHarmonicsCoefficientCount();
        else if (type.IsPrimitive || type == typeof(decimal))
            return 1;
        else
            throw new ArgumentException($"Cannot determine element count for type: {type}");
    }

    public static object GetDefaultValue(this Type type)
    {
        if (type.IsValueType)
            return Activator.CreateInstance(type);
        else
            return null;
    }

    public static int ComputeMaxGenericDepth(this Type type)
    {
        // It's not a generic type, depth is 0
        if (!type.IsGenericType)
            return 0;

        // It's a type definition, which means it doesn't have any actual generic arguments
        // depth is also 0 in this case
        if (type.IsGenericTypeDefinition)
            return 0;

        var args = type.GetGenericArguments();

        int maxDepth = 0;

        foreach (var arg in args)
            maxDepth = MathX.Max(maxDepth, arg.ComputeMaxGenericDepth());

        return maxDepth + 1;
    }
}
