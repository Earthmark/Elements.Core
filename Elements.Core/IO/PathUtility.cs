using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Elements.Core
{
    public static class PathUtility
    {
        public static string NormalizePath(string path)
        {
            if (Path.DirectorySeparatorChar == '\\')
                return path.Replace('/', '\\');
            else
                return path.Replace('\\', '/');
        }

        public static string GetDirectoryName(string path)
        {
            if (Path.DirectorySeparatorChar == '\\' || Path.AltDirectorySeparatorChar == '\\' || !path.Contains('\\'))
                return Path.GetDirectoryName(path);

            path = path.Replace('\\', '/');

            var directoryName = Path.GetDirectoryName(path);
            directoryName = directoryName.Replace('/', '\\');

            return directoryName;
        }

        public static string GetFileName(string path)
        {
            if (Path.DirectorySeparatorChar == '\\' || Path.AltDirectorySeparatorChar == '\\' || !path.Contains('\\'))
                return Path.GetFileName(path);

            path = path.Replace('\\', '/');

            return Path.GetFileName(path);
        }
    }
}
