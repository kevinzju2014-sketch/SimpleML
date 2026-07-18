using System;
using Grasshopper.Kernel;
using SimpleML.Core;

namespace SimpleML.Components.About
{
    /// <summary>
    /// Installation Guide Component
    /// 安装指南组件 - 提供插件安装和依赖库安装建议（跨平台）
    /// </summary>
    public class InstallationGuideComponent : GH_Component
    {
        public InstallationGuideComponent()
          : base("安装指南 Installation Guide", "安装指南",
              "提供SimpleML插件的跨平台安装指南和Python依赖库安装建议",
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
            string pluginInstallation = @"SimpleML 插件安装说明（跨平台）
═══════════════════════════════════════════════════════════════

1. 需要放置的文件
   • SimpleML.gha（Grasshopper 插件）
   • myML/ 文件夹（内含 components/ 与 core/）

2. 推荐安装位置
   Windows:
   • %APPDATA%\Grasshopper\Libraries\SimpleML\
   macOS:
   • ~/Library/Application Support/McNeel/Rhinoceros/8.0/Plug-ins/Grasshopper/Libraries/SimpleML/
   也可把 myML 放在与 .gha 同一目录。

3. 安装步骤
   a) 关闭 Rhino / Grasshopper
   b) 复制 SimpleML.gha 与 myML 到上述目录
   c) 重启 Rhino / Grasshopper
   d) 在组件面板中应看到 SimpleML 标签页

4. 验证
   • 搜索「环境体检」或「读取CSV」
   • 运行「环境体检 Health Check」组件，状态应为 PASS

5. 可选环境变量
   • SIMPLEML_PATH = myML 根目录（含 components 与 core）
   • PYTHON_PATH = python/python3 可执行文件
   • SIMPLEML_TIMEOUT_MS = 超时毫秒（默认 120000）
   • SIMPLEML_PERSISTENT_PYTHON = 1/0（常驻 Python 会话，默认开）";

            string pythonRequirements = @"Python 依赖要求
═══════════════════════════════════════════════════════════════

必需:
• Python 3.9+（推荐 3.9–3.12）
• scikit-learn >= 1.0
• numpy >= 1.20
• pandas >= 1.3
• joblib >= 1.0

可选:
• openpyxl >= 3.0（Excel .xlsx）

安装（请对「Grasshopper 实际调用的同一个 Python」执行）:
  python -m pip install -r requirements.txt
  # 或
  python3 -m pip install scikit-learn numpy pandas joblib openpyxl

Windows 若使用 Rhino Code Python:
  通常位于 %USERPROFILE%\.rhinocode\py39-rh8\python.exe
macOS/Linux:
  使用 python3，或设置 PYTHON_PATH";

            string installationSteps = @"推荐上手路径（5 分钟）
═══════════════════════════════════════════════════════════════

1) 安装依赖（见 Python Requirements）
2) 放置 .gha + myML，重启 Rhino
3) 放入「环境体检」组件，Run=true，确认 PASS
4) 打开 examples/ 中的示例说明，按线连接：
   加载数据集 → 创建数据集 → 智能训练 → 预测/评估
5) 进阶调参时再用 05 Algorithm 参数电池 → Train *

新手优先使用:
• 智能训练 Smart Train（04 Model）
• 环境体检 Health Check（09 Help）";

            string troubleshooting = @"常见问题
═══════════════════════════════════════════════════════════════

1) 找不到 myML
   • 确认目录含 components/ 与 core/
   • 设置 SIMPLEML_PATH
   • 运行「环境体检」查看 Package Root

2) ModuleNotFoundError
   • 对当前 Python 执行 pip 安装
   • 用「环境体检」查看 Python 路径是否一致

3) 训练超时
   • 增大 SIMPLEML_TIMEOUT_MS（如 300000）
   • 或减少数据量 / 树数量

4) 预测结果异常（做了标准化）
   • v1.1 起预处理会随模型打包；请用同一流程重新训练

5) macOS 权限 / Gatekeeper
   • 允许 Rhino 运行未签名插件，或右键打开 .gha 所在目录后重试

6) 组件没有图标文字
   • 确认使用本版本 icons 资源重新编译的 SimpleML.gha";

            DA.SetData(0, pluginInstallation);
            DA.SetData(1, pythonRequirements);
            DA.SetData(2, installationSteps);
            DA.SetData(3, troubleshooting);
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(InstallationGuideComponent));
        public override Guid ComponentGuid => new Guid("A2B3C4D5-E6F7-8901-ABCD-EF1234567890");
    }
}
