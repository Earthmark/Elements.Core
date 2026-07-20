using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Elements.Core
{
    public readonly struct RefID : IComparable, IEquatable<RefID>, IComparable<RefID>
    {
        public const int USER_BITS = 8;
        public const ulong USER_MASK = (~(0UL)) >> (sizeof(ulong) * 8 - USER_BITS);
        public const ulong ID_MASK = ~USER_MASK;
        public const int MAX_USERS = (1 << USER_BITS) - 1;
        public const byte LOCAL_ID = (byte)0xFFU;

        readonly ulong id;

        public RefID(ulong id)
        {
            this.id = id;
        }

        public static RefID Construct(ulong position, byte user) => new RefID((position << USER_BITS) | (user & USER_MASK));

        public void ExtractIDs(out ulong position, out byte user)
        {
            user = (byte)(id & USER_MASK);
            position = id >> USER_BITS;
        }

        public readonly bool IsLocalID => (id & LOCAL_ID) == LOCAL_ID;
        public readonly byte User => (byte)(id & USER_MASK);
        public readonly ulong Position => id >> USER_BITS;

        public override string ToString() => "ID" + id.ToString("X");

        public readonly bool Equals(RefID other) => id == other.id;

        public override bool Equals(object obj)
        {
            if (!(obj is RefID))
                return false;

            return Equals((RefID)obj);
        }

        public override int GetHashCode() => id.GetHashCode();

        public int CompareTo(object obj)
        {
            if (!(obj is RefID))
                throw new ArgumentException("Object is not RefID");

            var other = (RefID)obj;

            return this.CompareTo(other);
        }

        public readonly int CompareTo(RefID other)
        {
            int comp = User.CompareTo(other.User);

            if (comp == 0)
                return Position.CompareTo(other.Position);

            return comp;
        }

        public readonly int RawCompareTo(RefID other) => id.CompareTo(other.id);

        public static bool TryParse(string str, out RefID value)
        {
            if(str.Length < 3)
            {
                value = default;
                return false;
            }

            if (str[0] != 'I' && str[1] != 'D')
            {
                value = default;
                return false;
            }

            var substr = str.Substring(2);

            if(ulong.TryParse(substr, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var ulongValue))
            {
                value = ulongValue;
                return true;
            }

            value = default;
            return false;
        }

        public static RefID Parse(string str)
        {
            if (TryParse(str, out var value))
                return value;

            throw new ArgumentException("Failed to parse RefID");
        }

        public static RefID MinValue => ulong.MinValue;
        public static RefID MaxValue => ulong.MaxValue;

        public static RefID Null => default;

        public static explicit operator ulong(RefID id) => id.id;
        public static implicit operator RefID(ulong id) => new RefID(id);

        public static bool operator ==(RefID a, RefID b) => a.id == b.id;
        public static bool operator !=(RefID a, RefID b) => a.id != b.id;

        public static RefID operator +(RefID a, RefID b) => a.id + b.id;
        public static RefID operator -(RefID a, RefID b) => a.id - b.id;

        public static RefID operator >>(RefID a, int n) => a.id >> n;
        public static RefID operator <<(RefID a, int n) => a.id << n;

        public static RefID operator &(RefID a, ulong m) => a.id & m;
        public static RefID operator |(RefID a, ulong m) => a.id | m;
        public static RefID operator ^(RefID a, ulong m) => a.id ^ m;

        public static bool operator >(RefID a, RefID b) => a.CompareTo(b) > 0;
        public static bool operator <(RefID a, RefID b) => a.CompareTo(b) < 0;
    }

    public static class RefIDExtensions
    {
        public static DataTreeValue Save(this RefID id) => new DataTreeValue(id.ToString());
        public static RefID LoadRefID(this DataTreeNode node) => RefID.Parse(node.LoadString());

        public static void Write(this BinaryWriter wr, RefID id) => wr.Write((ulong)id);
        public static RefID ReadRefID(this BinaryReader rd) => rd.ReadUInt64();
    }
}
