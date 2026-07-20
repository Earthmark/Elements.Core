using Elements.Core; using Elements.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elements.Data;

namespace Elements
{
    [DataModelType]
    public enum Axis2D : byte
    {
        X, Y
    }

    [DataModelType]
    public enum Axis3D : byte
    {
        X, Y, Z
    }

    [DataModelType]
    public enum Axis4D : byte
    {
        X, Y, Z, W
    }
}
