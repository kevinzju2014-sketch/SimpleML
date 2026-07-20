using System;
using System.Text;

namespace SimpleML.Core
{
    /// <summary>
    /// 统一生成 Python 引导代码与超时配置，减少组件内复制粘贴。
    /// </summary>
    public static class PythonBridge
    {
        public static int DefaultTimeoutMs
        {
            get
            {
                string env = Environment.GetEnvironmentVariable("SIMPLEML_TIMEOUT_MS");
                if (int.TryParse(env, out int ms) && ms > 1000)
                    return ms;
                return 120000; // 默认 120 秒，适应训练/可视化
            }
        }

        /// <summary>
        /// 跨平台 site-packages / 项目路径引导（写入临时脚本头部）。
        /// </summary>
        public static string BuildBootstrap(string mymlPath)
        {
            string escaped = (mymlPath ?? "").Replace("\\", "\\\\").Replace("'", "\\'");
            var sb = new StringBuilder();
            sb.AppendLine("# -*- coding: utf-8 -*-");
            sb.AppendLine("import sys, os, io, site");
            sb.AppendLine("if hasattr(sys.stdout, 'buffer'):");
            sb.AppendLine("    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')");
            sb.AppendLine("if hasattr(sys.stderr, 'buffer'):");
            sb.AppendLine("    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')");
            sb.AppendLine($"sys.path.insert(0, r'{escaped}')");
            sb.AppendLine("os.environ['SIMPLEML_PATH'] = r'" + escaped + "'");
            // Keep Python verdicts / explanations in sync with UI language.
            string lang = "en";
            try { lang = SimpleML.Localization.UiLanguage.Current; } catch { }
            sb.AppendLine("os.environ['SIMPLEML_LANG'] = r'" + lang.Replace("'", "") + "'");
            sb.AppendLine("try:");
            sb.AppendLine("    from core.env_bootstrap import bootstrap_python_paths");
            sb.AppendLine($"    bootstrap_python_paths(r'{escaped}')");
            sb.AppendLine("except Exception:");
            sb.AppendLine("    try:");
            sb.AppendLine("        for sp in site.getsitepackages():");
            sb.AppendLine("            if sp not in sys.path and os.path.isdir(sp):");
            sb.AppendLine("                sys.path.insert(0, sp)");
            sb.AppendLine("        from pathlib import Path");
            sb.AppendLine("        home = Path.home()");
            sb.AppendLine("        roots = [home / '.rhinocode', home / 'Library' / 'Application Support' / 'McNeel' / 'Rhinoceros' / '.rhinocode']");
            sb.AppendLine("        for root in roots:");
            sb.AppendLine("            if not root.exists():");
            sb.AppendLine("                continue");
            sb.AppendLine("            for child in root.iterdir():");
            sb.AppendLine("                if not child.is_dir():");
            sb.AppendLine("                    continue");
            sb.AppendLine("                envs = child / 'site-envs'");
            sb.AppendLine("                targets = [envs] if envs.is_dir() else []");
            sb.AppendLine("                for t in targets:");
            sb.AppendLine("                    for item in t.iterdir():");
            sb.AppendLine("                        if item.is_dir() and str(item) not in sys.path:");
            sb.AppendLine("                            sys.path.insert(0, str(item))");
            sb.AppendLine("                            for sub in [item / 'Lib' / 'site-packages', item / 'lib' / 'site-packages']:");
            sb.AppendLine("                                if sub.is_dir() and str(sub) not in sys.path:");
            sb.AppendLine("                                    sys.path.insert(0, str(sub))");
            sb.AppendLine("    except Exception:");
            sb.AppendLine("        pass");
            return sb.ToString();
        }

        public static string EscapeForTripleSingleQuotes(string value)
        {
            if (value == null) return "";
            return value.Replace("\\", "\\\\").Replace("'''", "\\'\\'\\'");
        }

        public static string ToPythonStringLiteral(string str)
        {
            if (string.IsNullOrEmpty(str))
                return "''";
            var sb = new StringBuilder();
            sb.Append("r'''");
            sb.Append(EscapeForTripleSingleQuotes(str));
            sb.Append("'''");
            return sb.ToString();
        }

        public static string ExtractValue(string output, string prefix)
        {
            int startIndex = output.IndexOf(prefix, StringComparison.Ordinal);
            if (startIndex == -1) return string.Empty;
            startIndex += prefix.Length;

            int endIndex = output.Length;
            for (int i = startIndex; i < output.Length - 7; i++)
            {
                if (string.Compare(output, i, "OUTPUT_", 0, 7, StringComparison.Ordinal) == 0)
                {
                    endIndex = i;
                    break;
                }
            }
            return output.Substring(startIndex, endIndex - startIndex).Trim();
        }
    }
}
