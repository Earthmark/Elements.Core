using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public static class EnumUtil
    {
        static ConcurrentDictionary<Type, List<EnumsNET.EnumMember>> uniqueNonObsoleteValues = new ConcurrentDictionary<Type, List<EnumsNET.EnumMember>>();
        public static E ShiftEnum<E>(this E val, int delta, bool checkIfObsolete = true)
            where E : Enum
        {
            return (E)ShiftEnum((IConvertible)val, delta, checkIfObsolete);
        }

        public static IConvertible ShiftEnum(IConvertible val, int delta, bool checkIfObsolete = true)
        {
            var type = val.GetType();

            if (!type.IsEnum)
                throw new Exception("Object is not an enum");

            IReadOnlyList<EnumsNET.EnumMember> values;

            if (checkIfObsolete)
            {
                if (!uniqueNonObsoleteValues.TryGetValue(type, out var valuesList))
                {
                    var members = EnumsNET.Enums.GetMembers(type);

                    valuesList = new List<EnumsNET.EnumMember>();
                    var uniqueValues = new HashSet<object>();

                    foreach (var member in members)
                    {
                        if (checkIfObsolete && member.Attributes.Get<ObsoleteAttribute>() != null)
                            continue;

                        // Skip values that were already added
                        if (!uniqueValues.Add(member.Value))
                            continue;

                        valuesList.Add(member);
                    }

                    uniqueNonObsoleteValues.TryAdd(type, valuesList);
                }

                values = valuesList;
            }
            else
                values = EnumsNET.Enums.GetMembers(type, EnumsNET.EnumMemberSelection.Distinct);

            int index = -1;

            for (int i = 0; i < values.Count; i++)
                if (values[i].Value.Equals(val))
                {
                    index = i;
                    break;
                }

            if (index == -1)
            {
                long nearestOffset = long.MaxValue;
                long numericVal = ((IConvertible)val).ToInt64(null);

                for(int i = 0; i < values.Count; i++)
                {
                    var offset = numericVal - values[i].ToInt64();

                    if (offset <= 0)
                        continue;

                    if(offset < nearestOffset)
                    {
                        nearestOffset = offset;
                        index = i;
                    }
                }

                // If it's still -1, means the value is before all the values in the enumeration, so just pick the first one
                if (index == -1)
                    index = 0;
            }

            index = MathX.Repeat(index + delta, values.Count - 1);

            return (IConvertible)values[index].Value;
        }

        public static E NextValue<E>(this E val)
            where E : Enum
        {
            return val.ShiftEnum(1);
        }

        public static E PreviousValue<E>(this E val)
            where E : Enum
        {
            return val.ShiftEnum(-1);
        }

        public static E UInt64ToEnum<E>(ulong val)
        {
            if (!typeof(E).IsEnum)
                throw new Exception("Argument must be an enumeration!");

            var underlyingType = Enum.GetUnderlyingType(typeof(E));

            // do a cast through underlying type to object to enum to make it work
            if (underlyingType == typeof(int))  // check for int first, because that's most common
                return (E)(object)(int)val;
            else if (underlyingType == typeof(byte))
                return (E)(object)(byte)val;
            else if (underlyingType == typeof(sbyte))
                return (E)(object)(sbyte)val;
            else if (underlyingType == typeof(short))
                return (E)(object)(short)val;
            else if (underlyingType == typeof(ushort))
                return (E)(object)(ushort)val;
            else if (underlyingType == typeof(uint))
                return (E)(object)(uint)val;
            else if (underlyingType == typeof(long))
                return (E)(object)(long)val;
            else if (underlyingType == typeof(ulong))
                return (E)(object)(ulong)val;
            else
                throw new Exception("Invalid underlying type for enum! " + underlyingType);
        }
    }
}
