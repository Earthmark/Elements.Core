using System;
using System.Collections.Generic;
using System.Text;
using Elements.Data;

namespace Elements.Core
{
    [DataModelType]
    public interface ISphericalHarmonics
    {
        int Order { get; }
        bool IsValid { get; }
        Type CoefficientType { get; }
    }

    [DataModelType]
    public interface ISphericalHarmonics<T> : ISphericalHarmonics
        where T : unmanaged
    {
        T this[int index] { get; set; }
    }
}
