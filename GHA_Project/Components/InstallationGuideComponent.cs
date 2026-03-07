using System;
using Grasshopper.Kernel;
using SimpleML.Core;

namespace SimpleML.Components.About
{
    /// <summary>
    /// Installation Guide Component
    /// 安装指南组件 - 提供插件安装和依赖库安装建议
    /// </summary>
    public class InstallationGuideComponent : GH_Component
    {
        public InstallationGuideComponent()
          : base("Installation Guide", "InstallGuide",
              "提供SimpleML插件的安装指南和Python依赖库安装建议",
              "SimpleML", "07 others")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            // 无输入参数
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
            // 插件安装说明
            string pluginInstallation = @"SimpleML插件安装说明
═══════════════════════════════════════════════════════════════

1. 安装位置
   • 将SimpleML.gha文件复制到Grasshopper的组件文件夹
   • 默认位置: C:\Users\[用户名]\AppData\Roaming\Grasshopper\UserObjects\
   • 或者: Grasshopper安装目录\Libraries\

2. 安装步骤
   a) 关闭Rhino和Grasshopper
   b) 复制SimpleML.gha文件到上述位置
   c) 复制myML文件夹到相同位置（如果未自动复制）
   d) 重新启动Rhino和Grasshopper
   e) 在Grasshopper中应该能看到SimpleML标签页

3. 验证安装
   • 在Grasshopper中搜索""Read CSV""组件
   • 如果能看到组件，说明安装成功
   • 如果看不到，检查GHA文件是否在正确位置

4. 卸载方法
   • 删除SimpleML.gha文件
   • 删除myML文件夹（可选）
   • 重新启动Grasshopper";

            // Python依赖库要求
            string pythonRequirements = @"Python依赖库要求
═══════════════════════════════════════════════════════════════

必需库（必须安装）:
• pandas >= 1.3.0        - 数据处理
• numpy >= 1.20.0         - 数值计算
• scikit-learn >= 1.0.0   - 机器学习算法

可选库（推荐安装）:
• openpyxl >= 3.0.0       - Excel文件支持（.xlsx）
• xlrd >= 2.0.0           - Excel文件支持（.xls）

Python版本要求:
• Python 3.7 或更高版本
• 推荐使用 Python 3.9 或 3.10

安装命令（在Rhino Python环境中）:
pip install pandas numpy scikit-learn openpyxl

注意: 如果使用Rhino内置的Python环境，可能需要管理员权限";

            // 详细安装步骤
            string installationSteps = @"详细安装步骤
═══════════════════════════════════════════════════════════════

步骤1: 安装Python依赖库
─────────────────────────────────────────────────────────────
方法A: 使用Rhino Python命令行
1. 打开Rhino
2. 在命令行输入: _Python
3. 输入以下命令:
   import subprocess
   subprocess.check_call(['pip', 'install', 'pandas', 'numpy', 'scikit-learn', 'openpyxl'])

方法B: 使用系统Python（如果Rhino使用系统Python）
1. 打开命令提示符（CMD）或PowerShell
2. 确保Python在PATH中
3. 运行: pip install pandas numpy scikit-learn openpyxl

方法C: 使用Rhino的包管理器（如果可用）
1. 在Rhino中打开Python编辑器
2. 使用包管理器安装依赖

步骤2: 验证Python库安装
─────────────────────────────────────────────────────────────
在Rhino Python中运行:
import pandas
import numpy
import sklearn
print('所有库已成功安装')

步骤3: 设置环境变量（可选）
─────────────────────────────────────────────────────────────
如果myML文件夹不在默认位置，设置环境变量:
变量名: SIMPLEML_PATH
变量值: myML文件夹的完整路径
例如: D:\Projects\SimpleML\myML

步骤4: 测试插件
─────────────────────────────────────────────────────────────
1. 在Grasshopper中创建新文件
2. 添加""Load Dataset""组件
3. 设置Dataset Name为""iris""
4. 如果能够成功加载数据，说明安装成功";

            // 常见问题排查
            string troubleshooting = @"常见问题排查
═══════════════════════════════════════════════════════════════

问题1: 组件无法找到myML文件夹
─────────────────────────────────────────────────────────────
解决方案:
• 检查myML文件夹是否在正确位置
• 设置SIMPLEML_PATH环境变量指向myML文件夹
• 确保myML文件夹包含components和core子文件夹

问题2: Python库导入错误（ModuleNotFoundError）
─────────────────────────────────────────────────────────────
解决方案:
• 确认已安装所有必需的Python库
• 检查Python版本是否兼容（需要3.7+）
• 如果使用虚拟环境，确保Rhino使用的是正确的Python环境
• 尝试重新安装库: pip install --upgrade pandas numpy scikit-learn

问题3: Excel文件读取失败
─────────────────────────────────────────────────────────────
解决方案:
• 安装openpyxl库: pip install openpyxl
• 对于.xls文件，安装xlrd: pip install xlrd
• 检查Excel文件是否损坏
• 确保文件路径正确

问题4: 模型训练失败
─────────────────────────────────────────────────────────────
解决方案:
• 检查数据格式是否正确（Tree结构）
• 确认数据没有缺失值或已正确处理
• 检查算法参数是否合理
• 查看错误信息中的详细提示

问题5: 组件在Grasshopper中不显示
─────────────────────────────────────────────────────────────
解决方案:
• 确认GHA文件在正确位置
• 检查GHA文件是否损坏
• 重新启动Rhino和Grasshopper
• 检查Grasshopper版本是否兼容

问题6: 中文显示乱码
─────────────────────────────────────────────────────────────
解决方案:
• 确保文件编码为UTF-8
• 检查系统区域设置
• 在Read CSV/Excel组件中使用UTF-8编码";

            DA.SetData(0, pluginInstallation);
            DA.SetData(1, pythonRequirements);
            DA.SetData(2, installationSteps);
            DA.SetData(3, troubleshooting);
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(InstallationGuideComponent));
        public override Guid ComponentGuid => new Guid("A2B3C4D5-E6F7-8901-ABCD-EF1234567890");
    }
}
