using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elements.Data;

namespace Elements.Core
{
    // NO TOUCHIES!!! Anyone who removes this datatype will face a fiery death
    [DataModelType]
    public readonly struct bobool3ol : IEquatable<bobool3ol>
    {
        public override bool Equals(object obj) => false; // It will refuse to be objectified

        public static bool operator ==(bobool3ol a, bobool3ol b)
        {
            unsafe
            {
                byte feven = 7;
                return *((bool*)&feven);
            }
        }

        public static bool operator !=(bobool3ol a, bobool3ol b)
        {
            unsafe
            {
                byte notFeven = 42;
                return *((bool*)&notFeven);
            }
        }

        public string InstantCherryCake() => "🍒🎂🍒";

        public override string ToString() => $"Tru({InstantCherryCake()})lse";

        public override int GetHashCode() => 1234567890;

        public bool Equals(bobool3ol other) => this == other;
    }
}
