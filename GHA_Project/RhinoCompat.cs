using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rhino;
using Rhino.Runtime;

namespace SimpleML.Core
{
    /// <summary>
    /// Rhino 7+ / Windows / macOS 兼容层。
    /// 原则：以 Rhino 7 API 为基线，运行时自动适配 7/8/更新版本与 Mac。
    /// </summary>
    public static class RhinoCompat
    {
        public static bool IsWindows
        {
            get
            {
                try { return HostUtils.RunningOnWindows; }
                catch { return Environment.OSVersion.Platform == PlatformID.Win32NT; }
            }
        }

        public static bool IsMac
        {
            get
            {
                try { return HostUtils.RunningOnOSX; }
                catch
                {
                    // 非 Rhino 宿主时的回退
                    if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                        return true;
                    return Directory.Exists("/System/Library/CoreServices");
                }
            }
        }

        /// <summary>当前 Rhino 主版本（7/8/9...），失败时返回 0。</summary>
        public static int RhinoMajorVersion
        {
            get
            {
                try { return RhinoApp.Version.Major; }
                catch { return 0; }
            }
        }

        public static string PlatformLabel
        {
            get
            {
                string os = IsMac ? "macOS" : (IsWindows ? "Windows" : "Unix");
                int v = RhinoMajorVersion;
                return v > 0 ? $"{os} / Rhino {v}" : os;
            }
        }

        /// <summary>
        /// 可能的 Rhinocode 根目录（Rhino 8+ CPython；Rhino 7 通常用系统 Python）。
        /// </summary>
        public static IEnumerable<string> EnumerateRhinocodeRoots()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            yield return Path.Combine(home, ".rhinocode");

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!string.IsNullOrEmpty(appData))
                yield return Path.Combine(appData, ".rhinocode");
            if (!string.IsNullOrEmpty(local))
                yield return Path.Combine(local, ".rhinocode");

            // macOS Application Support
            yield return Path.Combine(home, "Library", "Application Support", "McNeel", "Rhinoceros", ".rhinocode");
            yield return Path.Combine(home, "Library", "Application Support", ".rhinocode");
        }

        /// <summary>
        /// 枚举 Rhinocode 环境目录名，优先匹配当前 Rhino 主版本（rh7/rh8/rh9...）。
        /// </summary>
        public static IEnumerable<string> EnumerateRhinocodeEnvDirs()
        {
            int major = RhinoMajorVersion;
            var all = new List<string>();

            foreach (string root in EnumerateRhinocodeRoots())
            {
                if (!Directory.Exists(root))
                    continue;
                try
                {
                    foreach (string dir in Directory.GetDirectories(root))
                        all.Add(dir);
                }
                catch { }
            }

            // 优先：匹配当前版本后缀 -rh7 / -rh8 ...
            if (major >= 7)
            {
                string tag = "-rh" + major;
                foreach (string d in all.Where(p => Path.GetFileName(p).IndexOf(tag, StringComparison.OrdinalIgnoreCase) >= 0)
                                        .OrderByDescending(p => p))
                    yield return d;
            }

            // 其次：任意 py*-rh*（覆盖未来版本）
            foreach (string d in all.Where(p =>
            {
                string name = Path.GetFileName(p) ?? "";
                return name.StartsWith("py", StringComparison.OrdinalIgnoreCase)
                       && name.IndexOf("-rh", StringComparison.OrdinalIgnoreCase) >= 0;
            }).OrderByDescending(p => p))
                yield return d;

            // 其余目录
            foreach (string d in all.OrderByDescending(p => p))
                yield return d;
        }

        /// <summary>在环境目录中查找 python 可执行文件。</summary>
        public static string FindPythonInEnvDir(string envDir)
        {
            if (string.IsNullOrEmpty(envDir) || !Directory.Exists(envDir))
                return null;

            var candidates = new List<string>();
            if (IsWindows)
            {
                candidates.Add(Path.Combine(envDir, "python.exe"));
                candidates.Add(Path.Combine(envDir, "python3.exe"));
            }
            else
            {
                candidates.Add(Path.Combine(envDir, "bin", "python3"));
                candidates.Add(Path.Combine(envDir, "bin", "python"));
                candidates.Add(Path.Combine(envDir, "python3"));
                candidates.Add(Path.Combine(envDir, "python"));
            }

            foreach (string p in candidates)
            {
                if (File.Exists(p))
                    return p;
            }
            return null;
        }

        /// <summary>
        /// Grasshopper Libraries 候选目录（Windows + Mac，Rhino 7/8/9+）。
        /// </summary>
        public static IEnumerable<string> EnumerateGrasshopperLibraryDirs()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            // Windows：各版本常共用 %APPDATA%\Grasshopper
            if (!string.IsNullOrEmpty(appData))
            {
                yield return Path.Combine(appData, "Grasshopper", "Libraries");
                yield return Path.Combine(appData, "Grasshopper", "UserObjects");
            }

            // macOS：按 Rhino 主版本目录
            string macBase = Path.Combine(home, "Library", "Application Support", "McNeel", "Rhinoceros");
            int major = RhinoMajorVersion;
            var versions = new List<string>();
            if (major >= 7)
                versions.Add(major + ".0");
            // 同时探测常见版本，便于手动安装到非当前版本目录
            foreach (string v in new[] { "7.0", "8.0", "9.0", "10.0" })
            {
                if (!versions.Contains(v))
                    versions.Add(v);
            }

            foreach (string v in versions)
            {
                yield return Path.Combine(macBase, v, "Plug-ins", "Grasshopper", "Libraries");
                yield return Path.Combine(macBase, v, "Plug-ins", "Grasshopper", "UserObjects");
            }

            yield return Path.Combine(home, "Library", "Application Support", "Grasshopper", "Libraries");
        }

        public static string DescribeCompatibility()
        {
            int major = RhinoMajorVersion;
            string note = major > 0 && major < 7
                ? "当前 Rhino 版本低于 7，SimpleML 官方支持 Rhino 7 及以上。"
                : "支持 Rhino 7 及以上（含 Rhino 8/更新版本）与 macOS。";
            return PlatformLabel + " | " + note;
        }
    }
}
