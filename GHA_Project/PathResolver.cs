using System;
using System.IO;
using System.Reflection;

namespace SimpleML.Core
{
    /// <summary>
    /// 跨平台路径解析：定位 myML / Python 包根目录。
    /// </summary>
    public static class PathResolver
    {
        public static string GetMyMLPath()
        {
            string envPath = Environment.GetEnvironmentVariable("SIMPLEML_PATH");
            if (!string.IsNullOrEmpty(envPath) && Directory.Exists(envPath))
                return envPath;

            // 与 GHA 同级的 myML
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            if (!string.IsNullOrEmpty(assemblyDir))
            {
                string[] candidates =
                {
                    Path.Combine(assemblyDir, "myML"),
                    Path.Combine(assemblyDir, "..", "myML"),
                    assemblyDir // 直接把 components/core 放在插件旁
                };
                foreach (string c in candidates)
                {
                    string full = Path.GetFullPath(c);
                    if (IsValidPackageRoot(full))
                        return full;
                }
            }

            // 用户 Grasshopper Libraries / UserObjects（Windows + 通用 AppData）
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string[] appCandidates =
            {
                Path.Combine(appData, "Grasshopper", "Libraries", "SimpleML", "myML"),
                Path.Combine(appData, "Grasshopper", "UserObjects", "SimpleML", "myML"),
                Path.Combine(appData, "Grasshopper", "Libraries", "myML"),
                Path.Combine(appData, "Grasshopper", "UserObjects", "myML"),
            };
            foreach (string c in appCandidates)
            {
                if (IsValidPackageRoot(c))
                    return c;
            }

            // macOS Rhino 常见路径
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string[] macCandidates =
            {
                Path.Combine(home, "Library", "Application Support", "McNeel", "Rhinoceros", "8.0", "Plug-ins", "Grasshopper", "Libraries", "SimpleML", "myML"),
                Path.Combine(home, "Library", "Application Support", "Grasshopper", "Libraries", "SimpleML", "myML"),
            };
            foreach (string c in macCandidates)
            {
                if (IsValidPackageRoot(c))
                    return c;
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
