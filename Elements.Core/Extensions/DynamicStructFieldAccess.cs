using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Elements.Core
{
    public static class DynamicStructFieldAccess
    {
        public static object GetStructField(this object @struct, string path)
        {
            return new StructMemberAccessor(@struct.GetType(), path).Get(@struct);
        }

        public static object SetStructField(this object @struct, string path, object value)
        {
            return new StructMemberAccessor(@struct.GetType(), path).Set(@struct, value);
        }
    }
}
