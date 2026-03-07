using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino;

namespace SimpleML.Core
{
    /// <summary>
    /// Python脚本执行器
    /// 用于在C#组件中执行Python脚本
    /// </summary>
    public class PythonScriptExecutor
    {
        private static string _pythonPath = null;
        private static string _scriptBasePath = null;

        /// <summary>
        /// 获取Python可执行文件路径
        /// 排除Rhino的Python环境，使用系统Python
        /// </summary>
        public static string GetPythonPath()
        {
            if (_pythonPath != null && File.Exists(_pythonPath))
                return _pythonPath;

            // 尝试从环境变量获取
            string pythonEnv = Environment.GetEnvironmentVariable("PYTHON_PATH");
            if (!string.IsNullOrEmpty(pythonEnv) && File.Exists(pythonEnv))
            {
                // 如果用户明确设置了PYTHON_PATH，使用它
                _pythonPath = pythonEnv;
                return _pythonPath;
            }
            
            // 优先查找Rhino的Python环境
            string[] rhinoPythonPaths = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rhinocode", "py39-rh8", "python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rhinocode", "py310-rh8", "python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rhinocode", "py311-rh8", "python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rhinocode", "py312-rh8", "python.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".rhinocode", "py313-rh8", "python.exe"),
                @"C:\Users\Administrator\.rhinocode\py39-rh8\python.exe",
                @"C:\Users\Administrator\.rhinocode\py310-rh8\python.exe",
                @"C:\Users\Administrator\.rhinocode\py311-rh8\python.exe",
                @"C:\Users\Administrator\.rhinocode\py312-rh8\python.exe",
                @"C:\Users\Administrator\.rhinocode\py313-rh8\python.exe"
            };
            
            foreach (string path in rhinoPythonPaths)
            {
                if (File.Exists(path))
                {
                    _pythonPath = path;
                    return _pythonPath;
                }
            }
            
            // 如果找不到Rhino Python，尝试查找系统Python
            string[] systemPythonPaths = {
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Programs\Python\Python313\python.exe",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Programs\Python\Python312\python.exe",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Programs\Python\Python311\python.exe",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Programs\Python\Python310\python.exe",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Programs\Python\Python39\python.exe",
                @"C:\Python313\python.exe",
                @"C:\Python312\python.exe",
                @"C:\Python311\python.exe",
                @"C:\Python310\python.exe",
                @"C:\Python39\python.exe"
            };
            
            foreach (string path in systemPythonPaths)
            {
                if (File.Exists(path))
                {
                    _pythonPath = path;
                    return _pythonPath;
                }
            }

            // 尝试常见路径（排除Rhino路径）
            string[] commonPaths = {
                @"C:\Python39\python.exe",
                @"C:\Python310\python.exe",
                @"C:\Python311\python.exe",
                @"C:\Python312\python.exe",
                @"C:\Program Files\Python39\python.exe",
                @"C:\Program Files\Python310\python.exe",
                @"C:\Program Files\Python311\python.exe",
                @"C:\Program Files\Python312\python.exe",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Programs\Python\Python39\python.exe",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Programs\Python\Python310\python.exe",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Programs\Python\Python311\python.exe",
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Programs\Python\Python312\python.exe"
            };

            foreach (string path in commonPaths)
            {
                if (File.Exists(path))
                {
                    // 确保不是Rhino的Python环境
                    if (!path.Contains(".rhinocode") && !path.Contains("rhinocode"))
                    {
                        _pythonPath = path;
                        return _pythonPath;
                    }
                }
            }

            // 最后尝试从PATH查找，但排除Rhino路径
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (Process p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    string error = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    
                    if (p.ExitCode == 0)
                    {
                        // 检查python.exe的实际路径
                        string pythonFullPath = GetPythonFullPath();
                        if (!string.IsNullOrEmpty(pythonFullPath) && 
                            !pythonFullPath.Contains(".rhinocode") && 
                            !pythonFullPath.Contains("rhinocode"))
                        {
                            _pythonPath = pythonFullPath;
                            return _pythonPath;
                        }
                    }
                }
            }
            catch { }

            throw new Exception("未找到系统Python安装。请确保Python已安装并添加到PATH，或设置PYTHON_PATH环境变量指向系统Python（不是Rhino的Python）。");
        }

        /// <summary>
        /// 获取python命令的完整路径
        /// </summary>
        private static string GetPythonFullPath()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "where",
                    Arguments = "python",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };
                using (Process p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    
                    if (p.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        string[] paths = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string path in paths)
                        {
                            string trimmedPath = path.Trim();
                            if (!string.IsNullOrEmpty(trimmedPath) && 
                                File.Exists(trimmedPath) &&
                                !trimmedPath.Contains(".rhinocode") && 
                                !trimmedPath.Contains("rhinocode"))
                            {
                                return trimmedPath;
                            }
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// 设置脚本基础路径
        /// </summary>
        public static void SetScriptBasePath(string path)
        {
            _scriptBasePath = path;
        }

        /// <summary>
        /// 执行Python脚本
        /// </summary>
        public static string ExecuteScript(string scriptPath, string arguments = "", int timeout = 30000)
        {
            string pythonPath = GetPythonPath();
            string fullScriptPath = Path.IsPathRooted(scriptPath) 
                ? scriptPath 
                : Path.Combine(_scriptBasePath ?? Directory.GetCurrentDirectory(), scriptPath);

            if (!File.Exists(fullScriptPath))
                throw new FileNotFoundException($"Python脚本未找到: {fullScriptPath}");

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"\"{fullScriptPath}\" {arguments}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (!process.WaitForExit(timeout))
                {
                    process.Kill();
                    throw new TimeoutException($"Python脚本执行超时: {fullScriptPath}");
                }

                if (process.ExitCode != 0)
                {
                    // 合并stdout和stderr的错误信息
                    string combinedError = string.IsNullOrEmpty(error) ? output : $"{error}\n\n标准输出:\n{output}";
                    throw new Exception($"Python脚本执行失败:\n{combinedError}");
                }

                if (!string.IsNullOrEmpty(error))
                {
                    RhinoApp.WriteLine($"Python警告: {error}");
                }

                return output;
            }
        }

        /// <summary>
        /// 执行Python代码字符串
        /// </summary>
        public static string ExecuteCode(string pythonCode, string arguments = "", int timeout = 30000)
        {
            string pythonPath = GetPythonPath();
            string tempScript = Path.Combine(Path.GetTempPath(), $"simpleml_{Guid.NewGuid()}.py");

            try
            {
                File.WriteAllText(tempScript, pythonCode, Encoding.UTF8);
                return ExecuteScript(tempScript, arguments, timeout);
            }
            finally
            {
                if (File.Exists(tempScript))
                {
                    try { File.Delete(tempScript); } catch { }
                }
            }
        }
    }
}
