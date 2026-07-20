using System;
using System.IO;
using System.Reflection;

namespace SimpleML.Core
{
    /// <summary>
    /// 跨平台路径解析：定位 myML / Python 包根目录。
    /// 兼容 Rhino 7+、Windows 与 macOS。
    /// </summary>
    public static class PathResolver
    {
        public static string GetMyMLPath()
        {
            string envPath = Environment.GetEnvironmentVariable("SIMPLEML_PATH");
            if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
                return envPath;

            // 与 GHA 同级的 myML / 包根
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            if (!string.IsNullOrEmpty(assemblyDir))
            {
                string[] candidates =
                {
                    Path.Combine(assemblyDir, "myML"),
                    Path.Combine(assemblyDir, "..", "myML"),
                    assemblyDir
                };
                foreach (string c in candidates)
                {
                    try
                    {
                        string full = Path.GetFullPath(c);
                        if (IsValidPackageRoot(full))
                            return full;
                    }
                    catch { }
                }
            }

            // Grasshopper Libraries / UserObjects（Win + Mac，多版本）
            foreach (string libDir in RhinoCompat.EnumerateGrasshopperLibraryDirs())
            {
                string[] nested =
                {
                    Path.Combine(libDir, "SimpleML", "myML"),
                    Path.Combine(libDir, "myML"),
                    Path.Combine(libDir, "SimpleML"),
                };
                foreach (string c in nested)
                {
                    if (IsValidPackageRoot(c))
                        return c;
                }
            }

            return null;
        }

        public static bool IsValidPackageRoot(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return false;
            return Directory.Exists(Path.Combine(path, "components"))
                   && Directory.Exists(Path.Combine(path, "core"));
        }
    }
}
