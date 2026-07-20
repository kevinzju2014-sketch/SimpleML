using System;
using Grasshopper.Kernel;
using SimpleML.Core;
using Rhino;

using SimpleML.Localization;
namespace SimpleML.Components.About
{
    public class HealthCheckComponent : GH_Component
    {
        public HealthCheckComponent()
          : base(L.Name("HealthCheckComponent"), L.Nick("HealthCheckComponent"), L.Desc("HealthCheckComponent"),
              "SimpleML", "09 Help")
        {
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "设为 true 执行体检", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("AutoFix", "Fix", "缺失依赖时尝试自动 pip 安装（需网络）", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Status", "S", "PASS 或 FAIL", GH_ParamAccess.item);
            pManager.AddTextParameter("Summary", "Sum", "简要摘要", GH_ParamAccess.item);
            pManager.AddTextParameter("Next Steps", "Next", "失败时的下一步（请接 Panel）", GH_ParamAccess.item);
            pManager.AddTextParameter("Report", "Rep", "完整体检报告", GH_ParamAccess.item);
            pManager.AddTextParameter("Python", "Py", "Python 路径", GH_ParamAccess.item);
            pManager.AddTextParameter("Package Root", "Path", "包根目录", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = true;
            bool autoFix = false;
            DA.GetData(0, ref run);
            DA.GetData(1, ref autoFix);
            if (!run)
            {
                DA.SetData(0, "SKIP");
                DA.SetData(1, "未运行（Run=false）");
                DA.SetData(2, "将 Run 设为 true。上架前请确认 PASS。");
                return;
            }

            try
            {
                string mymlPath = PathResolver.GetMyMLPath();
                string pythonPath = "";
                try { pythonPath = PythonScriptExecutor.GetPythonPath(); }
                catch (Exception ex) { pythonPath = "未找到: " + ex.Message; }

                if (string.IsNullOrEmpty(mymlPath))
                {
                    string next =
                        "下一步:\n" +
                        "1) 打开「安装指南」组件\n" +
                        "2) 或运行仓库 install.sh / install.bat\n" +
                        "3) 将 SimpleML.gha 与 myML 放到 Grasshopper Libraries\n" +
                        "4) 重启 Rhino 后再体检\n" +
                        RhinoCompat.DescribeCompatibility();
                    DA.SetData(0, "FAIL");
                    DA.SetData(1, "未找到 SimpleML Python 包根目录");
                    DA.SetData(2, next);
                    DA.SetData(3, next + "\nPython: " + pythonPath);
                    DA.SetData(4, pythonPath);
                    DA.SetData(5, "");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "未找到包路径 → 请看 Next Steps / 安装指南");
                    return;
                }

                string code = PythonBridge.BuildBootstrap(mymlPath) + @"
import json
from components.health_check import run_health_check
result = run_health_check(r'''" + mymlPath.Replace("\\", "\\\\") + @"''', auto_fix=" + (autoFix ? "True" : "False") + @")
print('OUTPUT_0:' + result.get('status',''))
print('OUTPUT_1:' + result.get('summary',''))
print('OUTPUT_2:' + result.get('next_steps','').replace('\n','\\n'))
print('OUTPUT_3:' + result.get('report','').replace('\n','\\n'))
print('OUTPUT_4:' + result.get('python',''))
print('OUTPUT_5:' + (result.get('project_dir') or ''))
";
                string output = PythonScriptExecutor.ExecuteCode(code, timeout: PythonBridge.DefaultTimeoutMs);
                string status = PythonBridge.ExtractValue(output, "OUTPUT_0:");
                string summary = PythonBridge.ExtractValue(output, "OUTPUT_1:");
                string nextSteps = PythonBridge.ExtractValue(output, "OUTPUT_2:").Replace("\\n", "\n");
                string report = PythonBridge.ExtractValue(output, "OUTPUT_3:").Replace("\\n", "\n");
                string py = PythonBridge.ExtractValue(output, "OUTPUT_4:");
                string root = PythonBridge.ExtractValue(output, "OUTPUT_5:");

                string compat = RhinoCompat.DescribeCompatibility();
                summary = compat + " | " + summary;
                report = compat + "\n" + report;
                if (status != "PASS")
                    nextSteps = nextSteps + "\n\n也可搜索打开「安装指南」「新手向导」。";

                DA.SetData(0, status);
                DA.SetData(1, summary);
                DA.SetData(2, nextSteps);
                DA.SetData(3, report);
                DA.SetData(4, string.IsNullOrEmpty(py) ? pythonPath : py);
                DA.SetData(5, string.IsNullOrEmpty(root) ? mymlPath : root);

                if (status != "PASS")
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "体检未通过 → 请阅读 Next Steps 输出");
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "体检 PASS，可开始智能训练");
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, ex.Message);
                RhinoApp.WriteLine("SimpleML HealthCheck: " + ex);
                DA.SetData(0, "FAIL");
                DA.SetData(1, ex.Message);
                DA.SetData(2, "打开「安装指南」；确认 PYTHON_PATH；或运行 install 脚本。");
                DA.SetData(3, ex.ToString());
            }
        }

        protected override System.Drawing.Bitmap Icon => IconLoader.LoadComponentIcon(nameof(HealthCheckComponent));
        public override Guid ComponentGuid => new Guid("A0B1C2D3-E4F5-6789-ABCD-101112131415");
    }
}
