using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepuUtilities;
using BepuPhysics;

namespace Elements.Core
{
    public static class BepuConversions
    {
        public static BepuUtilities.BoundingBox ToBepu(this Elements.Core.BoundingBox bounds) => new BepuUtilities.BoundingBox(bounds.min, bounds.max);
        public static Elements.Core.BoundingBox ToEngine(this BepuUtilities.BoundingBox bounds) => new Elements.Core.BoundingBox(bounds.Min, bounds.Max);
    }
}
