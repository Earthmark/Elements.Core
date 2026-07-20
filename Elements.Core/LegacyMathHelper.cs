using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    /// <summary>
    /// This class is responsible for emulating legacy math under Mono / .NET Framework
    /// There is a number of differences that happened in .NET (Core) to make math consistent with the spec
    /// So this class is used to manually emulate the legacy behavior for compatibility reasons
    /// </summary>
    public static partial class MathX
    {
        public static int LegacyCastToInt(float value)
        {
            if (value >= int.MaxValue)
                return int.MinValue;

            return (int)value;
        }

        public static int LegacyCastToInt(double value)
        {
            if (value >= int.MaxValue)
                return int.MinValue;

            return (int)value;
        }

        public static int LegacyFloorToInt(float val) => LegacyCastToInt(Floor(val));
        public static int LegacyCeilToInt(float val) => LegacyCastToInt(Ceil(val));
        public static int LegacyRoundToInt(float val) => LegacyCastToInt(val + (val < 0 ? -0.5f : 0.5f));

        public static int LegacyFloorToInt(double val) => LegacyCastToInt(Floor(val));
        public static int LegacyCeilToInt(double val) => LegacyCastToInt(Ceil(val));
        public static int LegacyRoundToInt(double val) => LegacyCastToInt(val + (val < 0 ? -0.5 : 0.5));
    }
}
