using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public readonly struct Ray
    {
        public readonly float3 origin;
        public readonly float3 direction;

        public Ray(in float3 origin, in float3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }
    }
}
