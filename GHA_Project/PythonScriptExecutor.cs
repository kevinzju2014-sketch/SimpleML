using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Rhino;

namespace SimpleML.Core
{
    /// <summary>
    /// 跨平台 Python 执行器。
    /// - 自动查找 Rhino / 系统 Python（Windows / macOS / Linux）
    /// - 支持常驻会话（stdin/stdout JSON 协议），降低每次冷启动成本
    /// - 超时可通过 SIMPLEML_TIMEOUT_MS 配置
    /// </summary>
    public class PythonScriptExecutor
    {
        private static string _pythonPath = null;
        private static string _scriptBasePath = null;
        private static readonly object _sessionLock = new object();
        private static Process _sessionProcess;
        private static StreamWriter _sessionStdin;
        private static StreamReader _sessionStdout;
        private static bool _usePersistentSession = true;

        static PythonScriptExecutor()
        {
            string flag = Environment.GetEnvironmentVariable("SIMPLEML_PERSISTENT_PYTHON");
            if (!string.IsNullOrEmpty(flag))
            {
                _usePersistentSession = !(flag == "0" || flag.Equals("false", StringComparison.OrdinalIgnoreCase));
            }
        }

        public static string GetPythonPath()
        {
            if (_pythonPath != null && File.Exists(_pythonPath))
                return _pythonPath;

            string pythonEnv = Environment.GetEnvironmentVariable("PYTHON_PATH");
            if (!string.IsNullOrEmpty(pythonEnv) && File.Exists(pythonEnv))
            {
                _pythonPath = pythonEnv;
                return _pythonPath;
            }

            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var candidates = new List<string>();

            // Rhino Code CPython（Rhino 8+；按当前主版本优先，并兼容 rh7/rh8/rh9…）
            foreach (string envDir in RhinoCompat.EnumerateRhinocodeEnvDirs())
            {
                string py = RhinoCompat.FindPythonInEnvDir(envDir);
                if (!string.IsNullOrEmpty(py))
                    candidates.Add(py);
            }

            // 系统 Python：Rhino 7 与 macOS 的主要来源
            if (RhinoCompat.IsWindows)
            {
                string local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string[] pyVers = { "Python313", "Python312", "Python311", "Python310", "Python39" };
                foreach (string v in pyVers)
                {
                    candidates.Add(Path.Combine(local, "Programs", "Python", v, "python.exe"));
                    candidates.Add(Path.Combine("C:\\", v, "python.exe"));
                    candidates.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), v, "python.exe"));
                }
            }
            else
            {
                // macOS / Unix：Homebrew（Apple Silicon + Intel）与常见路径
                candidates.Add("/opt/homebrew/bin/python3");
                candidates.Add("/usr/local/bin/python3");
                candidates.Add("/usr/bin/python3");
                candidates.Add("/usr/bin/python");
                candidates.Add(Path.Combine(home, "miniconda3", "bin", "python"));
                candidates.Add(Path.Combine(home, "anaconda3", "bin", "python"));
                candidates.Add(Path.Combine(home, "mambaforge", "bin", "python"));
            }

            foreach (string path in candidates)
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    _pythonPath = path;
                    return _pythonPath;
                }
            }

            // PATH 查找
            string fromPath = FindOnPath(RhinoCompat.IsWindows ? "python" : "python3")
                              ?? FindOnPath("python");
            if (!string.IsNullOrEmpty(fromPath))
            {
                _pythonPath = fromPath;
                return _pythonPath;
            }

            throw new Exception(
                "未找到 Python。请安装 Python 3.9+，或设置 PYTHON_PATH。\n" +
                "兼容：Rhino 7+（Windows / macOS）。\n" +
                "Rhino 8+：可使用 Rhinocode（~/.rhinocode/py*-rh*）。\n" +
                "Rhino 7 / macOS：推荐系统或 Homebrew 的 python3。\n" +
                "当前环境：" + RhinoCompat.DescribeCompatibility());
        }

        private static bool IsWindows()
        {
            return RhinoCompat.IsWindows;
        }

        private static string FindOnPath(string command)
        {
            try
            {
                string fileName = IsWindows() ? "where" : "which";
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };
                using (Process p = Process.Start(psi))
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit(5000);
                    if (p.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string line in lines)
                        {
                            string trimmed = line.Trim();
                            if (File.Exists(trimmed))
                                return trimmed;
                        }
                    }
                }
            }
            catch { }
            return null;
        }

        public static void SetScriptBasePath(string path)
        {
            _scriptBasePath = path;
        }

        public static string ExecuteScript(string scriptPath, string arguments = "", int timeout = -1)
        {
            if (timeout < 0) timeout = PythonBridge.DefaultTimeoutMs;
            string pythonPath = GetPythonPath();
            string fullScriptPath = Path.IsPathRooted(scriptPath)
                ? scriptPath
                : Path.Combine(_scriptBasePath ?? Directory.GetCurrentDirectory(), scriptPath);

            if (!File.Exists(fullScriptPath))
                throw new FileNotFoundException($"Python脚本未找到: {fullScriptPath}");

            var psi = new ProcessStartInfo
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
                    try { process.Kill(); } catch { }
                    throw new TimeoutException($"Python脚本执行超时({timeout}ms): {fullScriptPath}");
                }

                if (process.ExitCode != 0)
                {
                    string combinedError = string.IsNullOrEmpty(error) ? output : $"{error}\n\n标准输出:\n{output}";
                    throw new Exception($"Python脚本执行失败:\n{combinedError}");
                }

                if (!string.IsNullOrEmpty(error))
                    RhinoApp.WriteLine($"Python警告: {error}");

                return output;
            }
        }

        public static string ExecuteCode(string pythonCode, string arguments = "", int timeout = -1)
        {
            if (timeout < 0) timeout = PythonBridge.DefaultTimeoutMs;

            if (_usePersistentSession)
            {
                try
                {
                    return ExecuteCodePersistent(pythonCode, timeout);
                }
                catch (Exception ex)
                {
                    RhinoApp.WriteLine($"SimpleML: 常驻 Python 会话失败，回退到一次性进程: {ex.Message}");
                    ResetSession();
                }
            }

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

        private static string ExecuteCodePersistent(string pythonCode, int timeout)
        {
            lock (_sessionLock)
            {
                EnsureSession();

                string marker = Guid.NewGuid().ToString("N");
                // 协议: 发送一行 JSON，含 code + marker；宿主执行后打印 __SIMPLEML_DONE__{marker}
                string payload = "{\"marker\":\"" + marker + "\",\"code\":" + ToJsonString(pythonCode) + "}\n";
                _sessionStdin.Write(payload);
                _sessionStdin.Flush();

                var sb = new StringBuilder();
                string doneToken = "__SIMPLEML_DONE__" + marker;
                string errToken = "__SIMPLEML_ERROR__" + marker;
                var sw = Stopwatch.StartNew();

                while (sw.ElapsedMilliseconds < timeout)
                {
                    if (_sessionProcess.HasExited)
                    {
                        ResetSessionUnlocked();
                        throw new Exception("常驻 Python 进程已退出");
                    }

                    string line = _sessionStdout.ReadLine();
                    if (line == null)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    if (line.StartsWith(doneToken, StringComparison.Ordinal))
                        return sb.ToString();
                    if (line.StartsWith(errToken, StringComparison.Ordinal))
                    {
                        string err = line.Substring(errToken.Length);
                        throw new Exception("Python执行失败: " + err + "\n" + sb);
                    }
                    sb.AppendLine(line);
                }

                ResetSessionUnlocked();
                throw new TimeoutException($"常驻 Python 执行超时({timeout}ms)");
            }
        }

        private static void EnsureSession()
        {
            if (_sessionProcess != null && !_sessionProcess.HasExited && _sessionStdin != null && _sessionStdout != null)
                return;

            ResetSessionUnlocked();

            string hostScript = Path.Combine(Path.GetTempPath(), "simpleml_pyhost.py");
            File.WriteAllText(hostScript, BuildHostScript(), Encoding.UTF8);

            var psi = new ProcessStartInfo
            {
                FileName = GetPythonPath(),
                Arguments = "\"" + hostScript + "\"",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            _sessionProcess = Process.Start(psi);
            _sessionStdin = new StreamWriter(_sessionProcess.StandardInput.BaseStream, new UTF8Encoding(false)) { AutoFlush = true };
            _sessionStdout = new StreamReader(_sessionProcess.StandardOutput.BaseStream, Encoding.UTF8);

            // 读 ready
            string ready = _sessionStdout.ReadLine();
            if (ready == null || !ready.Contains("SIMPLEML_HOST_READY"))
                throw new Exception("Python 宿主未能启动: " + ready);
        }

        private static void ResetSession()
        {
            lock (_sessionLock)
            {
                ResetSessionUnlocked();
            }
        }

        /// <summary>供「重置Python会话」组件调用。</summary>
        public static void ResetSessionPublic()
        {
            ResetSession();
            _pythonPath = null;
        }

        private static void ResetSessionUnlocked()
        {
            try { _sessionStdin?.Dispose(); } catch { }
            try { _sessionStdout?.Dispose(); } catch { }
            try
            {
                if (_sessionProcess != null && !_sessionProcess.HasExited)
                    _sessionProcess.Kill();
            }
            catch { }
            try { _sessionProcess?.Dispose(); } catch { }
            _sessionStdin = null;
            _sessionStdout = null;
            _sessionProcess = null;
        }

        private static string BuildHostScript()
        {
            return @"# -*- coding: utf-8 -*-
import sys, json, traceback, io
if hasattr(sys.stdout, 'buffer'):
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace', line_buffering=True)
if hasattr(sys.stderr, 'buffer'):
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace', line_buffering=True)
print('SIMPLEML_HOST_READY', flush=True)
while True:
    line = sys.stdin.readline()
    if not line:
        break
    try:
        req = json.loads(line)
        marker = req.get('marker', '')
        code = req.get('code', '')
        buf = io.StringIO()
        old = sys.stdout
        try:
            sys.stdout = buf
            exec(compile(code, '<simpleml>', 'exec'), {})
        finally:
            sys.stdout = old
        out = buf.getvalue()
        if out:
            sys.stdout.write(out)
            if not out.endswith('\n'):
                sys.stdout.write('\n')
        print('__SIMPLEML_DONE__' + marker, flush=True)
    except Exception as e:
        err = traceback.format_exc().replace('\n', ' | ')
        print('__SIMPLEML_ERROR__' + marker + err, flush=True)
";
        }

        private static string ToJsonString(string s)
        {
            if (s == null) return "null";
            var sb = new StringBuilder();
            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 32)
                            sb.AppendFormat("\\u{0:x4}", (int)c);
                        else
                            sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }
    }
}
