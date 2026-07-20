using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CodeGenerationConfig
{
    // Helper class
    public static class H
    {
        public static string VectorElements = "xyzw";

        public static string Capitalize(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static string FormatList<T>(Func<T,string> toString, string separator, params T[] list)
        {
            return FormatList((e, i) => toString(e), separator, list);
        }

        public static string FormatList<T>(Func<T,int,string> toString, string separator, params T[] list)
        {
            string str = "";

            for(int i = 0; i < list.Length; i++)
            {
                str += toString(list[i], i);

                if (i != list.Length - 1)
                    str += separator;
            }

            return str;
        }

        public static string FormatList(int elements, string format, string separator)
        {
            string str = "";

            for(int i = 0; i < elements; i++)
            {
                str += String.Format(format, i);

                if (i != elements - 1)
                    str += separator;
            }

            return str;
        }

        public static string FormatElements(List<string> elements, string format, string separator)
        {
            string str = "";

            for (int i = 0; i < elements.Count; i++)
            {
                str += String.Format(format, elements[i], i);

                if (i != elements.Count - 1)
                    str += separator;
            }

            return str;
        }

        // formats a series of elements
        public static string FormatElements(string elements, string format, string separator,
            int? forceElements = null, string emptyElement = "0")
        {
            string str = "";

            int n = forceElements ?? elements.Length;

            for (int i = 0; i < n; i++)
            {
                if (i < elements.Length)
                    str += String.Format(format, elements[i], i);
                else
                    str += emptyElement;

                if (i != n - 1)
                    str += separator;
            }

            return str;
        }

        // Generating swizzles
        public delegate string SwizzleStatementGenerator(int length, string swizzle,
            string expression);

        public static string GenerateSwizzles(string elements,
            int sourceSize, int minSize, int maxSize,
            SwizzleStatementGenerator generator, string defaultValue = "0")
        {
            StringBuilder str = new StringBuilder();

            // add the empty element
            elements += "_";

            for(int sn = minSize; sn <= maxSize; sn++)
            {
                // combinations for given swizzle length
                int combinations = (int)Math.Pow(elements.Length, sn);

                // iterate through all the possible combinations and generate swizzles
                for(int si = 0; si < combinations; si++)
                {
                    // first build the swizzle itself for given combination
                    int comb = si;
                    string swizzle = "";
                    for(int se = 0; se < sn; se++)
                    {
                        int el = comb % elements.Length;
                        comb /= elements.Length;
                        swizzle += elements[el];
                    }

                    // skip swizzle that contains only empty elements
                    if (swizzle.Count(ch => ch == '_') == swizzle.Length)
                        continue;

                    // build the inner element expression for given swizzle
                    string expression = "";
                    for(int ss = 0; ss < swizzle.Length; ss++)
                    {
                        if (swizzle[ss] == '_')
                            expression += defaultValue;
                        else
                            expression += swizzle[ss];

                        if (ss != swizzle.Length - 1)
                            expression += ",";
                    }

                    // call the passed function to generate full swizzle statement
                    str.AppendLine(generator(sn, swizzle, expression));
                }
            }

            return str.ToString();
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

        public static bool IsCastableTo(this TypeInfo from, TypeInfo to)
        {
            if (from.Type != null && to.Type != null)
                return from.Type.IsCastableTo(to.Type);

            if (from.IsVector != to.IsVector)
                return false;

            if (from.IsMatrix != to.IsMatrix)
                return false;

            if (from.IsQuaternion != to.IsQuaternion)
            {
                // Ignore the color types - they're technically vectors too
                if(from.IsColor || to.IsColor)
                    return false;

                // Quaternions can be casted to/from 4D vectors of the same base type
                if ((from.IsVector || to.IsVector) && (from.BaseType == to.BaseType) && (from.Dimensions == to.Dimensions))
                    return true;

                return false;
            }

            if (from.IsColor != to.IsColor)
            {
                if (from.BaseType == to.BaseType && from.Dimensions == to.Dimensions)
                    return true;

                return false;
            }

            if (from.IsVector)
            {
                if (from.Dimensions != to.Dimensions)
                    return from.BaseType == to.BaseType;
            }

            if(from.IsMatrix)
            {
                if (from.MatrixSize != to.MatrixSize)
                    return from.BaseType == to.BaseType;
            }

            if(from.IsColor)
            {
                var hasProfile = from.TypeDeclaration.EndsWith("X") || to.TypeDeclaration.EndsWith("X");
                var has32bit = from.TypeDeclaration.EndsWith("32") || to.TypeDeclaration.EndsWith("32");

                // The 32 bit color cannot be casted to color profile ones
                if (hasProfile && has32bit)
                    return false;
            }

            if(from.BaseType != null && to.BaseType != null)
            {
                if (!from.BaseType.IsCastableTo(to.BaseType))
                    return false;

                return true;
            }

            throw new NotSupportedException($"Cannot determine castability from {from} to {to}");
        }
    }
}
