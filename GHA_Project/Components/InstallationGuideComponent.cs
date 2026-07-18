using System;
using Grasshopper.Kernel;
using SimpleML.Core;

namespace SimpleML.Components.About
{
    /// <summary>
    /// 安装指南：Rhino 7+ / Windows / macOS
    /// </summary>
    public class InstallationGuideComponent : GH_Component
    {
        public InstallationGuideComponent()
          : base("安装指南 Installation Guide", "安装指南",
              "SimpleML 跨版本安装指南（Rhino 7 及以上，Windows 与 macOS）",
              "SimpleML", "09 Help")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Plugin Installation", "PI", "插件安装说明", GH_ParamAccess.item);
            pManager.AddTextParameter("Python Requirements", "PR", "Python依赖库要求", GH_ParamAccess.item);
            pManager.AddTextParameter("Installation Steps", "IS", "详细安装步骤", GH_ParamAccess.item);
            pManager.AddTextParameter("Troubleshooting", "T", "常见问题排查", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string platform = RhinoCompat.DescribeCompatibility();

            string pluginInstallation = $@"SimpleML 安装说明（Rhino 7+ / Windows / macOS）
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
• SIMPLEML_TIMEOUT_MS = 超时毫秒（默认 120000）
• SIMPLEML_PERSISTENT_PYTHON = 1/0";

            string pythonRequirements = @"Python 依赖（Rhino 7 / 8 / Mac 通用）
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

            string installationSteps = @"推荐上手（5 分钟）
═══════════════════════════════════════════════════════════════

1) 安装 Python 3.9+ 与依赖（见上）
2) 放置 SimpleML.gha + myML，重启 Rhino
3) 运行「环境体检」，确认 PASS 与平台信息
4) 按 examples/README.md 连接：
   加载数据集 → 创建数据集 → 智能训练 → 预测/评估

编译（开发者）
• 默认自动优先引用 Rhino 7（向前兼容）:
    dotnet build GHA_Project/SimpleML.csproj -c Release
• 指定版本:
    /p:RhinoMajorVersion=7
    /p:RhinoMajorVersion=8
• 或手动:
    /p:RhinoSystemDir=... /p:RhinoGrasshopperDir=...";

            string troubleshooting = @"常见问题
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

6) 找不到 myML
   → 设置 SIMPLEML_PATH，或把包放到 .gha 同级";

            DA.SetData(0, pluginInstallation);
            DA.SetData(1, pythonRequirements);
            DA.SetData(2, installationSteps);
            DA.SetData(3, troubleshooting);
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(InstallationGuideComponent));
        public override Guid ComponentGuid => new Guid("A2B3C4D5-E6F7-8901-ABCD-EF1234567890");
    }
}
