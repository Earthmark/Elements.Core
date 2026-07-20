using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public class BallisticStepper
    {
        public float3 Position;
        public float3 Velocity;
        public float3 Gravity;
        public float Drag;

        public float3 StepTime(float delta)
        {
            var positionDelta = Velocity * delta;

            Position += positionDelta;
            Velocity -= Velocity * Drag * delta;
            Velocity += Gravity * delta;

            return positionDelta;
        }

        public float3 StepDistance(float units)
        {
            return StepTime(units / Velocity.Magnitude);
        }
    }
}
