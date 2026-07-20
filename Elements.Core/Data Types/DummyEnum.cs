using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elements.Data;

namespace Elements.Core
{
    // Similar to the dummy type, this can be used for testing purposes and as a placeholder for generic types
    [DataModelType]
    public enum DummyEnum 
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,

        Seven = 7,

        Hundred = 100,
        Thousand = 1000,
        Million = 1000000,
        Billion = 1000000000,

        // Duplicate entry
        Cheese = 7,

        /// <summary>
        /// This value is obsolete, because the cuteness has increased way beyond what this can represent
        /// </summary>
        [Obsolete]
        GlitchCuteness = Billion,
    }
}
