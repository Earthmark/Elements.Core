using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public static class NiceTypeParser
    {
        public static Func<string, Type> DefaultTypeSearcher;

        readonly struct TypePart
        {
            public readonly StringSegment typeName;
            public readonly StringSegment typeNameRemainder;
            public readonly int genericArgCount;

            public bool IsEmpty => typeName.IsEmpty;

            public TypePart(StringSegment typeName, StringSegment typeNameRemainder, int genericArgumentCount)
            {
                this.typeName = typeName;
                this.typeNameRemainder = typeNameRemainder;
                this.genericArgCount = genericArgumentCount;
            }
        }


        public static Type TryParse(string str) => TryParse(str, DefaultTypeSearcher ?? (s => TypeHelper.FindType(s)));

        public static Type TryParse(string str, Func<string, Type> typeSearcher) => TryParse(new StringSegment(str), typeSearcher);

        static SlimList<int> ExtractArrayRanks(StringBuilder typename)
        {
            var ranks = new SlimList<int>();

            // We want to ignore any brackets at the beginning of the typename, because those can be used to indicate an assembly
            // We will only start considering array once we have found the first letter of the typename that's not within brackets
            bool foundTypeStart = false;

            int rank = 0;
            bool withinBrackets = false;

            int firstArrayStart = -1;

            for(int i = 0; i < typename.Length; i++)
            {
                var ch = typename[i];

                if (!foundTypeStart && !withinBrackets && char.IsLetter(ch))
                    foundTypeStart = true;

                if(ch == '[')
                {
                    // We are already within array! Something is wrong!
                    if (withinBrackets)
                    {
                        ranks.Clear();
                        ranks.Add(-1);
                        return ranks;
                    }

                    withinBrackets = true;

                    if (foundTypeStart)
                    {
                        if (firstArrayStart < 0)
                            firstArrayStart = i;

                        rank++; // we are within array, start with rank 1
                    }
                }
                else if(ch == ',' && foundTypeStart)
                {
                    // We found a comma! Increase the rank!
                    if (withinBrackets)
                        rank++;
                }
                else if(ch == ']')
                {
                    if (foundTypeStart)
                    {
                        // We're finished with this array! Add its rank to the list
                        ranks.Add(rank);

                        // Reset back, because we want to scan for even more arrays
                        rank = 0;
                    }

                    withinBrackets = false;
                }
            }

            // Cut the array parts from the typename
            if(firstArrayStart >= 0)
                typename.Length = firstArrayStart;

            return ranks;
        }

        static int? FindNestedSplitIndex(StringSegment str)
        {
            int nestingLevel = 0;

            for (int i = 0; i < str.length; i++)
            {
                var ch = str[i];

                if (ch == '<')
                    nestingLevel++;
                else if (ch == '>')
                {
                    nestingLevel--;

                    // Something is wrong
                    if (nestingLevel < 0)
                        return null;
                }
                else if (ch == '+' && nestingLevel == 0)
                    return i;
            }

            return -1;
        }

        static bool IsNullable(StringBuilder str)
        {
            for(int i = str.Length - 1; i >= 0; i--)
            {
                var ch = str[i];

                if (ch == '?')
                {
                    // Cut the part from the end of it
                    str.Length = i;
                    return true;
                }

                if (!char.IsWhiteSpace(ch))
                    return false;
            }

            return false;
        }

        public static Type TryParse(StringSegment str, Func<string, Type> typeSearcher)
        {
            var baseTypeParts = new List<TypePart>();
            var genericArguments = new List<Type>();

            do
            {
                var index = FindNestedSplitIndex(str);

                if (index == null)
                    return null;

                StringSegment typePartStr;

                if(index.Value < 0)
                {
                    typePartStr = str;
                    str = new StringSegment();
                }
                else
                {
                    str.SplitAt(index.Value, out typePartStr, out str);

                    if(str.length > 0)
                        str = str.Slice(1); // Remove the + symbol
                }

                var typePart = TryParseTypePart(typePartStr, genericArguments, typeSearcher);

                // If this failed to parse, then we bail out, because the type is incorrect
                if (typePart.IsEmpty)
                    return null;

                baseTypeParts.Add(typePart);

            } while (str.length > 0);

            // We now must determine the container type, by composing all the parts of the nested types to get the full string
            var containerTypeBuilder = new StringBuilder();

            for(int i = 0; i < baseTypeParts.Count; i++)
            {
                var part = baseTypeParts[i];

                containerTypeBuilder.Append(part.typeName);

                if (part.genericArgCount > 0)
                {
                    containerTypeBuilder.Append('`');
                    containerTypeBuilder.Append(part.genericArgCount);
                }

                containerTypeBuilder.Append(part.typeNameRemainder);

                if (i < baseTypeParts.Count - 1)
                    containerTypeBuilder.Append('+');
            }

            // Check if it's an array
            var arrayRanks = ExtractArrayRanks(containerTypeBuilder);

            // This indicates that something went wrong with parsing the array, we bail out
            if (arrayRanks.Count > 0 && arrayRanks[0] < 0)
                return null;

            // Check if it's nullable
            var isNullable = IsNullable(containerTypeBuilder);

            Type FinalizeType(Type type)
            {
                if (isNullable)
                {
                    // It must be value type in order to be nullable
                    if (!type.IsValueType)
                        return null;

                    type = typeof(Nullable<>).MakeGenericType(type);
                }

                for (int i = arrayRanks.Count - 1; i >= 0; i--)
                {
                    var rank = arrayRanks[i];

                    if (rank == 1)
                        type = type.MakeArrayType();
                    else
                        type = type.MakeArrayType(rank);
                }

                return type;
            }

            var containerTypeStr = containerTypeBuilder.ToString();

            // If there's nothing left, then this is not a valid type
            if (string.IsNullOrEmpty(containerTypeStr))
                return null;

            var containerType = TypeHelper.TryResolveAlias(containerTypeStr) ?? typeSearcher(containerTypeStr);

            // We didn't find the container type! Bail out!
            if (containerType == null)
                return null;

            // There's no generic arguments, just return the type!
            if (genericArguments.Count == 0)
                return FinalizeType(containerType);

            bool? isGeneric = null;

            // Check all the types, whether this is generic or not
            // It's not possible to mix and match. Either all arguments are null, or none of them are
            foreach(var arg in genericArguments)
            {
                if(arg == null)
                {
                    if (isGeneric == false)
                        return null;

                    isGeneric = true;
                }
                else
                {
                    if (isGeneric == true)
                        return null;

                    isGeneric = false;
                }
            }

            // We just care about the generic container type, so don't use any of the type arguments
            if (isGeneric == true)
                return FinalizeType(containerType);

            try
            {
                // Try to construct the type. We do this in try-catch block, because it's easiest way to do all the type
                // and constraint checks, which we don't want to do manually
                return FinalizeType(containerType.MakeGenericType(genericArguments.ToArray()));
            }
            catch
            {
                return null;
            }
        }

        static TypePart TryParseTypePart(StringSegment typePart, List<Type> genericArguments, Func<string, Type> typeSearcher)
        {
            var genericStart = typePart.IndexOf("<");

            // If there's no generic arguments at all, parsing is really simple, we just resolve the string
            if (genericStart < 0)
                return new TypePart(typePart, default, 0);

            var genericEnd = typePart.LastIndexOf(">");

            // If it doesn't have a closing one, it's invalid type string and we bail out
            if (genericEnd < 0)
                return default;

            var containerType = typePart.Slice(0, genericStart);
            var containerTypeRemainder = typePart.Slice(genericEnd + 1);

            int currentStart = genericStart + 1;
            int currentNestLevel = 0;

            int genericArgCount = 0;

            bool ProcessArgument(int index)
            {
                var subString = typePart.Slice(currentStart, index - currentStart).Trim();

                Type type = null;

                if (subString.length > 0)
                {
                    // Parse the nested type recursively
                    type = TryParse(subString, typeSearcher);

                    // We failed to parse the nested type
                    if (type == null)
                        return false;
                }

                // If the string is empty, then we just consider this to be generic type definition
                // and we assign null there. This is okay, we'll do a check on all of them after all types were processed

                genericArguments.Add(type);
                genericArgCount++;

                // Move the start for the next one
                currentStart = index + 1;

                return true;
            }

            bool insideArray = false;

            // We must now parse all the nested types recursively
            for (int i = genericStart + 1; i < genericEnd + 1; i++)
            {
                var ch = typePart[i];

                if (ch == '<')
                {
                    currentNestLevel++;
                    continue;
                }

                if (ch == '>')
                {
                    if (--currentNestLevel < 0)
                    {
                        // If we nested out of the brackets, consider this end of all types and process the last one
                        if (!ProcessArgument(i))
                            return default;

                        break;
                    }

                    continue;
                }

                // We are inside type tags, ignore everything else
                if (currentNestLevel > 0)
                    continue;

                if (ch == '[')
                {
                    // We're already inside array! Something is wrong.
                    if (insideArray)
                        return default;

                    insideArray = true;
                }
                else if (ch == ']')
                {
                    // We're not inside array! Something is wrong also.
                    if (!insideArray)
                        return default;

                    insideArray = false;
                }

                // We hit a comma at the root level, process the nested argument, but only if we're not within array
                if (!insideArray && ch == ',')
                    if (!ProcessArgument(i))
                        return default;
            }

            return new TypePart(containerType, containerTypeRemainder, genericArgCount);
        }
    }
}
