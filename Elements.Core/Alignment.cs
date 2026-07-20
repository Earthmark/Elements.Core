using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elements.Data;

namespace Elements.Core
{
    /// <summary>
    /// Controls alignment for various UI elements. Vertical is first followed by Horizontal
    /// </summary>
    [DataModelType]
    public enum Alignment
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}
