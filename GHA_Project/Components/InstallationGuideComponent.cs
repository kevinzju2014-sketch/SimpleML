using System;
using Grasshopper.Kernel;
using SimpleML.Core;
using SimpleML.Localization;

namespace SimpleML.Components.About
{
    /// <summary>
    /// Installation guide for Rhino 7+ / Windows / macOS (EN/ZH).
    /// </summary>
    public class InstallationGuideComponent : GH_Component
    {
        public InstallationGuideComponent()
          : base(L.Name(nameof(InstallationGuideComponent)),
                 L.Nick(nameof(InstallationGuideComponent)),
                 L.Desc(nameof(InstallationGuideComponent)),
                 "SimpleML", "09 Help")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Plugin Installation", "PI", "Install notes", GH_ParamAccess.item);
            pManager.AddTextParameter("Python Requirements", "PR", "Python requirements", GH_ParamAccess.item);
            pManager.AddTextParameter("Installation Steps", "IS", "Step-by-step setup", GH_ParamAccess.item);
            pManager.AddTextParameter("Troubleshooting", "T", "Troubleshooting", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string platform = RhinoCompat.DescribeCompatibility();
            if (UiLanguage.IsChinese)
            {
                DA.SetData(0, ZhInstall(platform));
                DA.SetData(1, ZhPython());
                DA.SetData(2, ZhSteps());
                DA.SetData(3, ZhTrouble());
            }
            else
            {
                DA.SetData(0, EnInstall(platform));
                DA.SetData(1, EnPython());
                DA.SetData(2, EnSteps());
                DA.SetData(3, EnTrouble());
            }
        }

        static string EnInstall(string platform) => $@"SimpleML Install Notes (Rhino 7+ / Windows / macOS)
═══════════════════════════════════════════════════════════════
Current environment: {platform}

Compatibility
• Rhino 7 and newer (including Rhino 8+)
• Windows and macOS
• One .gha build (prefer Rhino 7 refs for forward compatibility)

Files to place
• SimpleML.gha
• Python package root (components/ + core/), often named myML

Recommended location
Windows (shared across Rhino versions):
  %APPDATA%\Grasshopper\Libraries\SimpleML\
macOS (per major Rhino version):
  ~/Library/Application Support/McNeel/Rhinoceros/7.0/Plug-ins/Grasshopper/Libraries/SimpleML/
  ~/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper/Libraries/SimpleML/
You may also keep myML next to the .gha.

Verify
• Search Health Check → Run=true → Status PASS
• SimpleML tab should appear on the ribbon

Optional environment variables
• SIMPLEML_PATH = Python package root
• PYTHON_PATH = python executable
• SIMPLEML_LANG = en | zh
• SIMPLEML_TIMEOUT_MS = timeout ms (default 120000)
• SIMPLEML_PERSISTENT_PYTHON = 1/0";

        static string ZhInstall(string platform) => $@"SimpleML 安装说明（Rhino 7+ / Windows / macOS）
═══════════════════════════════════════════════════════════════
当前环境: {platform}

兼容范围
• Rhino 7 及以上（含 Rhino 8、后续版本）
• Windows 与 macOS
• 同一套 .gha（建议用 Rhino 7 程序集编译以获得最大向前兼容）

需要放置的文件
• SimpleML.gha
• Python 包根目录（含 components/ 与 core/），可命名为 myML

推荐安装位置
Windows（各 Rhino 版本通常共用）:
  %APPDATA%\Grasshopper\Libraries\SimpleML\
macOS（按 Rhino 主版本）:
  ~/Library/Application Support/McNeel/Rhinoceros/7.0/Plug-ins/Grasshopper/Libraries/SimpleML/
  ~/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper/Libraries/SimpleML/
也可将 myML 与 .gha 放在同一目录。

验证
• 搜索「环境体检」→ Run=true → 状态 PASS
• 面板应出现 SimpleML 标签页

环境变量（可选）
• SIMPLEML_PATH = Python 包根目录
• PYTHON_PATH = python / python3 可执行文件
• SIMPLEML_LANG = en | zh
• SIMPLEML_TIMEOUT_MS = 超时毫秒（默认 120000）
• SIMPLEML_PERSISTENT_PYTHON = 1/0";

        static string EnPython() => @"Python requirements (Rhino 7 / 8 / Mac)
═══════════════════════════════════════════════════════════════

Required: Python 3.9+, plus
  scikit-learn / numpy / pandas / joblib
Optional: openpyxl (Excel)

Rhino 7 (Windows / macOS)
• Usually no Rhinocode CPython — install system Python or Homebrew python3
• Then: python3 -m pip install -r requirements.txt
• Optionally set PYTHON_PATH to that interpreter

Rhino 8+
• May use Rhinocode: ~/.rhinocode/py*-rh8/...
• Or keep using system / Homebrew Python

macOS tips
• Apple Silicon: /opt/homebrew/bin/python3
• Intel: /usr/local/bin/python3
• Install packages into the same Python Grasshopper actually calls";

        static string ZhPython() => @"Python 依赖（Rhino 7 / 8 / Mac 通用）
═══════════════════════════════════════════════════════════════

必需: Python 3.9+ ，以及
  scikit-learn / numpy / pandas / joblib
可选: openpyxl（Excel）

Rhino 7（Windows / macOS）
• 通常没有 Rhinocode CPython，请安装系统 Python 或 Homebrew python3
• 然后: python3 -m pip install -r requirements.txt
• 可用 PYTHON_PATH 指向该解释器

Rhino 8+
• 可使用 Rhinocode: ~/.rhinocode/py*-rh8/...
• 或继续使用系统 / Homebrew Python

macOS 提示
• Apple Silicon: /opt/homebrew/bin/python3
• Intel: /usr/local/bin/python3
• 对「Grasshopper 实际调用的同一个 Python」安装依赖";

        static string EnSteps() => @"Quick start (5 minutes)
═══════════════════════════════════════════════════════════════

1) Install Python 3.9+ and dependencies (above)
2) Place SimpleML.gha (+ package path / SIMPLEML_PATH), restart Rhino
3) Run Health Check and confirm PASS
4) Follow examples/README.md:
   Load Dataset → Split → Smart Train → Predict / Evaluate

Language
• Use Help → Language: Chinese=true/false, or right-click → English / 中文
• Canvas labels refresh automatically (no Rhino restart)

Build (developers)
• Prefer Rhino 7 refs:
    dotnet build GHA_Project/SimpleML.csproj -c Release
• Or: /p:RhinoMajorVersion=7|8";

        static string ZhSteps() => @"推荐上手（5 分钟）
═══════════════════════════════════════════════════════════════

1) 安装 Python 3.9+ 与依赖（见上）
2) 放置 SimpleML.gha（并设置 SIMPLEML_PATH），重启 Rhino
3) 运行「环境体检」，确认 PASS
4) 按 examples/README.md 连接：
   加载数据集 → 分割 → 智能训练 → 预测/评估

语言
• Help →「语言」：Chinese=true/false，或右键选择 English / 中文
• 画布组件名会自动刷新（无需重启 Rhino）

编译（开发者）
• 默认优先 Rhino 7:
    dotnet build GHA_Project/SimpleML.csproj -c Release
• 或: /p:RhinoMajorVersion=7|8";

        static string EnTrouble() => @"Troubleshooting
═══════════════════════════════════════════════════════════════

1) Rhino 7 cannot find Python
   → Install system/Homebrew python3 and set PYTHON_PATH

2) Rhino 8 packages installed in the wrong env
   → pip into Rhinocode or the PYTHON_PATH interpreter

3) Components missing on macOS
   → Confirm Libraries path for 7.0 or 8.0
   → Allow unsigned plugins if prompted

4) Rhino older than 7
   → Not supported; upgrade to Rhino 7+

5) Training timeout
   → Increase SIMPLEML_TIMEOUT_MS

6) Package root not found
   → Set SIMPLEML_PATH, or place components/core next to the .gha

7) Duplicate SimpleML.gha conflict
   → Keep only one .gha under Grasshopper Libraries (do not junction the whole repo)";

        static string ZhTrouble() => @"常见问题
═══════════════════════════════════════════════════════════════

1) Rhino 7 找不到 Python
   → 安装系统/Homebrew python3，并设置 PYTHON_PATH

2) Rhino 8 依赖装错环境
   → 对 Rhinocode 或 PYTHON_PATH 指向的解释器执行 pip

3) macOS 看不到组件
   → 确认 Libraries 路径对应 7.0 或 8.0
   → 系统设置中允许运行未签名插件（如有提示）

4) 版本低于 Rhino 7
   → 不受支持；请升级到 Rhino 7 及以上

5) 训练超时
   → 增大 SIMPLEML_TIMEOUT_MS

6) 找不到包路径
   → 设置 SIMPLEML_PATH，或把 components/core 放到 .gha 同级

7) 出现两个 SimpleML.gha 冲突
   → Libraries 下只保留一份 .gha（不要把整个仓库做成联接）";

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(InstallationGuideComponent));
        public override Guid ComponentGuid => new Guid("A2B3C4D5-E6F7-8901-ABCD-EF1234567890");
    }
}
