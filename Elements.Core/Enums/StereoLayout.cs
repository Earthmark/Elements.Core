using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elements.Data;

namespace Elements.Core
{
    [DataModelType]
    [OldTypeName("FrooxEngine.StereoLayout", "FrooxEngine")]
    public enum StereoLayout
    {
        None,
        Horizontal_LR,
        Vertical_LR,
        Horizontal_RL,
        Vertical_RL,
        Custom
    }

    public static class StereoLayoutHelper
    {
        public static float2 StereoLayoutScaleRatio(this StereoLayout stereoLayout)
        {
            switch (stereoLayout)
            {
                case StereoLayout.None:
                    return float2.One;

                case StereoLayout.Horizontal_LR:
                case StereoLayout.Horizontal_RL:
                    return new float2(0.5f, 1f);

                case StereoLayout.Vertical_LR:
                case StereoLayout.Vertical_RL:
                    return new float2(1f, 0.5f);

                default:
                    throw new ArgumentException($"Cannot compute stereo layout scale ratio for layout: {stereoLayout}");
            }
        }
    }
}
