using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public static class ExceptionHelper
    {
        public static string PrintAllInnerExceptions(this Exception ex)
        {
            if (ex == null)
                return "(no Exception)";

            var str = new StringBuilder();

            str.AppendLine($"Exception:");
            str.AppendLine(ex.ToString());

            var inner = ex.InnerException;

            while(inner != null)
            {
                str.AppendLine($"InnerException:");
                str.AppendLine(inner.ToString());

                inner = inner.InnerException;
            }

            return str.ToString();
        }
    }
}
